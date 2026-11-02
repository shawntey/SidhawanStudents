Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports MySqlConnector

Namespace SidhawanForStudents

    ' Storage for user information in every session
    Public NotInheritable Class Session
        Private Sub New()
        End Sub

        Private Shared _userId As String = String.Empty
        Private Shared _userName As String = String.Empty

        Public Shared ReadOnly Property UserId As String
            Get
                Return _userId
            End Get
        End Property

        Public Shared ReadOnly Property UserName As String
            Get
                Return _userName
            End Get
        End Property

        Public Shared ReadOnly Property IsLoggedIn As Boolean
            Get
                Return Not String.IsNullOrEmpty(_userId)
            End Get
        End Property

        Friend Shared Sub SignIn(id As String, name As String)
            _userId = id
            _userName = name
            Data.ClearSessionData()
        End Sub

        Public Shared Sub SignOut()
            _userId = String.Empty
            _userName = String.Empty
            Data.ClearSessionData()
        End Sub
    End Class

    ' Various Login Authentication related methods
    Public NotInheritable Class AuthManager
        Private Sub New()
        End Sub

        Public Enum LoginResult
            Success
            InvalidCredentials
            LockedOut
            ServerError
        End Enum

        Private Const MaxAttempts As Integer = 5
        Private Shared _failedAttempts As Integer
        Private Shared _lockedUntil As DateTime = DateTime.MinValue

        ' User login validation
        Public Shared Function ValidateStudentLogin(userIdInput As String, passwordInput As String) As LoginResult
            If DateTime.UtcNow < _lockedUntil Then Return LoginResult.LockedOut

            If String.IsNullOrWhiteSpace(userIdInput) OrElse String.IsNullOrEmpty(passwordInput) Then
                Return LoginResult.InvalidCredentials
            End If

            Dim id As String = userIdInput.Trim()

            Try
                ' Parameterized query: the input is never concatenated into the SQL.
                Dim rows = Db.Query(
                    "SELECT student_Lname, PasswordHash FROM Student WHERE StudentID = @id LIMIT 1",
                    Function(r) (LastName:=r.Str("student_Lname"), Hash:=r.Str("PasswordHash")),
                    ("@id", id))

                ' Ordinal = case-sensitive match
                If rows.Count > 0 AndAlso
                   String.Equals(rows(0).Hash, passwordInput, StringComparison.Ordinal) Then
                    _failedAttempts = 0
                    Session.SignIn(id, rows(0).LastName)
                    Return LoginResult.Success
                End If

                _failedAttempts += 1
                If _failedAttempts >= MaxAttempts Then
                    _lockedUntil = DateTime.UtcNow.AddMinutes(1)
                    _failedAttempts = 0
                End If
                Return LoginResult.InvalidCredentials

            Catch ex As Exception
                System.Diagnostics.Trace.TraceError("Login failed: " & ex.ToString())
                System.Windows.MessageBox.Show("Could not reach the server. Please try again later.")
                Return LoginResult.ServerError
            End Try
        End Function

        Public Enum ChangeResult
            Success
            NotLoggedIn
            WrongCurrent
            WeakPassword
            SameAsOld
            ServerError
        End Enum

        ' Change Password method to overwrite database content
        Public Shared Function ChangePassword(currentPw As String, newPw As String) As ChangeResult
            If Not Session.IsLoggedIn Then Return ChangeResult.NotLoggedIn
            If Not Passwords.MeetsPolicy(newPw) Then Return ChangeResult.WeakPassword

            Try
                Dim rows = Db.Query(
                    "SELECT PasswordHash FROM Student WHERE StudentID = @id LIMIT 1",
                    Function(r) r.Str("PasswordHash"),
                    ("@id", Session.UserId))

                If rows.Count = 0 OrElse Not String.Equals(rows(0), currentPw, StringComparison.Ordinal) Then
                    Return ChangeResult.WrongCurrent
                End If
                If String.Equals(newPw, currentPw, StringComparison.Ordinal) Then Return ChangeResult.SameAsOld

                Dim changed As Integer = Db.Execute(
                    "UPDATE Student SET PasswordHash = @pw WHERE StudentID = @id",
                    ("@pw", newPw),
                    ("@id", Session.UserId))

                Return If(changed = 1, ChangeResult.Success, ChangeResult.ServerError)
            Catch ex As Exception
                System.Diagnostics.Trace.TraceError("Password change failed: " & ex.ToString())
                Return ChangeResult.ServerError
            End Try
        End Function
    End Class

    ' Password rules
    Public NotInheritable Class Passwords
        Private Sub New()
        End Sub

        Private Shared Function AnyChar(pw As String, test As Func(Of Char, Boolean)) As Boolean
            Return pw IsNot Nothing AndAlso pw.Any(test)
        End Function

        Public Shared Function HasLength(pw As String) As Boolean
            Return pw IsNot Nothing AndAlso pw.Length >= 8 AndAlso pw.Length <= 16
        End Function
        Public Shared Function HasUpper(pw As String) As Boolean
            Return AnyChar(pw, Function(c) Char.IsUpper(c))
        End Function
        Public Shared Function HasLower(pw As String) As Boolean
            Return AnyChar(pw, Function(c) Char.IsLower(c))
        End Function
        Public Shared Function HasDigit(pw As String) As Boolean
            Return AnyChar(pw, Function(c) Char.IsDigit(c))
        End Function
        Public Shared Function HasSpecial(pw As String) As Boolean
            Return AnyChar(pw, Function(c) Char.IsPunctuation(c) OrElse Char.IsSymbol(c))
        End Function
        Public Shared Function MeetsPolicy(pw As String) As Boolean
            Return HasLength(pw) AndAlso HasUpper(pw) AndAlso HasLower(pw) AndAlso HasDigit(pw) AndAlso HasSpecial(pw)
        End Function
    End Class

    ' Loading of Profile Information related methods
    Public NotInheritable Class ProfileService
        Private Sub New()
        End Sub
        Private Const LoadSql As String =
            "SELECT " &
            "    StudentID," &
            "    student_Fname," &
            "    student_Mname," &
            "    student_Lname," &
            "    student_Suffix," &
            "    DATE_FORMAT(student_DOB, '%M %e, %Y') AS student_DOB," &
            "    student_Sex," &
            "    COALESCE(student_ConNum, '') AS student_ConNum," &
            "    COALESCE(student_Email, '') AS student_Email," &
            "    YearLevel," &
            "    ProgramName," &
            "    CollegeName," &
            "    COALESCE(BatchYear, '') AS BatchYear " &
            "FROM vw_studentprofile WHERE StudentID = @id " &
            "ORDER BY BatchYear DESC LIMIT 1"
        Public Shared Sub LoadProfile()
            If Not Session.IsLoggedIn Then Return
            Dim p = LoadPerson(Session.UserId)
            Data.Student = If(p, New Person())
        End Sub
        Public Shared Function LoadPerson(studentId As String) As Person
            If String.IsNullOrWhiteSpace(studentId) Then Return Nothing
            Try
                Dim rows = Db.Query(LoadSql,
                    Function(r)
                        Dim p As New Person()
                        p.Id = r.Str("StudentID")
                        p.FirstName = r.Str("student_Fname")
                        p.MiddleName = r.Str("student_Mname")
                        p.LastName = r.Str("student_Lname")
                        p.Suffix = r.Str("student_Suffix")
                        p.BirthDate = r.Str("student_DOB")
                        p.Sex = r.Str("student_Sex")
                        p.ContactNumber = r.Str("student_ConNum")
                        p.Email = r.Str("student_Email")

                        AddAcademic(p, "Year Level", r.Str("YearLevel"))
                        AddAcademic(p, "Program", r.Str("ProgramName"))
                        AddAcademic(p, "College", r.Str("CollegeName"))
                        AddAcademic(p, "Batch Year", r.Str("BatchYear"))
                        Return p
                    End Function,
                    ("@id", studentId.Trim()))

                Return If(rows.Count > 0, rows(0), Nothing)
            Catch ex As Exception
                System.Diagnostics.Trace.TraceError("Profile load failed: " & ex.ToString())
                System.Windows.MessageBox.Show("Could not load your profile. Please try again later.")
                Return Nothing
            End Try
        End Function

        Private Shared Sub AddAcademic(p As Person, label As String, value As String)
            p.Academic.Add(New Field(label, value))
        End Sub
    End Class

    ' Grade Service related methods
    Public NotInheritable Class GradesService
        Private Sub New()
        End Sub

        ' Filter Display
        Private Const FilterSql As String =
            "SELECT DISTINCT SchoolYear, REPLACE(Semester, 'SEMESTER', '') AS Semester " &
            "FROM vw_studentgrades g WHERE g.StudentID = @StudentID " &
            "AND g.ClassID IS NOT NULL AND TRIM(g.ClassID) <> '' " &
            "AND EXISTS (SELECT 1 FROM vw_studentcoursegradesummary c " &
            "WHERE c.StudentID = g.StudentID AND c.ClassID = g.ClassID) " &
            "ORDER BY SchoolYear DESC, " &
            "CASE UCASE(TRIM(REPLACE(Semester, 'SEMESTER', ''))) " &
            "WHEN '2ND' THEN 1 WHEN 'SECOND' THEN 1 " &
            "WHEN '1ST' THEN 2 WHEN 'FIRST' THEN 2 ELSE 3 END"
        Public Shared Sub LoadFilters()
            If Not Session.IsLoggedIn Then Return

            Data.Years.Clear()
            Data.Semesters.Clear()
            Data.SelectedSchoolYear = ""
            Data.SelectedSemester = ""

            Safe(Sub()
                     Dim pairs = Db.Query(FilterSql,
                 Function(r) (Year:=r.Str("SchoolYear"), Sem:=r.Str("Semester")),
                 ("@StudentID", Session.UserId))

                     For Each yearText In pairs.Select(Function(x) x.Year).Distinct()
                         Data.Years.Add(yearText)
                     Next

                     For Each semText In pairs.Select(Function(x) x.Sem).Distinct()
                         Data.Semesters.Add(semText)
                     Next

                      If pairs.Count = 0 Then
                          System.Windows.MessageBox.Show(
                              "No grading records are available for this account.",
                              "No grading records",
                              System.Windows.MessageBoxButton.OK,
                              System.Windows.MessageBoxImage.Information)
                      Else
                          Data.SelectedSchoolYear = pairs(0).Year
                          Data.SelectedSemester = pairs(0).Sem
                      End If
                 End Sub)
        End Sub

        Private Shared _warned As Boolean

        ' Grade Display
        Private Const GradesSql As String =
            "SELECT StudentID, ClassID, CourseCode, CourseName, Section, Professor, SemGrade, Remarks, ReExam, UnitsEarned " &
            "FROM vw_studentgrades g WHERE g.StudentID = @StudentID " &
            "AND g.ClassID IS NOT NULL AND TRIM(g.ClassID) <> '' " &
            "AND EXISTS (SELECT 1 FROM vw_studentcoursegradesummary c " &
            "WHERE c.StudentID = g.StudentID AND c.ClassID = g.ClassID) " &
            "AND g.SchoolYear = @SchoolYear AND REPLACE(g.Semester, ' SEMESTER', '') = @Semester " &
            "ORDER BY CourseCode"

        Public Shared Sub Start()
            RemoveHandler Data.PageRequested, AddressOf OnPage
            AddHandler Data.PageRequested, AddressOf OnPage
        End Sub

        Private Shared Sub OnPage(page As String)
            If page <> "Grades" Then Return
            Data.Grades.Clear()
            Data.CumulativeAverage = ""
            Data.Years.Clear()
            Data.Semesters.Clear()
            LoadFilters()
        End Sub
        Public Shared Sub LoadGrades(schoolYear As String, semester As String)
            If Not Session.IsLoggedIn Then Return
            Data.Grades.Clear()
            Data.CumulativeAverage = ""
            If String.IsNullOrWhiteSpace(schoolYear) OrElse String.IsNullOrWhiteSpace(semester) Then Return

            Safe(Sub()
                     Dim rows As List(Of GradeRow) = Db.Query(GradesSql,
                         Function(reader)
                             Dim g As New GradeRow()
                              g.StudentId = reader.Str("StudentID")
                             g.ClassId = reader.Str("ClassID")
                             g.CourseCode = reader.Str("CourseCode")
                             g.CourseName = reader.Str("CourseName")
                             g.Section = reader.Str("Section")
                             g.Professor = reader.Str("Professor")
                             g.SemGrade = reader.Num("SemGrade")
                             g.Remarks = reader.Str("Remarks")
                             g.ReExam = reader.Str("ReExam")
                             g.Units = reader.Num("UnitsEarned")
                             Return g
                         End Function,
                         ("@StudentID", Session.UserId),
                         ("@SchoolYear", schoolYear),
                         ("@Semester", semester))

                      rows = rows.Where(Function(row) String.Equals(row.StudentId.Trim(), Session.UserId.Trim(), StringComparison.OrdinalIgnoreCase) AndAlso Not String.IsNullOrWhiteSpace(row.ClassId)).ToList()

                     For Each row In rows
                         Data.Grades.Add(row)
                     Next
                     Data.CumulativeAverage = Cwa(rows)
                     _warned = False
                 End Sub)
        End Sub

        Public Shared Function CalculateGwa(gradeAverage As Double) As String
            Dim gwa As Double

            If gradeAverage >= 97 Then
                gwa = 1.0
            ElseIf gradeAverage >= 94 Then
                gwa = 1.25
            ElseIf gradeAverage >= 91 Then
                gwa = 1.5
            ElseIf gradeAverage >= 88 Then
                gwa = 1.75
            ElseIf gradeAverage >= 85 Then
                gwa = 2.0
            ElseIf gradeAverage >= 82 Then
                gwa = 2.25
            ElseIf gradeAverage >= 79 Then
                gwa = 2.5
            ElseIf gradeAverage >= 76 Then
                gwa = 2.75
            ElseIf gradeAverage = 75 Then
                gwa = 3.0
            Else
                gwa = 5.0
            End If

            Return gwa.ToString("0.00", CultureInfo.InvariantCulture)
        End Function

        Public Shared Function CalculateOverallGrade(midtermGrade As Double, finalGrade As Double) As String
            Dim overallGrade = (midtermGrade + finalGrade) / 2.0
            Return overallGrade.ToString("0.00", CultureInfo.InvariantCulture)
        End Function

        Private Shared Function Cwa(rows As IEnumerable(Of GradeRow)) As String
            Dim points As Double = 0
            Dim units As Double = 0
            For Each row In rows
                Dim grade As Double
                Dim unit As Double
                If Double.TryParse(row.SemGrade, NumberStyles.Any, CultureInfo.InvariantCulture, grade) AndAlso
                   Double.TryParse(row.Units, NumberStyles.Any, CultureInfo.InvariantCulture, unit) Then
                    points += grade * unit
                    units += unit
                End If
            Next
            Return If(units > 0, (points / units).ToString("0.00", CultureInfo.InvariantCulture), "")
        End Function

        Private Shared Sub Safe(work As Action)
            Try
                work()
            Catch ex As Exception
                System.Diagnostics.Trace.TraceError("Grades load failed: " & ex.ToString())
                If _warned Then Return
                _warned = True
                System.Windows.MessageBox.Show("Could not load your grades from the server. Please try again later.")
            End Try
        End Sub
    End Class

End Namespace