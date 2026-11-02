Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls

Namespace SidhawanForStudents

    Public Enum SummaryComponent
        Attendance
        Quizzes
        ClassWork
        MajorExam
    End Enum

    Public Enum SummaryScope
        Midterm
        Finals
        Overall
    End Enum

    Public NotInheritable Class GradeSummaryService
        Private Sub New()
        End Sub

        ' Attendance log
        Private Const AttendanceLogSql As String =
            "SELECT AttendanceDate, UCASE(Status) AS Status " &
            "FROM vw_studentattendancesummary " &
            "WHERE StudentID = @sid AND ClassID = @cid " &
            "ORDER BY RawClassDate"

        ' Attendance total
        Private Const AttendanceTotalSql As String =
            "SELECT COUNT(*) AS Highest, " &
            "       SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) AS Acquired " &
            "FROM vw_studentattendancesummary " &
            "WHERE StudentID = @sid AND ClassID = @cid"

        ' Grading Component
        Private Const ComponentsSql As String =
            "SELECT GradingAspect, HighestPossibleScore, AcquiredScore, GradePercentage " &
            "FROM vw_studentcomponentsummary " &
            "WHERE StudentID = @sid AND ClassID = @cid"

        ' Class Items 
        Private Const ItemsSql As String =
            "SELECT ItemName, HighestScore, AcquiredScore " &
            "FROM vw_studentscores " &
            "WHERE StudentID = @sid AND ClassID = @cid " &
            "AND Term = @term " &
            "AND Component = @comp " &
            "ORDER BY ItemName"
        Private Const FinalSql As String =
            "SELECT MidtermGrade, FinalGrade " &
            "FROM vw_studentcoursegradesummary " &
            "WHERE StudentID = @sid AND ClassID = @cid LIMIT 1"

        Private Shared ReadOnly DefaultWeights As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
            {"Attendance", "5%"}
        }
        Private Shared _weights As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Private Shared Function TypeKey(c As SummaryComponent) As String
            Select Case c
                Case SummaryComponent.Quizzes : Return "Quizzes"
                Case SummaryComponent.ClassWork : Return "Class Work"
                Case SummaryComponent.MajorExam : Return "Major Examination"
                Case Else : Return "Attendance"
            End Select
        End Function

        Private Shared Function DatabaseComponentKey(c As SummaryComponent) As String
            If c = SummaryComponent.ClassWork Then Return "Classwork"
            Return TypeKey(c)
        End Function
        Public Shared Sub LoadWeights(classId As String)
            _weights.Clear()
            If Not Session.IsLoggedIn OrElse String.IsNullOrWhiteSpace(classId) Then Return
            Safe(Sub()
                     Dim rows = Db.Query(ComponentsSql,
                         Function(r) (Aspect:=r.Str("GradingAspect"), Pct:=r.Str("GradePercentage")),
                         ("@sid", Session.UserId), ("@cid", classId))
                     For Each x In rows
                         _weights(x.Aspect) = NormalizePct(x.Pct)
                     Next
                 End Sub)
        End Sub

        Private Shared Function NormalizePct(p As String) As String
            If String.IsNullOrWhiteSpace(p) Then Return ""
            p = p.Trim()
            Return If(p.EndsWith("%"), p, p & "%")
        End Function

        Private Shared Function WeightOf(aspect As String) As String
            Dim w As String = Nothing
            If _weights.TryGetValue(aspect, w) Then Return w
            If DefaultWeights.TryGetValue(aspect, w) Then Return w

            Dim normalizedAspect = aspect.Replace(" ", "")
            For Each entry In _weights
                If String.Equals(entry.Key.Replace(" ", ""), normalizedAspect, StringComparison.OrdinalIgnoreCase) Then
                    Return entry.Value
                End If
            Next

            Return ""
        End Function

        Private Shared Function DisplayAspect(aspect As String) As String
            If String.Equals(aspect, "Classwork", StringComparison.OrdinalIgnoreCase) Then
                Return "Class Work"
            End If
            Return aspect
        End Function
        Public Shared Function BandText(component As SummaryComponent, scope As SummaryScope) As String
            If scope = SummaryScope.Overall Then Return "Overall Summary"
            Dim key = TypeKey(component)
            Dim w = WeightOf(key)
            Return (If(w = "", key, key & " (" & w & ")")).ToUpperInvariant()
        End Function

        ' ---------- public entry point ----------
        Public Shared Sub Load(classId As String, component As SummaryComponent, scope As SummaryScope)
            If Not Session.IsLoggedIn OrElse String.IsNullOrWhiteSpace(classId) Then Return

            Clear()
            Safe(Sub()
                     If scope = SummaryScope.Overall Then
                         LoadOverall(classId)
                     ElseIf component = SummaryComponent.Attendance Then
                         LoadAttendance(classId)
                     Else
                         LoadComponent(classId, component, scope)
                     End If
                 End Sub)
        End Sub

        Public Shared Sub Clear()
            Data.AttendanceLog.Clear()
            Data.StudentScores.Clear()
            Data.Aspects.Clear()
            Data.FinalGrades.Clear()
        End Sub

        ' Loading of Attendance
        Private Shared Sub LoadAttendance(classId As String)
            Dim rows = Db.Query(AttendanceLogSql,
                Function(r) New AttendanceLogRow With {.[Date] = r.Str("AttendanceDate"), .Status = r.Str("Status")},
                ("@sid", Session.UserId), ("@cid", classId))
            For Each row In rows
                Data.AttendanceLog.Add(row)
            Next
        End Sub

        ' Loading of Quizzes / Class Work / Major Examination
        Private Shared Sub LoadComponent(classId As String, component As SummaryComponent, scope As SummaryScope)
            Dim term As String = If(scope = SummaryScope.Finals, "Finals", "Midterm")
            Dim rows = Db.Query(ItemsSql,
                Function(r) New ScoreRow With {
                    .Name = r.Str("ItemName"),
                    .Highest = r.Str("HighestScore"),
                    .Acquired = r.Str("AcquiredScore")},
                ("@sid", Session.UserId), ("@cid", classId),
                ("@term", term),
                ("@comp", DatabaseComponentKey(component)))
            For Each row In rows
                Data.StudentScores.Add(row)
            Next
        End Sub

        ' Loading of Overall Summary (Aspects + Final Grades)
        Private Shared Sub LoadOverall(classId As String)
            Dim hasAttendance As Boolean = False
            Dim aspects = Db.Query(ComponentsSql,
                Function(r) New AspectRow With {
                    .Aspect = DisplayAspect(r.Str("GradingAspect")),
                    .Highest = r.Num("HighestPossibleScore"),
                    .Acquired = r.Num("AcquiredScore"),
                    .Percentage = NormalizePct(r.Str("GradePercentage"))},
                ("@sid", Session.UserId), ("@cid", classId))
            For Each a In aspects
                If String.Equals(a.Aspect, "Attendance", StringComparison.OrdinalIgnoreCase) Then hasAttendance = True
                Data.Aspects.Add(a)
            Next

            If Not hasAttendance Then
                Dim att = Db.Query(AttendanceTotalSql,
                    Function(r) New AspectRow With {
                        .Aspect = "Attendance",
                        .Highest = r.Num("Highest"),
                        .Acquired = r.Num("Acquired"),
                        .Percentage = WeightOf("Attendance")},
                    ("@sid", Session.UserId), ("@cid", classId))
                If att.Count > 0 Then Data.Aspects.Insert(0, att(0))
            End If

            Dim finals = Db.Query(FinalSql,
                Function(r)
                    Dim midterm = r.Num("MidtermGrade")
                    Dim final = r.Num("FinalGrade")
                    Dim midtermValue As Double
                    Dim finalValue As Double
                    Dim overall = ""
                    Dim gwa = ""

                    If Double.TryParse(midterm, NumberStyles.Any, CultureInfo.InvariantCulture, midtermValue) AndAlso
                       Double.TryParse(final, NumberStyles.Any, CultureInfo.InvariantCulture, finalValue) Then
                        overall = GradesService.CalculateOverallGrade(midtermValue, finalValue)

                        Dim overallValue As Double
                        If Double.TryParse(overall, NumberStyles.Any, CultureInfo.InvariantCulture, overallValue) Then
                            gwa = GradesService.CalculateGwa(overallValue)
                        End If
                    End If

                    Return New FinalGradeRow With {
                        .Midterm = midterm,
                        .Final = final,
                        .Overall = overall,
                        .Gwa = gwa}
                End Function,
                ("@sid", Session.UserId), ("@cid", classId))
            For Each f In finals
                Data.FinalGrades.Add(f)
            Next
        End Sub

        Private Shared _warned As Boolean
        Private Shared Sub Safe(work As Action)
            Try
                work()
                _warned = False
            Catch ex As Exception
                System.Diagnostics.Trace.TraceError("Summary load failed: " & ex.ToString())
                If _warned Then Return
                _warned = True
                System.Windows.MessageBox.Show("Could not load the grade summary. Please try again later.")
            End Try
        End Sub
    End Class

    Public NotInheritable Class GradeSummaryPage
        Private Sub New()
        End Sub

        Public Shared Function Create(classId As String,
                                      courseName As String,
                                      backTo As Func(Of UserControl)) As ListPage

            GradeSummaryService.LoadWeights(classId)

            Dim componentCombo As ComboBox = Ui.Combo("Class Record", 305, Data.RecordKinds)
            componentCombo.SelectedIndex = 0

            Dim scopeCombo As ComboBox = Ui.Combo("Category", 178, Data.ViewCategories)
            scopeCombo.SelectedIndex = 0

            Dim page As New ListPage("Summary of Record | " & courseName)

            Dim component As Func(Of SummaryComponent) = Function() CType(Math.Max(0, componentCombo.SelectedIndex), SummaryComponent)
            Dim scope As Func(Of SummaryScope) = Function() CType(Math.Max(0, scopeCombo.SelectedIndex), SummaryScope)

            Dim refresh As Action =
                Sub()
                    componentCombo.Visibility = If(scope() = SummaryScope.Overall,
                                                   Visibility.Collapsed, Visibility.Visible)

                    GradeSummaryService.Load(classId, component(), scope())
                    page.Table(BuildBody(component(), scope()))
                End Sub

            page.BackTo(backTo).
                 Wide().
                 Filter(componentCombo).
                 Tool(scopeCombo).
                 Band(Function() GradeSummaryService.BandText(component(), scope()),
                      componentCombo, scopeCombo)

            AddHandler componentCombo.SelectionChanged, Sub(s, e) refresh()
            AddHandler scopeCombo.SelectionChanged, Sub(s, e) refresh()

            refresh()
            Return page
        End Function
        Private Shared Function BuildBody(component As SummaryComponent, scope As SummaryScope) As UIElement
            If scope = SummaryScope.Overall Then
                Dim stack As New StackPanel()
                stack.Children.Add(BuildAspectTable())
                stack.Children.Add(BuildFinalGradeTable())
                Return stack
            End If

            If component = SummaryComponent.Attendance Then
                Return BuildAttendanceTable()
            End If
            Return BuildScoreTable()
        End Function
        Private Shared Function BuildAttendanceTable() As UIElement
            Return Tbl.For(CType(Data.AttendanceLog, IEnumerable)).Big().NoHeader().
                Col("Date", "Date", 1, left:=True).
                Col("Status", "Status", 1, left:=True, bold:=True).Build()
        End Function

        Private Shared Function BuildScoreTable() As UIElement
            Return Tbl.For(CType(Data.StudentScores, IEnumerable)).Big().Head("GridHeaderLeft").HeadH(47).
                Col("NAME", "Name", 1, left:=True).
                Col("Highest Possible Score", "Highest", 1, left:=True).
                Col("Acquired Score", "Acquired", 1, left:=True).Build()
        End Function

        Private Shared Function BuildAspectTable() As UIElement
            Return Tbl.For(CType(Data.Aspects, IEnumerable)).HeadH(46).RowH(46).
                Col("Grading Aspect", "Aspect", 213).Col("Highest Possible Score", "Highest", 266).
                Col("Acquired Score", "Acquired", 272).Col("Grading Percentage", "Percentage", 275).Build()
        End Function

        Private Shared Function BuildFinalGradeTable() As UIElement
            Dim t = Tbl.For(CType(Data.FinalGrades, IEnumerable)).HeadH(46).RowH(46).
                Col("Midterm Grade", "Midterm", 213).Col("Final Grade", "Final", 266).
                Col("Overall Grade", "Overall", 272).Col("GWA", "Gwa", 275, bold:=True).Build()
            t.Margin = New Thickness(0, 14, 0, 0)
            Return t
        End Function
    End Class

End Namespace