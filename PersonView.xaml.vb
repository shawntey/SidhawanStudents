Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data

Namespace SidhawanForStudents

    Partial Public Class PersonView
        Inherits UserControl

        Private Shared ReadOnly PersonalFields As (String, String)() = {
            ("First Name", NameOf(Person.FirstName)),
            ("Middle Name", NameOf(Person.MiddleName)),
            ("Last Name", NameOf(Person.LastName)),
            ("Suffix", NameOf(Person.Suffix)),
            ("Birth Date", NameOf(Person.BirthDate)),
            ("Sex", NameOf(Person.Sex)),
            ("Contact Number", NameOf(Person.ContactNumber)),
            ("Email Address", NameOf(Person.Email))
        }

        Private ReadOnly _back As Func(Of UserControl)

        Public Shared Function ForCurrentStudent(Optional back As Func(Of UserControl) = Nothing) As PersonView
            Dim person = ProfileService.LoadPerson(Session.UserId)
            If person Is Nothing Then Return Nothing
            Return New PersonView("Profile", "Student ID", person, False, back)
        End Function

        Public Sub New(title As String, idLabel As String, person As Person,
                       Optional canEdit As Boolean = False,
                       Optional back As Func(Of UserControl) = Nothing)
            InitializeComponent()
            TitleText.Text = title

            IdField.Label = idLabel
            Bind(IdField, person, NameOf(person.Id))

            For Each entry In PersonalFields
                Dim infoField = New InfoField With {.Label = entry.Item1}
                Bind(infoField, person, entry.Item2)
                PersonalGrid.Children.Add(infoField)
            Next

            For Each item In person.Academic
                Dim infoField = New InfoField With {.Label = item.Label}
                Bind(infoField, item, "Value")
                AcademicGrid.Children.Add(infoField)
            Next

            EditBtn.Visibility = If(canEdit, Visibility.Visible, Visibility.Collapsed)
            _back = back
            BackBtn.Visibility = If(back IsNot Nothing, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Private Shared Sub Bind(field As InfoField, source As Object, path As String)
            Dim binding = New Binding(path)
            binding.Source = source
            binding.Mode = BindingMode.TwoWay
            field.SetBinding(InfoField.ValueProperty, binding)
        End Sub

        Private Sub Back_Click(sender As Object, e As RoutedEventArgs)
            If _back IsNot Nothing Then
                Dim win = TryCast(Window.GetWindow(Me), Window2)
                If win IsNot Nothing Then win.Navigate(_back())
            End If
        End Sub

        Private Sub Edit_Click(sender As Object, e As RoutedEventArgs)
            Ui.ToggleEdit(Me)
        End Sub
    End Class
End Namespace