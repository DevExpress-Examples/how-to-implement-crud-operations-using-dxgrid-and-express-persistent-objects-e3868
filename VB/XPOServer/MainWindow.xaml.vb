Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB

Namespace XPOServer
    Partial Public Class MainWindow
        Inherits Window

        Public Shared ReadOnly CollectionProperty As DependencyProperty = DependencyProperty.Register("Collection", GetType(XPServerCollectionSource), GetType(MainWindow), New PropertyMetadata(Nothing))
        Public Property Collection() As XPServerCollectionSource
            Get
                Return DirectCast(GetValue(CollectionProperty), XPServerCollectionSource)
            End Get
            Set(ByVal value As XPServerCollectionSource)
                SetValue(CollectionProperty, value)
            End Set
        End Property
        Public Sub New()
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(AccessConnectionProvider.GetConnectionString("..\..\Database.mdb"), AutoCreateOption.DatabaseAndSchema)
            Collection = New XPServerCollectionSource(New UnitOfWork(), GetType(Items)) With {.AllowNew = True, .AllowRemove = True, .AllowEdit = True, .TrackChanges = True, .DeleteObjectOnRemove = True}
            Collection.DisplayableProperties = "Id;Name"
            DataContext = Me
            InitializeComponent()
        End Sub
    End Class
    Public Class Items
        Inherits XPObject

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Sub New()
        End Sub
        Private fId As Integer
        Public Property Id() As Integer
            Get
                Return fId
            End Get
            Set(ByVal value As Integer)
                SetPropertyValue("Id", fId, value)
            End Set
        End Property
        Private fName As String
        Public Property Name() As String
            Get
                Return fName
            End Get
            Set(ByVal value As String)
                SetPropertyValue("Name", fName, value)
            End Set
        End Property
    End Class
End Namespace