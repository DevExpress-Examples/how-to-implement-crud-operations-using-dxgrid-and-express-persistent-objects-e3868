using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace XPOInstant {
    public partial class MainWindow: Window {
        public static readonly DependencyProperty CollectionProperty = 
            DependencyProperty.Register("Collection", typeof(XPInstantFeedbackSource), typeof(MainWindow), new UIPropertyMetadata(null));
        public XPInstantFeedbackSource Collection {
            get { return (XPInstantFeedbackSource)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }
        public MainWindow() {
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(AccessConnectionProvider.GetConnectionString(@"..\..\Database.mdb"), AutoCreateOption.DatabaseAndSchema);
            DataContext = this;
            InitializeComponent();

            Collection = new XPInstantFeedbackSource(typeof(Items));
            Collection.DisplayableProperties = "Id;Name";
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