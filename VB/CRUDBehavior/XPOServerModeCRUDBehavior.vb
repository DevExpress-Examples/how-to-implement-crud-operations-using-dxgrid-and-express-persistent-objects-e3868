Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Interactivity
Imports System.Windows
Imports DevExpress.Xpf.Grid
Imports System.Windows.Controls
Imports System.Windows.Input
Imports DevExpress.Xpf.Core
Imports System.ComponentModel
Imports DevExpress.Xpo
Imports CRUDBehaviorBase

Namespace XPOServer
    Public Class XPOServerModeCRUDBehavior
        Inherits CRUDBehaviorBase.CRUDBehaviorBase

        Protected Overrides Function CanExecuteRemoveRowCommand() As Boolean
            If CollectionSource Is Nothing OrElse Grid Is Nothing OrElse View Is Nothing OrElse View.FocusedRow Is Nothing Then
                Return False
            End If
            Return True
        End Function
        Protected Overrides Sub OnAttached()
            MyBase.OnAttached()
            If View IsNot Nothing AndAlso CollectionSource IsNot Nothing Then
                Initialize()
            Else
                AddHandler Grid.Loaded, AddressOf OnGridLoaded
            End If
        End Sub
        Protected Overrides Sub Initialize()
            Grid.ItemsSource = CollectionSource
            NewRowCommand.RaiseCanExecuteChangedEvent()
            RemoveRowCommand.RaiseCanExecuteChangedEvent()
            EditRowCommand.RaiseCanExecuteChangedEvent()
            MyBase.Initialize()
        End Sub
        Protected Overrides Sub UpdateDataSource()
            CollectionSource.Reload()
        End Sub
    End Class
End Namespace