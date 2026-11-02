Imports System.Collections.ObjectModel

Namespace SidhawanForStudents

    Public Module DemoData
        Private Sub Fill(Of T)([to] As System.Collections.ObjectModel.ObservableCollection(Of T), ParamArray items As T())
            For Each i In items
                [to].Add(i)
            Next
        End Sub

        Public Sub Load()
            Fill(SidhawanForStudents.Data.Years, "2025-2026", "2026-2027")
            Fill(SidhawanForStudents.Data.Semesters, "1st", "2nd")

            Dim s As New Person()
            s.Id = "25-00999"
            s.FirstName = "Maria"
            s.MiddleName = "Dimaguiba"
            s.LastName = "Santos"
            s.Suffix = "N/A"
            s.BirthDate = "2007-12-25"
            s.Sex = "Female"
            s.ContactNumber = "09981123221"
            s.Email = "santosmaria@plpasig.edu.ph"

            s.Academic.Add(New Field("Batch Year", "2026-2027"))
            s.Academic.Add(New Field("Year Level", "Second Year"))
            s.Academic.Add(New Field("College", "College of Computer Studies"))
            s.Academic.Add(New Field("Program", "Bachelor of Science in Computer Science"))
          

            Data.Student = s

            Fill(SidhawanForStudents.Data.Grades, New SidhawanForStudents.GradeRow With {
        .CourseCode = "GE 001",
        .CourseName = "Understanding the Self",
        .Section = "BSIT 1D",
        .Professor = "Elena R. Javate",
        .SemGrade = "1.00",
        .Remarks = "Passed",
        .Units = "3.00"
    }, New SidhawanForStudents.GradeRow With {
        .CourseCode = "CS 101",
        .CourseName = "Discrete Structures 1",
        .Section = "BSIT 1D",
        .Professor = "Federico G. Nueva",
        .SemGrade = "1.25",
        .Remarks = "Passed",
        .Units = "3.00"
    }, New SidhawanForStudents.GradeRow With {
        .CourseCode = "COMP 103",
        .CourseName = "Intermediate" & Global.Microsoft.VisualBasic.Constants.vbLf & "Programming",
        .Section = "BSIT 1D",
        .Professor = "Riegie D. Tan",
        .SemGrade = "1.00",
        .Remarks = "Passed",
        .Units = "3.00"
    }, New SidhawanForStudents.GradeRow With {
        .CourseCode = "GE 005",
        .CourseName = "Purposive" & Global.Microsoft.VisualBasic.Constants.vbLf & "Communication",
        .Section = "BSIT 1D",
        .Professor = "Shiela G. Manahan",
        .SemGrade = "1.50",
        .Remarks = "Passed",
        .Units = "3.00"
    }, New SidhawanForStudents.GradeRow With {
        .CourseCode = "GE 007",
        .CourseName = "Science, Technology" & Global.Microsoft.VisualBasic.Constants.vbLf & "and Society",
        .Section = "BSIT 1D",
        .Professor = "Joseph S. Cruz",
        .SemGrade = "1.25",
        .Remarks = "Passed",
        .Units = "3.00"
    })
            SidhawanForStudents.Data.CumulativeAverage = "1.20"

            Fill(SidhawanForStudents.Data.AttendanceLog, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/09/2026",
        .Status = "PRESENT"
    }, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/09/2026",
        .Status = "ABSENT"
    }, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/14/2026",
        .Status = "PRESENT"
    }, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/16/2026",
        .Status = "PRESENT"
    }, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/21/2026",
        .Status = "PRESENT"
    }, New SidhawanForStudents.AttendanceLogRow With {
        .[Date] = "08/23/2026",
        .Status = "PRESENT"
    })
            Fill(SidhawanForStudents.Data.StudentScores, New SidhawanForStudents.ScoreRow With {
        .Name = "Quiz 1",
        .Highest = "20",
        .Acquired = "20"
    }, New SidhawanForStudents.ScoreRow With {
        .Name = "Quiz 2",
        .Highest = "30",
        .Acquired = "0"
    }, New SidhawanForStudents.ScoreRow With {
        .Name = "Quiz 3",
        .Highest = "20",
        .Acquired = "20"
    }, New SidhawanForStudents.ScoreRow With {
        .Name = "Quiz 4",
        .Highest = "20",
        .Acquired = "20"
    })
            Fill(SidhawanForStudents.Data.Aspects, New SidhawanForStudents.AspectRow With {
        .Aspect = "Attendance",
        .Highest = "20",
        .Acquired = "20",
        .Percentage = "5%"
    }, New SidhawanForStudents.AspectRow With {
        .Aspect = "Quizzes",
        .Highest = "90",
        .Acquired = "89",
        .Percentage = "30%"
    }, New SidhawanForStudents.AspectRow With {
        .Aspect = "Class Work",
        .Highest = "100",
        .Acquired = "99",
        .Percentage = "25%"
    }, New SidhawanForStudents.AspectRow With {
        .Aspect = "Major Examination",
        .Highest = "180",
        .Acquired = "178",
        .Percentage = "40%"
    })
            Fill(SidhawanForStudents.Data.FinalGrades, New SidhawanForStudents.FinalGradeRow With {
                    .Midterm = "99.04",
                    .Final = "99.04",
                    .Overall = "99.04",
                    .Gwa = "1.00"
                })
        End Sub
    End Module
End Namespace
