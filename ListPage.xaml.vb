Imports System
Imports System.Windows
Imports System.Windows.Controls

Namespace SidhawanForStudents


    Public Partial Class ListPage
        Inherits UserControl
        Private _back, _forward, _edit As Action

        Public ReadOnly Property Popup As SidhawanForStudents.MessageDialog
            Get
                Return Me.PopupDialog
            End Get
        End Property

        Public Sub New(title As String)
            Me.InitializeComponent()
            Me.TitleText.Text = title
        End Sub

        Public Function BackTo(page As Func(Of UserControl)) As ListPage
            Return Back(Sub() [GoTo](page()))
        End Function
        Public Function Back(onClick As Action) As ListPage
            _back = onClick
            Me.BackBtn.Visibility = Visibility.Visible
            Return Me
        End Function
        Public Function Forward(onClick As Action) As ListPage
            _forward = onClick
            Me.NextBtn.Visibility = Visibility.Visible
            Return Me
        End Function
        Public Function Pencil(Optional onClick As Action = Nothing) As ListPage
            _edit = onClick
            Me.EditBtn.Visibility = Visibility.Visible
            Return Me
        End Function

        Public Sub ShowArrows(back As Boolean, forward As Boolean)
            Me.BackBtn.Visibility = If(back, Visibility.Visible, Visibility.Collapsed)
            Me.NextBtn.Visibility = If(forward, Visibility.Visible, Visibility.Collapsed)
        End Sub

        Public Function Filter(ParamArray combos As ComboBox()) As ListPage
            For Each c In combos
                Me.FilterBar.Children.Add(c)
            Next

            Return Me
        End Function
        Public Function Tool(pTool As UIElement) As ListPage
            Me.ToolHost.Content = pTool
            Return Me
        End Function
        Private _light As Boolean

        Public Function Light() As ListPage
            _light = True
            Return Me
        End Function

        Public Function Band(text As String) As ListPage
            Me.BandHost.Content = SidhawanForStudents.Ui.Band(text, _light)
            Return Me
        End Function

        Public Function Band(text As Func(Of String), ParamArray watch As ComboBox()) As ListPage
            Dim lBand = SidhawanForStudents.Ui.Band(text(), _light)
            Dim label = CType(lBand.Child, TextBlock)
            BandHost.Visibility = If(String.IsNullOrWhiteSpace(label.Text), Visibility.Collapsed, Visibility.Visible)
            For Each c In watch
                AddHandler c.SelectionChanged,
                    Sub(src, args)
                        label.Text = text()
                        BandHost.Visibility = If(String.IsNullOrWhiteSpace(label.Text), Visibility.Collapsed, Visibility.Visible)
                    End Sub
            Next
            Me.BandHost.Content = lBand
            Return Me
        End Function
        Public Function Table(content As UIElement) As ListPage
            Me.BodyHost.Content = content
            Return Me
        End Function

        Public Function Plain() As ListPage
            Me.Box.BorderThickness = New Thickness(0)
            Return Me
        End Function
        Public Function SmallTitle() As ListPage
            Me.TitleText.FontSize = 21
            Return Me
        End Function
        Public Function Gap(top As Double) As ListPage
            Me.Box.Margin = New Thickness(0, top, 0, 0)
            Return Me
        End Function

        Public Function Wide(Optional filterIndent As Double = 0) As ListPage
            Me.Box.Margin = New Thickness(-13, Me.Box.Margin.Top, 30, 0)
            Me.FilterBar.Margin = New Thickness(filterIndent, 0, 0, 0)
            Me.ToolStrip.Margin = New Thickness(0, -12, -25, 0)
            Return Me
        End Function

        Public Sub [GoTo](page As UserControl)
            TryCast(Window.GetWindow(Me), SidhawanForStudents.Window2)?.Navigate(page)
        End Sub

        Private Sub Back_Click(sender As Object, e As RoutedEventArgs)
            _back?.Invoke()
        End Sub
        Private Sub Next_Click(sender As Object, e As RoutedEventArgs)
            _forward?.Invoke()
        End Sub
        Private Sub Edit_Click(sender As Object, e As RoutedEventArgs)
            If _edit IsNot Nothing Then
                _edit()
            Else
                SidhawanForStudents.Ui.ToggleEdit(Me)
            End If
        End Sub
    End Class
End Namespace
