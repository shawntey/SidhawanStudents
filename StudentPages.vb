Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace SidhawanForStudents
    Partial Public Module Pages
        Public Function StudentInfo() As UserControl
            Return New SidhawanForStudents.PersonView("Student Information", "Student Number", SidhawanForStudents.Data.Student)
        End Function

        Public Function StudentGrades() As UserControl
            Dim p = New SidhawanForStudents.ListPage("Grades").Gap(25)
            Dim year = SidhawanForStudents.Ui.Combo("School Year", 161, SidhawanForStudents.Data.Years)
            Dim semester = SidhawanForStudents.Ui.Combo("Semester", 161, SidhawanForStudents.Data.Semesters)
            Dim yearBox = CType(year, ComboBox)
            Dim semesterBox = CType(semester, ComboBox)
            yearBox.IsSynchronizedWithCurrentItem = False
            semesterBox.IsSynchronizedWithCurrentItem = False
            yearBox.SelectedIndex = -1
            semesterBox.SelectedIndex = -1

            Dim table = SidhawanForStudents.Tbl.For(CType(SidhawanForStudents.Data.Grades, IEnumerable)).HeadH(54).RowH(38).Col("Course Code", "CourseCode", 128).Link("Course Name", "CourseName", 159, CType(Pages.Opens(CType(p, SidhawanForStudents.ListPage), Function(row) Pages.StudentRecord(CType(CType(row, SidhawanForStudents.GradeRow), SidhawanForStudents.GradeRow))), Action(Of Object))).Col("Section", "Section", 119).Col("Professor", "Professor", 151).Col("Sem Grade", "SemGrade", 116).Col("Remarks", "Remarks", 118).Col("Re-Exam", "ReExam", 129).Col("Units Earned", "Units", 106).Build()

            Dim average = New TextBlock With {
                .FontSize = 19,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
}
            Dim box = New Border With {
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Width = 381,
                .Height = 44,
                .Background = SidhawanForStudents.Ui.B("SectionBody"),
                .BorderBrush = Brushes.Black,
                .BorderThickness = New Thickness(1),
                .Child = average
}
            Dim updateDataVisibility As Action =
                Sub()
                    Dim hasData = SidhawanForStudents.Data.Grades.Count > 0
                    table.Visibility = Visibility.Visible
                    box.Visibility = Visibility.Visible
                    average.Text = If(hasData,
                                      "Cumulative Weighted Average: " & SidhawanForStudents.Data.CumulativeAverage,
                                      "")
                    box.Visibility = Visibility.Visible
                End Sub
            AddHandler SidhawanForStudents.Data.Grades.CollectionChanged,
                Sub(sender, args) updateDataVisibility()
            Dim restoringSelections As Boolean = True

            Dim loadGrades As Action =
                Sub()
                    Dim semBox = semesterBox
                    If restoringSelections Then Return
                    If yearBox.SelectedItem Is Nothing OrElse semBox.SelectedItem Is Nothing Then Return
                    SidhawanForStudents.Data.SelectedSchoolYear = yearBox.SelectedItem.ToString()
                    SidhawanForStudents.Data.SelectedSemester = semBox.SelectedItem.ToString()
                    SidhawanForStudents.GradesService.LoadGrades(SidhawanForStudents.Data.SelectedSchoolYear, SidhawanForStudents.Data.SelectedSemester)
                    updateDataVisibility()
                    If SidhawanForStudents.Data.Grades.Count = 0 Then
                        p.Popup.ShowNotice("check",
                                           "The data for S.Y. " & SidhawanForStudents.Data.SelectedSchoolYear &
                                           " | " & SidhawanForStudents.Data.SelectedSemester &
                                           " is currently in progress.")
                    End If
                End Sub

            AddHandler year.SelectionChanged, Sub(sender, args) loadGrades()
            AddHandler semester.SelectionChanged, Sub(sender, args) loadGrades()

            If Not String.IsNullOrWhiteSpace(SidhawanForStudents.Data.SelectedSchoolYear) AndAlso
               yearBox.Items.Contains(SidhawanForStudents.Data.SelectedSchoolYear) Then
                yearBox.SelectedItem = SidhawanForStudents.Data.SelectedSchoolYear
            End If
            If Not String.IsNullOrWhiteSpace(SidhawanForStudents.Data.SelectedSemester) AndAlso
               semesterBox.Items.Contains(SidhawanForStudents.Data.SelectedSemester) Then
                semesterBox.SelectedItem = SidhawanForStudents.Data.SelectedSemester
            End If

            restoringSelections = False
            If yearBox.SelectedItem IsNot Nothing AndAlso semesterBox.SelectedItem IsNot Nothing AndAlso
               SidhawanForStudents.Data.Grades.Count = 0 Then
                loadGrades()
            End If

            updateDataVisibility()

            p.Filter(CType(year, ComboBox), CType(semester, ComboBox)).Band(
                Function()
                    Dim selectedYear = SidhawanForStudents.Ui.Sel(CType(year, ComboBox))
                    Dim selectedSemester = SidhawanForStudents.Ui.Sel(CType(semester, ComboBox))
                    If String.IsNullOrWhiteSpace(selectedYear) OrElse String.IsNullOrWhiteSpace(selectedSemester) Then Return ""
                    Return "S.Y. " & selectedYear & "  |  " & selectedSemester & " Semester"
                End Function,
                CType(year, ComboBox), CType(semester, ComboBox)).Table(SidhawanForStudents.Ui.Stack(table, box))
            Return p
        End Function

        Private Function StudentRecord(course As SidhawanForStudents.GradeRow) As UserControl
            Return SidhawanForStudents.GradeSummaryPage.Create(
                course.ClassId,
                course.CourseName,
                Function() StudentGrades())
        End Function
    End Module
End Namespace
