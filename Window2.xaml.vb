Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Namespace SidhawanForStudents

    Partial Public Class Window2
        Inherits Window

        Public Sub New()
            InitializeComponent()

            If Session.IsLoggedIn Then
                WelcomeName.Text = Session.UserName & "!"
            End If

            Sidebar.Background = CType(FindResource("StudentSide"), Brush)

            Dim brush = New ImageBrush(New BitmapImage(New Uri("pack://application:,,,/Images/Pfp.png")))
            brush.Stretch = Stretch.UniformToFill
            SideAvatar.Fill = brush
            HeaderAvatar.Fill = brush

            NavList.ItemsSource = New String() {"Student Information", "Grades", "Change Password"}
            AddHandler NavList.SelectionChanged, Sub(src, args) ShowPage()
            NavList.SelectedIndex = 0
        End Sub

        Public Sub Navigate(view As UserControl)
            Body.Content = view
        End Sub

        Private Sub ShowPage()
            Dim page As String = TryCast(NavList.SelectedItem, String)
            If page Is Nothing Then Return
            Data.Request(page)

            Dim view As UserControl = Nothing
            If page = "Student Information" Then
                view = Pages.StudentInfo()
            ElseIf page = "Grades" Then
                view = Pages.StudentGrades()
            ElseIf page = "Change Password" Then
                view = New ChangePassword()
            End If
            Body.Content = view
        End Sub

        Private Sub LogOut_Click(sender As Object, e As RoutedEventArgs)
            SidhawanForStudents.Session.SignOut()
            Dim login As Window = New LogInWindow()
            login.Show()
            Close()
        End Sub

        Private Sub TOS_Click(sender As Object, e As RoutedEventArgs)
            Me.Dialog.ShowText("Terms of Service",
                "1. Use your own account only." & vbCrLf & vbCrLf &
                "2. Do not share your password." & vbCrLf & vbCrLf &
                "3. Your records are for your own viewing." & vbCrLf & vbCrLf)
        End Sub

        Private Sub PrivPolicy_Click(sender As Object, e As RoutedEventArgs)
            Me.Dialog.ShowText("Privacy Policy",
                "1. Use your own account only." & vbCrLf & vbCrLf &
                "2. Do not share your password." & vbCrLf & vbCrLf &
                "3. Your records are for your own viewing." & vbCrLf & vbCrLf)
        End Sub

        Private Sub Guidelines_Click(sender As Object, e As RoutedEventArgs)
            Me.Dialog.ShowText("Guidelines",
                "1. Use your own account only." & vbCrLf & vbCrLf &
                "2. Do not share your password." & vbCrLf & vbCrLf &
                "3. Your records are for your own viewing." & vbCrLf & vbCrLf)
        End Sub
    End Class
End Namespace