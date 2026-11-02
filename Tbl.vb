Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Threading

Namespace SidhawanForStudents
    Public Class Tbl
        Private ReadOnly _grid As DataGrid = New DataGrid With {
            .Style = SidhawanForStudents.Ui.S("GreenGrid")
        }
        Private ReadOnly _links As Dictionary(Of DataGridColumn, Action(Of Object)) = New Dictionary(Of DataGridColumn, Action(Of Object))()
        Private ReadOnly _editCols As List(Of DataGridColumn) = New List(Of DataGridColumn)()
        Private _boldRows As Boolean

        Private Sub New(rows As IEnumerable)
            _grid.ItemsSource = rows
        End Sub

        Public Shared Function [For](rows As IEnumerable) As Tbl
            Return New Tbl(rows)
        End Function

        Public Function Col(header As String, path As String, width As Double, Optional left As Boolean = False, Optional bold As Boolean = False) As Tbl
            Add(header, path, width, left, edit:=False, bold:=bold)
            Return Me
        End Function

        Public Function Link(header As String, path As String, width As Double, onClick As Action(Of Object), Optional left As Boolean = False) As Tbl
            _links(Add(header, path, width, left, edit:=False, link:=True)) = onClick
            Return Me
        End Function

        Public Function Edit(header As String, path As String, width As Double, Optional left As Boolean = False, Optional locked As Boolean = False) As Tbl
            Dim c = Add(header, path, width, left, edit:=True)
            c.IsReadOnly = locked
            _editCols.Add(c)
            Return Me
        End Function

        Public Function Cell(header As String, templateKey As String, width As Double, Optional onClick As Action(Of Object) = Nothing) As Tbl
            Dim c = New DataGridTemplateColumn With {
    .header = header,
    .width = Star(width),
    .IsReadOnly = True,
    .CellTemplate = SidhawanForStudents.Ui.R(Of DataTemplate)(templateKey)
}
            _grid.Columns.Add(c)
            If onClick IsNot Nothing Then _links(c) = onClick
            Return Me
        End Function

        Public Function Matrix(firstHeader As String, firstWidth As Double, headers As IEnumerable(Of String), widths As Double(), Optional edit As Boolean = False, Optional lockLast As Integer = 0) As Tbl
            Dim list = headers.ToList()
            Add(firstHeader, "Label", firstWidth, left:=True, edit:=False)
            For i = 0 To list.Count - 1
                Add(list(i), "Cells[" & i.ToString() & "]", widths(Math.Min(i, widths.Length - 1)), left:=False, edit:=edit AndAlso i < list.Count - lockLast)
            Next
            Return Me
        End Function

        Public Function Head(styleKey As String) As Tbl
            _grid.ColumnHeaderStyle = SidhawanForStudents.Ui.S(styleKey)
            Return Me
        End Function
        Public Function HeadH(height As Double) As Tbl
            _grid.ColumnHeaderHeight = height
            Return Me
        End Function
        Public Function RowH(height As Double) As Tbl
            _grid.MinRowHeight = height
            Return Me
        End Function
        Public Function NoHeader() As Tbl
            _grid.HeadersVisibility = DataGridHeadersVisibility.None
            Return Me
        End Function
        Public Function BoldRows() As Tbl
            _boldRows = True
            Return Me
        End Function

        Public Function Big() As Tbl
            _grid.FontFamily = New FontFamily("Verdana")
            _grid.FontSize = 16
            _grid.MinRowHeight = 47
            Return Me
        End Function

        Public Function Plain() As Tbl
            _grid.BorderThickness = New Thickness(0)
            _grid.CellStyle = SidhawanForStudents.Ui.S("GridCellLines")
            _grid.ColumnHeaderStyle = SidhawanForStudents.Ui.S("GridHeaderPlain")
            Return Me
        End Function

        Public Function Build() As DataGrid
            If _boldRows Then
                For Each c In _grid.Columns.OfType(Of DataGridTextColumn)()
                    Dim style = New Style(GetType(TextBlock), c.ElementStyle)
                    Dim trigger = New DataTrigger()
                    trigger.Binding = New Binding("Bold")
                    trigger.Value = True
                    trigger.Setters.Add(New Setter(TextBlock.FontWeightProperty, FontWeights.Bold))
                    style.Triggers.Add(trigger)
                    c.ElementStyle = style
                Next
            End If

            _grid.Tag = _editCols

            AddHandler _grid.PreviewMouseLeftButtonUp, Sub(s, e)
                                                           Dim cell As DataGridCell = FindUp(Of DataGridCell)(TryCast(e.OriginalSource, DependencyObject))
                                                           If cell Is Nothing Then Return
                                                           Dim go As Action(Of Object) = Nothing

                                                           If _links.TryGetValue(cell.Column, go) AndAlso cell.DataContext IsNot Nothing Then
                                                               go(cell.DataContext)
                                                           End If
                                                       End Sub

            _grid.AddHandler(UIElement.GotFocusEvent, New RoutedEventHandler(Sub(s, e)
                                                                                 Dim c As DataGridCell = TryCast(e.OriginalSource, DataGridCell)
                                                                                 If c IsNot Nothing AndAlso Not c.IsReadOnly AndAlso Not c.IsEditing Then
                                                                                     _grid.BeginEdit(e)
                                                                                 End If
                                                                             End Sub))

            AddHandler _grid.PreviewMouseWheel, Sub(s, e)
                                                    Dim parent As UIElement = TryCast(_grid.Parent, UIElement)
                                                    If e.Handled OrElse parent Is Nothing Then Return
                                                    e.Handled = True
                                                    Dim args = New MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                                                    args.RoutedEvent = UIElement.MouseWheelEvent
                                                    args.Source = s
                                                    parent.RaiseEvent(args)
                                                End Sub

            AddHandler _grid.CellEditEnding, Sub(s, e)
                                                 If e.EditAction <> DataGridEditAction.Commit Then Return
                                                 Dim item = e.Row.Item
                                                 Dim column = ""
                                                 If e.Column.Header IsNot Nothing Then column = e.Column.Header.ToString()
                                                 _grid.Dispatcher.BeginInvoke(New Action(Sub() SidhawanForStudents.Data.Edited(item, column)), DispatcherPriority.Background)
                                             End Sub

            Return _grid
        End Function

        Public Shared Sub ToggleEdit(g As DataGrid)
            SetLocked(g, Nothing)
        End Sub

        Public Shared Sub Lock(g As DataGrid)
            SetLocked(g, True)
        End Sub

        Private Shared Sub SetLocked(g As DataGrid, locked As Boolean?)
            g.CommitEdit(DataGridEditingUnit.Row, True)
            Dim cols As List(Of DataGridColumn) = TryCast(g.Tag, List(Of DataGridColumn))
            If cols IsNot Nothing Then
                For Each c In cols
                    c.IsReadOnly = If(locked.HasValue, locked.Value, Not c.IsReadOnly)
                Next
            End If
        End Sub

        Private Function Add(header As String, path As String, width As Double, left As Boolean, edit As Boolean, Optional link As Boolean = False, Optional bold As Boolean = False) As DataGridTextColumn
            Dim style = SidhawanForStudents.Ui.S(If(link, If(left, "GridLinkLeft", "GridLink"), If(left, "GridTextLeft", "GridText")))
            If bold Then
                style = New Style(GetType(TextBlock), style)
                style.Setters.Add(New Setter(TextBlock.FontWeightProperty, FontWeights.Bold))
            End If

            Dim binding = New Binding(path)
            binding.Mode = If(edit, BindingMode.TwoWay, BindingMode.OneWay)

            Dim col = New DataGridTextColumn With {
    .header = header,
    .binding = binding,
    .width = Star(width),
    .IsReadOnly = Not edit,
    .ElementStyle = style,
    .EditingElementStyle = SidhawanForStudents.Ui.S("GridEditBox")
}
            _grid.Columns.Add(col)
            Return col
        End Function

        Private Shared Function Star(w As Double) As DataGridLength
            Return New DataGridLength(w, DataGridLengthUnitType.Star)
        End Function

        Private Shared Function FindUp(Of T As DependencyObject)(d As DependencyObject) As T
            While d IsNot Nothing AndAlso Not (TypeOf d Is T)
                If TypeOf d Is Visual Then
                    d = VisualTreeHelper.GetParent(d)
                Else
                    d = LogicalTreeHelper.GetParent(d)
                End If
            End While
            Return TryCast(d, T)
        End Function
    End Class
End Namespace
