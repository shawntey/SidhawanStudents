Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace SidhawanForStudents

    Partial Public Class ChangePassword
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            NewPw.AddHandler(PasswordBox.PasswordChangedEvent, New RoutedEventHandler(AddressOf OnNewPwTyped), True)
            NewPw.AddHandler(TextBox.TextChangedEvent, New TextChangedEventHandler(AddressOf OnNewPwTyped), True)
        End Sub

        Private Sub OnNewPwTyped(sender As Object, e As RoutedEventArgs)
            Dim pw = ReadText(NewPw)
            RuleLength.IsChecked = Passwords.HasLength(pw)
            RuleUpper.IsChecked = Passwords.HasUpper(pw)
            RuleLower.IsChecked = Passwords.HasLower(pw)
            RuleNumber.IsChecked = Passwords.HasDigit(pw)
            RuleSpecial.IsChecked = Passwords.HasSpecial(pw)
        End Sub

        Private Sub Change_Click(sender As Object, e As RoutedEventArgs)
            Dim currentPassword = ReadText(CurrentPw)
            Dim newPassword = ReadText(NewPw)
            Dim confirmPassword = ReadText(ConfirmPw)

            If String.IsNullOrEmpty(currentPassword) Then
                Dialog.ShowNotice("incorrect", "Enter your current password.") : Return
            End If
            If Not Passwords.MeetsPolicy(newPassword) Then
                Dialog.ShowNotice("incorrect", "New password doesn't meet all the requirements.") : Return
            End If
            If Not String.Equals(newPassword, confirmPassword, StringComparison.Ordinal) Then
                Dialog.ShowNotice("incorrect", "New password and confirmation don't match.") : Return
            End If

            Dialog.ShowConfirm("Are you sure you want to change your password?", "", "",
                Sub()
                    Select Case AuthManager.ChangePassword(currentPassword, newPassword)
                        Case AuthManager.ChangeResult.Success
                            Dialog.ShowNotice("check", "Password changed successfully!")
                            ClearPill(CurrentPw) : ClearPill(NewPw) : ClearPill(ConfirmPw)
                        Case AuthManager.ChangeResult.WrongCurrent
                            Dialog.ShowNotice("incorrect", "Your current password is incorrect.")
                        Case AuthManager.ChangeResult.SameAsOld
                            Dialog.ShowNotice("incorrect", "New password must be different from the old one.")
                        Case AuthManager.ChangeResult.WeakPassword
                            Dialog.ShowNotice("incorrect", "New password doesn't meet all the requirements.")
                        Case AuthManager.ChangeResult.NotLoggedIn
                            Dialog.ShowNotice("incorrect", "Your session has ended. Please sign in again.")
                        Case Else
                            Dialog.ShowNotice("incorrect", "Couldn't change your password. Please try again later.")
                    End Select
                End Sub)
        End Sub
        Private Shared Function ReadText(pill As DependencyObject) As String
            Dim pb = FindAll(Of PasswordBox)(pill).FirstOrDefault()
            If pb IsNot Nothing AndAlso pb.Password <> "" Then Return pb.Password
            Dim tb = FindAll(Of TextBox)(pill).FirstOrDefault()
            Return If(tb IsNot Nothing, tb.Text, "")
        End Function

        Private Shared Sub ClearPill(pill As DependencyObject)
            For Each pb In FindAll(Of PasswordBox)(pill) : pb.Password = "" : Next
            For Each tb In FindAll(Of TextBox)(pill) : tb.Text = "" : Next
        End Sub

        Private Shared Function FindAll(Of T As DependencyObject)(root As DependencyObject) As List(Of T)
            Dim result As New List(Of T)
            CollectAll(root, result)
            Return result
        End Function
        Private Shared Sub CollectAll(Of T As DependencyObject)(root As DependencyObject, result As List(Of T))
            For i = 0 To VisualTreeHelper.GetChildrenCount(root) - 1
                Dim child = VisualTreeHelper.GetChild(root, i)
                Dim hit = TryCast(child, T)
                If hit IsNot Nothing Then result.Add(hit)
                CollectAll(child, result)
            Next
        End Sub
    End Class
End Namespace
