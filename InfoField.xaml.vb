Imports System.Windows
Imports System.Windows.Controls

Namespace SidhawanForStudents

    Partial Public Class InfoField
        Inherits UserControl

        Public Shared ReadOnly LabelProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(InfoField.Label), GetType(String), GetType(InfoField),
            New PropertyMetadata("", Sub(d, e) CType(d, InfoField).LabelText.Text = CStr(e.NewValue)))

        Public Shared ReadOnly ValueProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(InfoField.Value), GetType(String), GetType(InfoField),
            New PropertyMetadata("", Sub(d, e) CType(d, InfoField).ValueBox.Text = CStr(e.NewValue)))

        Public Shared ReadOnly IsEditableProperty As DependencyProperty = DependencyProperty.Register(
            NameOf(InfoField.IsEditable), GetType(Boolean), GetType(InfoField),
            New PropertyMetadata(False, Sub(d, e) CType(d, InfoField).ValueBox.IsReadOnly = Not CBool(e.NewValue)))

        Public Property Label As String
            Get
                Return CStr(GetValue(LabelProperty))
            End Get
            Set(value As String)
                SetValue(LabelProperty, value)
            End Set
        End Property

        Public Property Value As String
            Get
                Return CStr(GetValue(ValueProperty))
            End Get
            Set(value As String)
                SetValue(ValueProperty, value)
            End Set
        End Property

        Public Property IsEditable As Boolean
            Get
                Return CBool(GetValue(IsEditableProperty))
            End Get
            Set(value As Boolean)
                SetValue(IsEditableProperty, value)
            End Set
        End Property

        Public Sub New()
            InitializeComponent()
            ValueBox.IsReadOnly = True

            AddHandler ValueBox.TextChanged, Sub(src, args) Value = ValueBox.Text
        End Sub
    End Class
End Namespace