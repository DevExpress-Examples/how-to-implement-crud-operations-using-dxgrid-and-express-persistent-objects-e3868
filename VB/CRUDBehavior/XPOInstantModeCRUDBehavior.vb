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

Namespace XPOInstant
    Public Class XPOInstantModeCRUDBehavior
        Inherits CRUDBehaviorBase.CRUDBehaviorBase

        Public Shared ReadOnly InstantCollectionSourceProperty As DependencyProperty = DependencyProperty.Register("InstantCollectionSource", GetType(XPInstantFeedbackSource), GetType(XPOInstantModeCRUDBehavior), New PropertyMetadata(Nothing))
        Public Property InstantCollectionSource() As XPInstantFeedbackSource
            Get
                Return CType(GetValue(InstantCollectionSourceProperty), XPInstantFeedbackSource)
            End Get
            Set(ByVal value As XPInstantFeedbackSource)
                SetValue(InstantCollectionSourceProperty, value)
            End Set
        End Property
        Protected Overrides Function CanExecuteRemoveRowCommand() As Boolean
            If CollectionSource Is Nothing OrElse Grid Is Nothing OrElse View Is Nothing OrElse Grid.CurrentItem Is Nothing Then
                Return False
            End If
            Return True
        End Function
        Protected Overrides Sub Initialize()
            Grid.ItemsSource = InstantCollectionSource
            NewRowCommand.RaiseCanExecuteChangedEvent()
            CollectionSource = New XPServerCollectionSource(New UnitOfWork(), InstantCollectionSource.ObjectType) With {.AllowNew = True, .AllowRemove = True, .AllowEdit = True, .TrackChanges = True, .DeleteObjectOnRemove = True}
            MyBase.Initialize()
        End Sub
        Protected Overrides Sub UpdateDataSource()
            InstantCollectionSource.Refresh()
        End Sub
        Protected Overrides Sub OnAttached()
            MyBase.OnAttached()
            If View IsNot Nothing AndAlso InstantCollectionSource IsNot Nothing Then
                Initialize()
            Else
                AddHandler Grid.Loaded, AddressOf OnGridLoaded
            End If
        End Sub
    End Class
End Namespace