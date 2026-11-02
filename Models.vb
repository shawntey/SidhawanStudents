Imports System.Collections.Generic
Namespace SidhawanForStudents

    Public Class GradeRow
        Public Property StudentId As String = ""
        Public Property ClassId As String = ""
        Public Property CourseCode As String = ""
        Public Property CourseName As String = ""
        Public Property Section As String = ""
        Public Property Professor As String = ""
        Public Property SemGrade As String = ""
        Public Property Remarks As String = ""
        Public Property ReExam As String = ""
        Public Property Units As String = ""
    End Class

    Public Class AttendanceLogRow
        Public Property [Date] As String = ""
        Public Property Status As String = ""
    End Class

    Public Class ScoreRow
        Public Property Name As String = ""
        Public Property Highest As String = ""
        Public Property Acquired As String = ""
    End Class

    Public Class AspectRow
        Public Property Aspect As String = ""
        Public Property Highest As String = ""
        Public Property Acquired As String = ""
        Public Property Percentage As String = ""
    End Class

    Public Class FinalGradeRow
        Public Property Midterm As String = ""
        Public Property Final As String = ""
        Public Property Overall As String = ""
        Public Property Gwa As String = ""
    End Class

    Public Class Field
        Public Property Label As String = ""
        Public Property Value As String = ""
        Public Sub New()
        End Sub
        Public Sub New(label As String, value As String)
            Me.Label = label
            Me.Value = value
        End Sub
    End Class

    Public Class Person
        Public Property Id As String = ""
        Public Property FirstName As String = ""
        Public Property MiddleName As String = ""
        Public Property LastName As String = ""
        Public Property Suffix As String = ""
        Public Property BirthDate As String = ""
        Public Property Sex As String = ""
        Public Property ContactNumber As String = ""
        Public Property Email As String = ""
        Public ReadOnly Property Academic As List(Of Field) = New List(Of Field)()
    End Class
End Namespace
