Imports System
Imports System.Collections.ObjectModel

Namespace SidhawanForStudents

    Public Module Data

        Public ReadOnly Property Years As ObservableCollection(Of String) = New ObservableCollection(Of String)()
        Public ReadOnly Property Semesters As ObservableCollection(Of String) = New ObservableCollection(Of String)()
        Public ReadOnly RecordKinds As String() = {"Attendance", "Quizzes", "Class Work", "Major Exam"}
        Public ReadOnly ViewCategories As String() = {"Midterms", "Finals", "Overall Summary"}

        Public Property Student As SidhawanForStudents.Person = New SidhawanForStudents.Person()
        Public ReadOnly Property Grades As ObservableCollection(Of SidhawanForStudents.GradeRow) = New ObservableCollection(Of SidhawanForStudents.GradeRow)()
        Public Property CumulativeAverage As String = ""
        Public Property SelectedSchoolYear As String = ""
        Public Property SelectedSemester As String = ""
        Public ReadOnly Property AttendanceLog As ObservableCollection(Of SidhawanForStudents.AttendanceLogRow) = New ObservableCollection(Of SidhawanForStudents.AttendanceLogRow)()
        Public ReadOnly Property StudentScores As ObservableCollection(Of SidhawanForStudents.ScoreRow) = New ObservableCollection(Of SidhawanForStudents.ScoreRow)()
        Public ReadOnly Property Aspects As ObservableCollection(Of SidhawanForStudents.AspectRow) = New ObservableCollection(Of SidhawanForStudents.AspectRow)()
        Public ReadOnly Property FinalGrades As ObservableCollection(Of SidhawanForStudents.FinalGradeRow) = New ObservableCollection(Of SidhawanForStudents.FinalGradeRow)()

        Public Event PageRequested As Action(Of String)
        Public Event FilterChanged As Action(Of String, String)
        Public Event RowOpened As Action(Of Object)
        Public Event RowEdited As Action(Of Object, String)

        Public Sub ClearSessionData()
            Student = New SidhawanForStudents.Person()
            Years.Clear()
            Semesters.Clear()
            Grades.Clear()
            AttendanceLog.Clear()
            StudentScores.Clear()
            Aspects.Clear()
            FinalGrades.Clear()
            CumulativeAverage = ""
            SelectedSchoolYear = ""
            SelectedSemester = ""
        End Sub

        Public Sub Request(page As String)
            RaiseEvent PageRequested(page)
        End Sub
        Public Sub RaiseFilterChanged(label As String, value As String)
            RaiseEvent FilterChanged(label, value)
        End Sub
        Public Sub Open(row As Object)
            RaiseEvent RowOpened(row)
        End Sub
        Public Sub Edited(row As Object, column As String)
            RaiseEvent RowEdited(row, column)
        End Sub
    End Module
End Namespace
