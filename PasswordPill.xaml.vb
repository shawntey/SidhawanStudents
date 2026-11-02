Imports System.Windows
Imports System.Windows.Controls

Namespace SidhawanForStudents

    Partial Public Class PasswordPill
        Inherits UserControl

        Public Shared ReadOnly HintProperty As DependencyProperty =
            DependencyProperty.Register("Hint", GetType(String), GetType(PasswordPill),
                New PropertyMetadata("", New PropertyChangedCallback(AddressOf OnHintChanged)))

        Public Property Hint As String
            Get
                Return CStr(GetValue(HintProperty))
            End Get
            Set(value As String)
                SetValue(HintProperty, value)
            End Set
        End Property

        Public ReadOnly Property Password As String
            Get
                Return Hidden.Password
            End Get
        End Property

        Private _syncing As Boolean

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Shared Sub OnHintChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
            CType(d, PasswordPill).HintText.Text = CStr(e.NewValue)
        End Sub

        Private Sub Hidden_PasswordChanged(sender As Object, e As RoutedEventArgs)
            If _syncing Then Return
            _syncing = True
            Shown.Text = Hidden.Password
            _syncing = False
            UpdateHint()
        End Sub

        Private Sub Shown_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _syncing Then Return
            _syncing = True
            Hidden.Password = Shown.Text
            _syncing = False
            UpdateHint()
        End Sub

        Private Sub UpdateHint()
            If Hidden.Password.Length = 0 Then
                HintText.Visibility = Visibility.Visible
            Else
                HintText.Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Sub Eye_Click(sender As Object, e As RoutedEventArgs)
            Dim show As Boolean = Eye.IsChecked.GetValueOrDefault()
            If show Then
                Shown.Visibility = Visibility.Visible
                Hidden.Visibility = Visibility.Collapsed
                Shown.Focus()
            Else
                Shown.Visibility = Visibility.Collapsed
                Hidden.Visibility = Visibility.Visible
                Hidden.Focus()
            End If
        End Sub

    End Class
End Namespace