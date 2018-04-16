using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace XPOServer {
    public partial class MainWindow: Window {
        public static readonly DependencyProperty CollectionProperty = 
            DependencyProperty.Register("Collection", typeof(XPServerCollectionSource), typeof(MainWindow), new PropertyMetadata(null));
        public XPServerCollectionSource Collection {
            get { return (XPServerCollectionSource)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }
        public MainWindow() {
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(AccessConnectionProvider.GetConnectionString(@"..\..\Database.mdb"), AutoCreateOption.DatabaseAndSchema);
            Collection = new XPServerCollectionSource(new UnitOfWork(), typeof(Items)) {
                AllowNew = true,
                AllowRemove = true,
                AllowEdit = true,
                TrackChanges = true,
                DeleteObjectOnRemove = true
            };
            Collection.DisplayableProperties = "Id;Name"; 
            DataContext = this;
            InitializeComponent();
        }
    }
    public class Items: XPObject {
        public Items(Session session) : base(session) { }
        public Items() { }
        int fId;
        public int Id {
            get { return fId; }
            set { SetPropertyValue("Id", ref fId, value); }
        }
        string fName;
        public string Name {
            get { return fName; }
            set { SetPropertyValue("Name", ref fName, value); }
        }
    }
}