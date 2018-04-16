Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows.Interactivity
Imports DevExpress.Xpf.Grid
Imports System.Windows.Input
Imports System.Windows
Imports DevExpress.Xpf.Core.ServerMode
Imports System.Data.Linq
Imports DevExpress.Xpf.Core
Imports System.Windows.Controls
Imports DevExpress.Xpf.Bars
Imports DevExpress.Xpo
Imports System.ComponentModel

Namespace CRUDBehaviorBase
    Public Class CRUDBehaviorBase
        Inherits Behavior(Of GridControl)

        Public Shared ReadOnly NewRowFormProperty As DependencyProperty = DependencyProperty.Register("NewRowForm", GetType(DataTemplate), GetType(CRUDBehaviorBase), New PropertyMetadata(Nothing))
        Public Shared ReadOnly EditRowFormProperty As DependencyProperty = DependencyProperty.Register("EditRowForm", GetType(DataTemplate), GetType(CRUDBehaviorBase), New PropertyMetadata(Nothing))
        Public Shared ReadOnly XPObjectTypeProperty As DependencyProperty = DependencyProperty.Register("XPObjectType", GetType(Type), GetType(CRUDBehaviorBase), New PropertyMetadata(Nothing))
        Public Shared ReadOnly AllowKeyDownActionsProperty As DependencyProperty = DependencyProperty.Register("AllowKeyDownActions", GetType(Boolean), GetType(CRUDBehaviorBase), New PropertyMetadata(False))
        Public Shared ReadOnly CollectionSourceProperty As DependencyProperty = DependencyProperty.Register("CollectionSource", GetType(XPServerCollectionSource), GetType(CRUDBehaviorBase), New PropertyMetadata(Nothing))
        Public Shared ReadOnly PrimaryKeyProperty As DependencyProperty = DependencyProperty.Register("PrimaryKey", GetType(String), GetType(CRUDBehaviorBase), New PropertyMetadata(String.Empty))

        Public Property CollectionSource() As XPServerCollectionSource
            Get
                Return CType(GetValue(CollectionSourceProperty), XPServerCollectionSource)
            End Get
            Set(ByVal value As XPServerCollectionSource)
                SetValue(CollectionSourceProperty, value)
            End Set
        End Property
        Public Property NewRowForm() As DataTemplate
            Get
                Return CType(GetValue(NewRowFormProperty), DataTemplate)
            End Get
            Set(ByVal value As DataTemplate)
                SetValue(NewRowFormProperty, value)
            End Set
        End Property
        Public Property EditRowForm() As DataTemplate
            Get
                Return CType(GetValue(EditRowFormProperty), DataTemplate)
            End Get
            Set(ByVal value As DataTemplate)
                SetValue(EditRowFormProperty, value)
            End Set
        End Property
        Public Property XPObjectType() As Type
            Get
                Return CType(GetValue(XPObjectTypeProperty), Type)
            End Get
            Set(ByVal value As Type)
                SetValue(XPObjectTypeProperty, value)
            End Set
        End Property
        Public Property PrimaryKey() As String
            Get
                Return CStr(GetValue(PrimaryKeyProperty))
            End Get
            Set(ByVal value As String)
                SetValue(PrimaryKeyProperty, value)
            End Set
        End Property
        Public Property AllowKeyDownActions() As Boolean
            Get
                Return CBool(GetValue(AllowKeyDownActionsProperty))
            End Get
            Set(ByVal value As Boolean)
                SetValue(AllowKeyDownActionsProperty, value)
            End Set
        End Property

        Public ReadOnly Property Grid() As GridControl
            Get
                Return AssociatedObject
            End Get
        End Property
        Public ReadOnly Property View() As TableView
            Get
                Return If(Grid IsNot Nothing, CType(Grid.View, TableView), Nothing)
            End Get
        End Property

        #Region "Commands"
        Private privateNewRowCommand As CustomCommand
        Public Property NewRowCommand() As CustomCommand
            Get
                Return privateNewRowCommand
            End Get
            Private Set(ByVal value As CustomCommand)
                privateNewRowCommand = value
            End Set
        End Property
        Private privateRemoveRowCommand As CustomCommand
        Public Property RemoveRowCommand() As CustomCommand
            Get
                Return privateRemoveRowCommand
            End Get
            Private Set(ByVal value As CustomCommand)
                privateRemoveRowCommand = value
            End Set
        End Property
        Private privateEditRowCommand As CustomCommand
        Public Property EditRowCommand() As CustomCommand
            Get
                Return privateEditRowCommand
            End Get
            Private Set(ByVal value As CustomCommand)
                privateEditRowCommand = value
            End Set
        End Property
        Protected Overridable Sub ExecuteNewRowCommand()
            AddNewRow()
        End Sub
        Protected Overridable Function CanExecuteNewRowCommand() As Boolean
            Return True
        End Function
        Protected Overridable Sub ExecuteRemoveRowCommand()
            RemoveSelectedRows()
        End Sub
        Protected Overridable Function CanExecuteRemoveRowCommand() As Boolean
            Return True
        End Function
        Protected Overridable Sub ExecuteEditRowCommand()
            EditRow()
        End Sub
        Protected Overridable Function CanExecuteEditRowCommand() As Boolean
            Return CanExecuteRemoveRowCommand()
        End Function
        #End Region

        Public Sub New()
            NewRowCommand = New CustomCommand(AddressOf ExecuteNewRowCommand, AddressOf CanExecuteNewRowCommand)
            RemoveRowCommand = New CustomCommand(AddressOf ExecuteRemoveRowCommand, AddressOf CanExecuteRemoveRowCommand)
            EditRowCommand = New CustomCommand(AddressOf ExecuteEditRowCommand, AddressOf CanExecuteEditRowCommand)
        End Sub
        Public Overridable Function CreateNewRow() As Object
            Return Activator.CreateInstance(XPObjectType, CollectionSource.Session)
        End Function
        Public Overridable Sub AddNewRow(ByVal newRow As Object)
            AddNewRowIntoCollectionSource(CollectionSource, newRow)
        End Sub
        Public Overridable Sub AddNewRowIntoCollectionSource(ByVal collectionSource As XPServerCollectionSource, ByVal newRow As Object)
            If collectionSource Is Nothing Then
                Return
            End If
            DirectCast(DirectCast(collectionSource, IListSource).GetList(), IBindingList).Add(newRow)
            collectionSource.Session.CommitTransaction()
            UpdateDataSource()
        End Sub
        Public Overridable Sub AddNewRow()
            CollectionSource.Session.BeginTransaction()
            Dim dialog As DXWindow = CreateDialogWindow(CreateNewRow(), False)
            AddHandler dialog.Closed, AddressOf OnNewRowDialogClosed
            dialog.ShowDialog()
        End Sub
        Public Overridable Sub RemoveRow()
            If CollectionSource Is Nothing OrElse PrimaryKey = String.Empty Then
                Return
            End If
            For i As Integer = 0 To (DirectCast(CollectionSource, IListSource).GetList().Count) - 1
                If DirectCast(DirectCast(CollectionSource, IListSource).GetList()(i), XPObject).GetMemberValue(PrimaryKey).ToString() = Grid.GetFocusedRowCellValue(PrimaryKey).ToString() Then
                    DirectCast(DirectCast(CollectionSource, IListSource).GetList(), IBindingList).RemoveAt(i)
                    Exit For
                End If
            Next i
        End Sub
        Public Overridable Sub RemoveSelectedRows()
            CollectionSource.Reload()
            CollectionSource.Session.BeginTransaction()
            Dim selectedRowsHandles() As Integer = Grid.GetSelectedRowHandles()
            If selectedRowsHandles IsNot Nothing OrElse selectedRowsHandles.Length > 0 Then
                Dim rowKeys As New List(Of Object)()
                For Each index As Integer In selectedRowsHandles
                    rowKeys.Add(Grid.GetCellValue(index, PrimaryKey))
                Next index
                For i As Integer = 0 To (DirectCast(CollectionSource, IListSource).GetList().Count) - 1
                    If rowKeys.Contains(DirectCast(DirectCast(CollectionSource, IListSource).GetList()(i), XPObject).GetMemberValue(PrimaryKey)) Then
                        DirectCast(DirectCast(CollectionSource, IListSource).GetList(), IBindingList).RemoveAt(i)
                        i -= 1
                    End If
                Next i
            ElseIf Grid.CurrentItem IsNot Nothing Then
                RemoveRow()
            End If
            CollectionSource.Session.CommitTransaction()
            CollectionSource.Session.PurgeDeletedObjects()
            UpdateDataSource()
        End Sub
        Public Overridable Sub EditRow()
            If View Is Nothing OrElse Grid.CurrentItem Is Nothing OrElse PrimaryKey = String.Empty Then
                Return
            End If
            For i As Integer = 0 To (DirectCast(CollectionSource, IListSource).GetList().Count) - 1
                If DirectCast(DirectCast(CollectionSource, IListSource).GetList()(i), XPObject).GetMemberValue(PrimaryKey).ToString() = Grid.GetFocusedRowCellValue(PrimaryKey).ToString() Then
                    Dim dialog As DXWindow = CreateDialogWindow(DirectCast(CollectionSource, IListSource).GetList()(i), True)
                    AddHandler dialog.Closed, AddressOf OnEditRowDialogClosed
                    dialog.ShowDialog()
                    Exit For
                End If
            Next i
        End Sub
        Protected Overridable Function CreateDialogWindow(ByVal content As Object, Optional ByVal isEditingMode As Boolean = False) As DXWindow
            Dim dialog As DXDialog = New DXDialog With {.Tag = content, .Buttons = DialogButtons.OkCancel, .Title = If(isEditingMode, "Edit Row", "Add New Row"), .SizeToContent = SizeToContent.WidthAndHeight}
            Dim c As ContentControl = New ContentControl With {.Content = content}
            If isEditingMode Then
                dialog.Title = "Edit Row"
                c.ContentTemplate = EditRowForm
            Else
                dialog.Title = "Add New Row"
                c.ContentTemplate = NewRowForm
            End If
            dialog.Content = c
            Return dialog
        End Function
        Protected Overridable Sub OnNewRowDialogClosed(ByVal sender As Object, ByVal e As EventArgs)
            RemoveHandler DirectCast(sender, DXWindow).Closed, AddressOf OnNewRowDialogClosed
            If CBool(DirectCast(sender, DXWindow).DialogResult) Then
                AddNewRow(DirectCast(sender, DXWindow).Tag)
            End If
        End Sub
        Protected Overridable Sub OnEditRowDialogClosed(ByVal sender As Object, ByVal e As EventArgs)
            RemoveHandler DirectCast(sender, DXWindow).Closed, AddressOf OnEditRowDialogClosed
            If CBool(DirectCast(sender, DXWindow).DialogResult) Then
                CollectionSource.Session.CommitTransaction()
            Else
                CollectionSource.Session.RollbackTransaction()
                CollectionSource.Session.Reload(DirectCast(sender, DXWindow).Tag, True)
            End If
            UpdateDataSource()
        End Sub
        Protected Overridable Sub OnViewKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
            If Not AllowKeyDownActions Then
                Return
            End If
            If e.Key = Key.Delete Then
                RemoveSelectedRows()
                e.Handled = True
            End If
            If e.Key = Key.Enter Then
                EditRow()
                e.Handled = True
            End If
        End Sub
        Protected Overridable Sub OnViewRowDoubleClick(ByVal sender As Object, ByVal e As RowDoubleClickEventArgs)
            EditRow()
            e.Handled = True
        End Sub
        Protected Overridable Sub OnGridLoaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            RemoveHandler Grid.Loaded, AddressOf OnGridLoaded
            Initialize()
        End Sub
        Protected Overridable Sub OnGridCurrentItemChanged(ByVal sender As Object, ByVal e As CurrentItemChangedEventArgs)
            UpdateCommands()
        End Sub
        Protected Overridable Sub UpdateCommands()
            RemoveRowCommand.RaiseCanExecuteChangedEvent()
            EditRowCommand.RaiseCanExecuteChangedEvent()
        End Sub
        Protected Overrides Sub OnAttached()
            MyBase.OnAttached()
        End Sub
        Protected Overrides Sub OnDetaching()
            Uninitialize()
            MyBase.OnDetaching()
        End Sub
        Protected Overridable Sub Initialize()
            AddHandler View.KeyDown, AddressOf OnViewKeyDown
            AddHandler View.RowDoubleClick, AddressOf OnViewRowDoubleClick
            AddHandler Grid.CurrentItemChanged, AddressOf OnGridCurrentItemChanged
            UpdateCommands()
        End Sub
        Protected Overridable Sub Uninitialize()
            RemoveHandler View.KeyDown, AddressOf OnViewKeyDown
            RemoveHandler View.RowDoubleClick, AddressOf OnViewRowDoubleClick
            RemoveHandler Grid.CurrentItemChanged, AddressOf OnGridCurrentItemChanged
        End Sub
        Protected Overridable Sub UpdateDataSource()
        End Sub
    End Class
    Public Class CustomCommand
        Implements ICommand

        Private _executeMethod As Action
        Private _canExecuteMethod As Func(Of Boolean)
        Public Sub New(ByVal executeMethod As Action, ByVal canExecuteMethod As Func(Of Boolean))
            _executeMethod = executeMethod
            _canExecuteMethod = canExecuteMethod
        End Sub
        Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _canExecuteMethod()
        End Function
        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
        Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
            _executeMethod()
        End Sub
        Public Sub RaiseCanExecuteChangedEvent()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace