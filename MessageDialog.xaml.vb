Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media

Namespace SidhawanForStudents

    Partial Public Class MessageDialog
        Inherits UserControl

        Private _onYes As Action

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub ShowConfirm(before As String, bold As String, after As String, onYes As Action)
            TermsPanel.Visibility = Visibility.Collapsed
            _onYes = onYes
            ConfirmText.Inlines.Clear()
            ConfirmText.Inlines.Add(New Run(before))

            Dim boldRun = New Run(bold)
            boldRun.FontWeight = FontWeights.Bold
            boldRun.Foreground = CType(FindResource("Brand"), Brush)
            ConfirmText.Inlines.Add(boldRun)

            ConfirmText.Inlines.Add(New Run(after))

            ConfirmPanel.Visibility = Visibility.Visible
            NoticePanel.Visibility = Visibility.Collapsed
            Me.Visibility = Visibility.Visible
        End Sub

        ''' <param name="kind">"check", "lock" or "unlock"</param>
        Public Sub ShowNotice(kind As String, message As String)
            TermsPanel.Visibility = Visibility.Collapsed
            NoticeText.Text = message
            BuildIcon(kind)

            NoticePanel.Visibility = Visibility.Visible
            ConfirmPanel.Visibility = Visibility.Collapsed
            Me.Visibility = Visibility.Visible
        End Sub

        Private Sub BuildIcon(kind As String)
            IconHost.Children.Clear()
            Dim ic = New Icon()
            ic.File = kind
            IconHost.Children.Add(ic)
        End Sub

        Private Sub Close_Click(sender As Object, e As RoutedEventArgs)
            Me.Visibility = Visibility.Collapsed
        End Sub

        Private Sub Yes_Click(sender As Object, e As RoutedEventArgs)
            Me.Visibility = Visibility.Collapsed
            Dim action As Action = _onYes
            _onYes = Nothing
            If action IsNot Nothing Then action()
        End Sub
        Public Sub ShowText(title As String, body As String)
            NoticePanel.Visibility = Visibility.Collapsed
            ConfirmPanel.Visibility = Visibility.Collapsed
            TermsPanel.Visibility = Visibility.Visible
            TermsTitle.Text = title
            TermsText.Text = body
            Visibility = Visibility.Visible
        End Sub

    End Class
End Namespace