using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interactivity;
using System.Windows;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using System.ComponentModel;
using DevExpress.Xpo;
using CRUDBehaviorBase;

namespace XPOInstant {
    public class XPOInstantModeCRUDBehavior: CRUDBehaviorBase.CRUDBehaviorBase {
        public static readonly DependencyProperty InstantCollectionSourceProperty =
            DependencyProperty.Register("InstantCollectionSource", typeof(XPInstantFeedbackSource), typeof(XPOInstantModeCRUDBehavior), new PropertyMetadata(null));
        public XPInstantFeedbackSource InstantCollectionSource {
            get { return (XPInstantFeedbackSource)GetValue(InstantCollectionSourceProperty); }
            set { SetValue(InstantCollectionSourceProperty, value); }
        }
        protected override bool CanExecuteRemoveRowCommand() {
            if(CollectionSource == null || Grid == null || View == null || Grid.CurrentItem == null) return false;
            return true;
        }
        protected override void Initialize() {
            Grid.ItemsSource = InstantCollectionSource;
            NewRowCommand.RaiseCanExecuteChangedEvent();
            CollectionSource = new XPServerCollectionSource(new UnitOfWork(), InstantCollectionSource.ObjectType) {
                AllowNew = true,
                AllowRemove = true,
                AllowEdit = true,
                TrackChanges = true,
                DeleteObjectOnRemove = true
            };
            base.Initialize();
        }
        protected override void UpdateDataSource() {
            InstantCollectionSource.Refresh();
        }
        protected override void OnAttached() {
            base.OnAttached();
            if(View != null && InstantCollectionSource != null)
                Initialize();
            else
                Grid.Loaded += OnGridLoaded;
        }
    }
}