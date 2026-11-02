Imports System.Windows

Namespace SidhawanForStudents

    Public Partial Class LogInWindow
        Inherits Window
        Public Sub New()
            Me.InitializeComponent()
        End Sub

        Private Sub LogIn_Click(sender As Object, e As RoutedEventArgs)
            Dim idMissing As Boolean = String.IsNullOrWhiteSpace(UserId.Text)
            Dim pwdMissing As Boolean = String.IsNullOrEmpty(Pwd.Password)

            If idMissing AndAlso pwdMissing Then
                MessageBox.Show("Please enter your student number and password.",
                        "Missing information", MessageBoxButton.OK, MessageBoxImage.Information)
                UserId.Focus()
                Return
            ElseIf idMissing Then
                MessageBox.Show("Please enter your student number.",
                        "Missing information", MessageBoxButton.OK, MessageBoxImage.Information)
                UserId.Focus()
                Return
            ElseIf pwdMissing Then
                MessageBox.Show("Please enter your password.",
                        "Missing information", MessageBoxButton.OK, MessageBoxImage.Information)
                Pwd.Focus()
                Return
            End If

            Dim result = AuthManager.ValidateStudentLogin(UserId.Text, Pwd.Password)

            Select Case result
                Case AuthManager.LoginResult.Success
                    ProfileService.LoadProfile()
                    Call New SidhawanForStudents.Window2().Show()
                    Close()

                Case AuthManager.LoginResult.LockedOut
                    MessageBox.Show("Too many failed attempts. Please wait a minute and try again.",
                            "Locked", MessageBoxButton.OK, MessageBoxImage.Warning)

                Case AuthManager.LoginResult.ServerError
                    MessageBox.Show("Can't reach the server right now. Please try again later.",
                            "Connection problem", MessageBoxButton.OK, MessageBoxImage.Error)

                Case Else
                    MessageBox.Show("Invalid student number or password.",
                            "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning)
            End Select
        End Sub

    End Class
End Namespace
