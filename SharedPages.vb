Imports System
Imports System.Windows
Imports System.Windows.Controls

Namespace SidhawanForStudents

    Public Partial Module Pages
        Private ReadOnly RecordBands As String() = {"ATTENDANCE (5%)", "QUIZZES (30%)", "CLASS WORK (25%)", "MAJOR EXAM (40%)"}

        Private Function Opens(from As SidhawanForStudents.ListPage, [next] As Func(Of Object, UserControl)) As Action(Of Object)
            Return Sub(row)
                       SidhawanForStudents.Data.Open(row)
                       from.GoTo([next](row))
                   End Sub
        End Function

        Private Function RecordPage(title As String, back As Func(Of UserControl)) As UserControl
            Dim p = New SidhawanForStudents.ListPage(title)
            Dim kind = 0
            Dim overall = False

            Dim classCombo = SidhawanForStudents.Ui.Combo("Class Record", 305, SidhawanForStudents.Data.RecordKinds)
            Dim category = SidhawanForStudents.Ui.Combo("Category", 178, SidhawanForStudents.Data.ViewCategories)
            Dim Show As Action =
                Sub()
                    classCombo.Visibility = If(overall, Visibility.Hidden, Visibility.Visible)
                    p.Band(If(overall, "Overall Summary", RecordBands(kind)))
                    If overall Then
                        p.Table(OverallTables())
                    Else
                        p.Table(ViewTable(kind))
                    End If
                End Sub

            AddHandler classCombo.SelectionChanged,
                Sub(src, args)
                    kind = Math.Max(0, classCombo.SelectedIndex)
                    Show()
                End Sub
            AddHandler category.SelectionChanged,
                Sub(src, args)
                    overall = Equals(TryCast(category.SelectedItem, String), "Overall Summary")
                    Show()
                End Sub

            p.Filter(CType(classCombo, ComboBox)).Tool(CType(category, UIElement)).BackTo(back)
            Show()
            Return p
        End Function

        Private Function ViewTable(kind As Integer) As UIElement
            Return If(kind = 0, SidhawanForStudents.Tbl.For(CType(SidhawanForStudents.Data.AttendanceLog, IEnumerable)).Big().NoHeader().Col("Date", "Date", 1, left:=True).Col("Status", "Status", 1, left:=True, bold:=True).Build(), SidhawanForStudents.Tbl.For(CType(SidhawanForStudents.Data.StudentScores, IEnumerable)).Big().Head("GridHeaderLeft").HeadH(47).Col("NAME", "Name", 1, left:=True).Col("Highest Possible Score", "Highest", 1, left:=True).Col("Acquired Score", "Acquired", 1, left:=True).Build())
        End Function

        Private Function OverallTables() As UIElement
            Dim aspects = SidhawanForStudents.Tbl.For(CType(SidhawanForStudents.Data.Aspects, IEnumerable)).HeadH(46).RowH(46).Col("Grading Aspect", "Aspect", 213).Col("Highest Possible Score", "Highest", 266).Col("Acquired Score", "Acquired", 272).Col("Grading Percentage", "Percentage", 275).Build()

            Dim grades = SidhawanForStudents.Tbl.For(CType(SidhawanForStudents.Data.FinalGrades, IEnumerable)).HeadH(46).RowH(46).Col("Midterm Grade", "Midterm", 213).Col("Final Grade", "Final", 266).Col("Overall Grade", "Overall", 272).Col("GWA", "Gwa", 275, bold:=True).Build()
            grades.Margin = New Thickness(0, 14, 0, 0)

            Return SidhawanForStudents.Ui.Stack(aspects, grades)
        End Function
    End Module
End Namespace
