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

namespace XPOServer {
    public class XPOServerModeCRUDBehavior: CRUDBehaviorBase.CRUDBehaviorBase {
        protected override bool CanExecuteRemoveRowCommand() {
            if(CollectionSource == null || Grid == null || View == null || View.FocusedRow == null) return false;
            return true;
        }
        protected override void OnAttached() {
            base.OnAttached();
            if(View != null && CollectionSource != null)
                Initialize();
            else
                Grid.Loaded += OnGridLoaded;
        }
        protected override void Initialize() {
            Grid.ItemsSource = CollectionSource;
            NewRowCommand.RaiseCanExecuteChangedEvent();
            RemoveRowCommand.RaiseCanExecuteChangedEvent();
            EditRowCommand.RaiseCanExecuteChangedEvent();
            base.Initialize();
        }
        protected override void UpdateDataSource() {
            CollectionSource.Reload();
        }
    }
}