using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using DevExpress.Xpf.Grid;
using System.Windows.Input;
using System.Windows;
using DevExpress.Xpf.Core.ServerMode;
using System.Data.Linq;
using DevExpress.Xpf.Core;
using System.Windows.Controls;
using DevExpress.Xpf.Bars;
using DevExpress.Xpo;
using System.ComponentModel;

namespace CRUDBehaviorBase {
    public class CRUDBehaviorBase: Behavior<GridControl> {
        public static readonly DependencyProperty NewRowFormProperty =
            DependencyProperty.Register("NewRowForm", typeof(DataTemplate), typeof(CRUDBehaviorBase), new PropertyMetadata(null));
        public static readonly DependencyProperty EditRowFormProperty =
            DependencyProperty.Register("EditRowForm", typeof(DataTemplate), typeof(CRUDBehaviorBase), new PropertyMetadata(null));
        public static readonly DependencyProperty XPObjectTypeProperty =
            DependencyProperty.Register("XPObjectType", typeof(Type), typeof(CRUDBehaviorBase), new PropertyMetadata(null));
        public static readonly DependencyProperty AllowKeyDownActionsProperty =
            DependencyProperty.Register("AllowKeyDownActions", typeof(bool), typeof(CRUDBehaviorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty CollectionSourceProperty =
            DependencyProperty.Register("CollectionSource", typeof(XPServerCollectionSource), typeof(CRUDBehaviorBase), new PropertyMetadata(null));
        public static readonly DependencyProperty PrimaryKeyProperty =
            DependencyProperty.Register("PrimaryKey", typeof(string), typeof(CRUDBehaviorBase), new PropertyMetadata(string.Empty));

        public XPServerCollectionSource CollectionSource {
            get { return (XPServerCollectionSource)GetValue(CollectionSourceProperty); }
            set { SetValue(CollectionSourceProperty, value); }
        }
        public DataTemplate NewRowForm {
            get { return (DataTemplate)GetValue(NewRowFormProperty); }
            set { SetValue(NewRowFormProperty, value); }
        }
        public DataTemplate EditRowForm {
            get { return (DataTemplate)GetValue(EditRowFormProperty); }
            set { SetValue(EditRowFormProperty, value); }
        }
        public Type XPObjectType {
            get { return (Type)GetValue(XPObjectTypeProperty); }
            set { SetValue(XPObjectTypeProperty, value); }
        }
        public string PrimaryKey {
            get { return (string)GetValue(PrimaryKeyProperty); }
            set { SetValue(PrimaryKeyProperty, value); }
        }
        public bool AllowKeyDownActions {
            get { return (bool)GetValue(AllowKeyDownActionsProperty); }
            set { SetValue(AllowKeyDownActionsProperty, value); }
        }

        public GridControl Grid { get { return AssociatedObject; } }
        public TableView View { get { return Grid != null ? (TableView)Grid.View : null; } }

        #region Commands
        public CustomCommand NewRowCommand { get; private set; }
        public CustomCommand RemoveRowCommand { get; private set; }
        public CustomCommand EditRowCommand { get; private set; }
        protected virtual void ExecuteNewRowCommand() {
            AddNewRow();
        }
        protected virtual bool CanExecuteNewRowCommand() {
            return true;
        }
        protected virtual void ExecuteRemoveRowCommand() {
            RemoveSelectedRows();
        }
        protected virtual bool CanExecuteRemoveRowCommand() {
            return true;
        }
        protected virtual void ExecuteEditRowCommand() {
            EditRow();
        }
        protected virtual bool CanExecuteEditRowCommand() {
            return CanExecuteRemoveRowCommand();
        }
        #endregion

        public CRUDBehaviorBase() {
            NewRowCommand = new CustomCommand(ExecuteNewRowCommand, CanExecuteNewRowCommand);
            RemoveRowCommand = new CustomCommand(ExecuteRemoveRowCommand, CanExecuteRemoveRowCommand);
            EditRowCommand = new CustomCommand(ExecuteEditRowCommand, CanExecuteEditRowCommand);
        }
        public virtual object CreateNewRow() {
            return Activator.CreateInstance(XPObjectType, CollectionSource.Session);
        }
        public virtual void AddNewRow(object newRow) {
            AddNewRowIntoCollectionSource(CollectionSource, newRow);
        }
        public virtual void AddNewRowIntoCollectionSource(XPServerCollectionSource collectionSource, object newRow) {
            if(collectionSource == null) return;
            ((IBindingList)((IListSource)collectionSource).GetList()).Add(newRow);
            collectionSource.Session.CommitTransaction();
            UpdateDataSource();
        }
        public virtual void AddNewRow() {
            CollectionSource.Session.BeginTransaction();
            DXWindow dialog = CreateDialogWindow(CreateNewRow(), false);
            dialog.Closed += OnNewRowDialogClosed;
            dialog.ShowDialog();
        }
        public virtual void RemoveRow() {
            if(CollectionSource == null || PrimaryKey == string.Empty) return;
            for(int i = 0; i < ((IListSource)CollectionSource).GetList().Count; i++) {
                if(((XPObject)((IListSource)CollectionSource).GetList()[i]).GetMemberValue(PrimaryKey).ToString() == Grid.GetFocusedRowCellValue(PrimaryKey).ToString()) {
                    ((IBindingList)((IListSource)CollectionSource).GetList()).RemoveAt(i);
                    break;
                }
            }
        }
        public virtual void RemoveSelectedRows() {
            CollectionSource.Reload();
            CollectionSource.Session.BeginTransaction();
            int[] selectedRowsHandles = Grid.GetSelectedRowHandles();
            if(selectedRowsHandles != null || selectedRowsHandles.Length > 0) {
                List<object> rowKeys = new List<object>();
                foreach(int index in selectedRowsHandles)
                    rowKeys.Add(Grid.GetCellValue(index, PrimaryKey));
                for(int i = 0; i < ((IListSource)CollectionSource).GetList().Count; i++) {
                    if(rowKeys.Contains(((XPObject)((IListSource)CollectionSource).GetList()[i]).GetMemberValue(PrimaryKey))) {
                        ((IBindingList)((IListSource)CollectionSource).GetList()).RemoveAt(i);
                        i--;
                    }
                }
            }
            else if(Grid.CurrentItem != null)
                RemoveRow();
            CollectionSource.Session.CommitTransaction();
            CollectionSource.Session.PurgeDeletedObjects();
            UpdateDataSource();
        }
        public virtual void EditRow() {
            if(View == null || Grid.CurrentItem == null || PrimaryKey == string.Empty) return;
            for(int i = 0; i < ((IListSource)CollectionSource).GetList().Count; i++) {
                if(((XPObject)((IListSource)CollectionSource).GetList()[i]).GetMemberValue(PrimaryKey).ToString() == Grid.GetFocusedRowCellValue(PrimaryKey).ToString()) {
                    DXWindow dialog = CreateDialogWindow(((IListSource)CollectionSource).GetList()[i], true);
                    dialog.Closed += OnEditRowDialogClosed;
                    dialog.ShowDialog();
                    break;
                }
            }
        }
        protected virtual DXWindow CreateDialogWindow(object content, bool isEditingMode = false) {
            DXDialog dialog = new DXDialog {
                Tag = content,
                Buttons = DialogButtons.OkCancel,
                Title = isEditingMode ? "Edit Row" : "Add New Row",
                SizeToContent = SizeToContent.WidthAndHeight
            };
            ContentControl c = new ContentControl { Content = content };
            if(isEditingMode) {
                dialog.Title = "Edit Row";
                c.ContentTemplate = EditRowForm;
            }
            else {
                dialog.Title = "Add New Row";
                c.ContentTemplate = NewRowForm;
            }
            dialog.Content = c;
            return dialog;
        }
        protected virtual void OnNewRowDialogClosed(object sender, EventArgs e) {
            ((DXWindow)sender).Closed -= OnNewRowDialogClosed;
            if((bool)((DXWindow)sender).DialogResult)
                AddNewRow(((DXWindow)sender).Tag);
        }
        protected virtual void OnEditRowDialogClosed(object sender, EventArgs e) {
            ((DXWindow)sender).Closed -= OnEditRowDialogClosed;
            if((bool)((DXWindow)sender).DialogResult)
                CollectionSource.Session.CommitTransaction();
            else {
                CollectionSource.Session.RollbackTransaction();
                CollectionSource.Session.Reload(((DXWindow)sender).Tag, true);
            }
            UpdateDataSource();
        }
        protected virtual void OnViewKeyDown(object sender, KeyEventArgs e) {
            if(!AllowKeyDownActions)
                return;
            if(e.Key == Key.Delete) {
                RemoveSelectedRows();
                e.Handled = true;
            }
            if(e.Key == Key.Enter) {
                EditRow();
                e.Handled = true;
            }
        }
        protected virtual void OnViewRowDoubleClick(object sender, RowDoubleClickEventArgs e) {
            EditRow();
            e.Handled = true;
        }
        protected virtual void OnGridLoaded(object sender, RoutedEventArgs e) {
            Grid.Loaded -= OnGridLoaded;
            Initialize();
        }
        protected virtual void OnGridCurrentItemChanged(object sender, CurrentItemChangedEventArgs e) {
            UpdateCommands();
        }
        protected virtual void UpdateCommands() {
            RemoveRowCommand.RaiseCanExecuteChangedEvent();
            EditRowCommand.RaiseCanExecuteChangedEvent();
        }
        protected override void OnAttached() {
            base.OnAttached();
        }
        protected override void OnDetaching() {
            Uninitialize();
            base.OnDetaching();
        }
        protected virtual void Initialize() {
            View.KeyDown += OnViewKeyDown;
            View.RowDoubleClick += OnViewRowDoubleClick;
            Grid.CurrentItemChanged += OnGridCurrentItemChanged;
            UpdateCommands();
        }
        protected virtual void Uninitialize() {
            View.KeyDown -= OnViewKeyDown;
            View.RowDoubleClick -= OnViewRowDoubleClick;
            Grid.CurrentItemChanged -= OnGridCurrentItemChanged;
        }
        protected virtual void UpdateDataSource() {
        }
    }
    public class CustomCommand: ICommand {
        Action _executeMethod;
        Func<bool> _canExecuteMethod;
        public CustomCommand(Action executeMethod, Func<bool> canExecuteMethod) {
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }
        public bool CanExecute(object parameter) {
            return _canExecuteMethod();
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter) {
            _executeMethod();
        }
        public void RaiseCanExecuteChangedEvent() {
            if(CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}