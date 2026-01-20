using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Ecoinv.Common;
using Ecoinv.Components;

namespace Ecoinv.DataContext
{
    public class DataContextBase : SimpleModel
    {
        protected DataContextBase()
        {
            IsNewRecord = false;
            IsEditing = false;
            IsGridBrowsing = true;
            IsShowDetailPanel = false;
            IsRecordSelected = false;
            ContentBtnModifyCancel = "Módosít";
            ToolTipBtnModifyCancel = "Kiválasztott rekord módosítása/módosítás elvetése";
            ContentBtnNewSave = "Új";
            ToolTipBtnNewSave = "Új rekord felvitele/Új adatok mentése";
        }

        #region ... IsNewRecord property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isnewrecord;

        public bool IsNewRecord
        {
            get => __isnewrecord;
            set
            {
                OnIsNewRecordChanging(value);
                SetPropertyValue(nameof(IsNewRecord), ref __isnewrecord, value);
                OnIsNewRecordChanged();
            }
        }

        /*partial*/
        private void OnIsNewRecordChanging(bool value)
        {
        }

        /*partial*/
        private void OnIsNewRecordChanged()
        {
        }

        #endregion ... end of IsNewRecord property ...

        #region ... IsEditing property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isediting;

        public bool IsEditing
        {
            get => __isediting;
            set
            {
                SetPropertyValue(nameof(IsEditing), ref __isediting, value);
                IsGridBrowsing = !__isediting;
            }
        }

        #endregion ... end of IsEditing property ...

        #region ... LoginUserName property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static string __loginusername = "";

        public static string LoginUserName
        {
            get => __loginusername;
            set => __loginusername = value;
        }

        #endregion ... end of LoginUserName property ...

        #region ... IsAdmin property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static bool __isAdmin;

        public static bool IsAdmin
        {
            get => __isAdmin;
            set => __isAdmin = value;
        }

        #endregion ... end of IsAdmin property ...

        #region ... SelectedClientForInvoice (STATIC) ...

        // +++ ÚJ: esemény, ha később automatán frissíteni akarunk UI-t
        public static event System.Action<int> SelectedClientForInvoiceChanged;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static int __selectedClientForInvoice;

        public static int SelectedClientForInvoice
        {
            get => __selectedClientForInvoice;
            set
            {
                if (__selectedClientForInvoice == value)
                    return;

                __selectedClientForInvoice = value;
                SelectedClientForInvoiceChanged?.Invoke(__selectedClientForInvoice);
            }
        }

        #endregion ... end of SelectedClientForInvoice (STATIC) ...

        #region ... IsEnabledJustInsert property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isenabledjustinsert;

        public bool IsEnabledJustInsert
        {
            get => __isenabledjustinsert;
            set
            {
                OnIsEnabledJustInsertChanging(value);
                SetPropertyValue(nameof(IsEnabledJustInsert), ref __isenabledjustinsert, value);
                OnIsEnabledJustInsertChanged();
            }
        }

        /*partial*/
        private void OnIsEnabledJustInsertChanging(bool value)
        {
        }

        /*partial*/
        private void OnIsEnabledJustInsertChanged()
        {
        }

        #endregion ... end of IsEnabledJustInsert property ...

        #region ... IsGridBrowsing property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isgridbrowsing;

        public bool IsGridBrowsing
        {
            get => __isgridbrowsing;
            set
            {
                OnIsGridBrowsingChanging(value);
                SetPropertyValue(nameof(IsGridBrowsing), ref __isgridbrowsing, value);
                OnIsGridBrowsingChanged();
            }
        }

        /*partial*/
        private void OnIsGridBrowsingChanging(bool value)
        {
        }

        /*partial*/
        private void OnIsGridBrowsingChanged()
        {
        }

        #endregion ... end of IsGridBrowsing property ...

        #region ... IsRecordSelected property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isrecordselected;

        public bool IsRecordSelected
        {
            get => __isrecordselected;
            set
            {
                OnIsRecordSelectedChanging(value);
                SetPropertyValue(nameof(IsRecordSelected), ref __isrecordselected, value);
                OnIsRecordSelectedChanged();
            }
        }

        /*partial*/
        private void OnIsRecordSelectedChanging(bool value)
        {
        }

        /*partial*/
        private void OnIsRecordSelectedChanged()
        {
        }

        #endregion ... end of __isRecordSelected property ...

        #region ... IsShowDetailPanel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool __isshowdetailpanel;

        public bool IsShowDetailPanel
        {
            get => __isshowdetailpanel;
            set
            {
                OnIsShowDetailPanelChanging(value);
                SetPropertyValue(nameof(IsShowDetailPanel), ref __isshowdetailpanel, value);
                OnIsShowDetailPanelChanged();
            }
        }

        /*partial*/
        private void OnIsShowDetailPanelChanging(bool value)
        {
        }

        /*partial*/
        private void OnIsShowDetailPanelChanged()
        {
        }

        #endregion ... end of IsShowDetailPanel property ...

        #region ... ContentBtnModifyCancel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __contentbtnmodifycancel;

        public string ContentBtnModifyCancel
        {
            get => __contentbtnmodifycancel;
            set
            {
                OnContentBtnModifyCancelChanging(value);
                SetPropertyValue(nameof(ContentBtnModifyCancel), ref __contentbtnmodifycancel, value);
                OnContentBtnModifyCancelChanged();
            }
        }

        /*partial*/
        private void OnContentBtnModifyCancelChanging(string value)
        {
        }

        /*partial*/
        private void OnContentBtnModifyCancelChanged()
        {
        }

        #endregion ... end of ContentBtnModifyCancel property ...

        #region ... ToolTipBtnModifyCancel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __tooltipbtnmodifycancel;

        public string ToolTipBtnModifyCancel
        {
            get => __tooltipbtnmodifycancel;
            set
            {
                OnToolTipBtnModifyCancelChanging(value);
                SetPropertyValue(nameof(ToolTipBtnModifyCancel), ref __tooltipbtnmodifycancel, value);
                OnToolTipBtnModifyCancelChanged();
            }
        }

        /*partial*/
        private void OnToolTipBtnModifyCancelChanging(string value)
        {
        }

        /*partial*/
        private void OnToolTipBtnModifyCancelChanged()
        {
        }

        #endregion ... end of ToolTipBtnModifyCancel property ...

        #region ... ContentBtnNewSave property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __contentbtnnewsave;

        public string ContentBtnNewSave
        {
            get => __contentbtnnewsave;
            set
            {
                OnContentBtnNewSaveChanging(value);
                SetPropertyValue(nameof(ContentBtnNewSave), ref __contentbtnnewsave, value);
                OnContentBtnNewSaveChanged();
            }
        }

        /*partial*/
        private void OnContentBtnNewSaveChanging(string value)
        {
        }

        /*partial*/
        private void OnContentBtnNewSaveChanged()
        {
        }

        #endregion ... end of ContentBtnNewSave property ...

        #region ... ToolTipBtnNewSave property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __tooltipbtnnewsave;

        public string ToolTipBtnNewSave
        {
            get => __tooltipbtnnewsave;
            set
            {
                OnToolTipBtnNewSaveChanging(value);
                SetPropertyValue(nameof(ToolTipBtnNewSave), ref __tooltipbtnnewsave, value);
                OnToolTipBtnNewSaveChanged();
            }
        }

        /*partial*/
        private void OnToolTipBtnNewSaveChanging(string value)
        {
        }

        /*partial*/
        private void OnToolTipBtnNewSaveChanged()
        {
        }

        #endregion ... end of ToolTipBtnNewSave property ...

        public void ShowDetailPanel()
        {
            IsShowDetailPanel = !IsShowDetailPanel;
            ContentBtnModifyCancel = (IsEditing ? "Elvet" : "Módosít");
            ContentBtnNewSave = (IsEditing ? "Rögzít" : "Új");
        }

        #region ... FBConnX property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static FBConnectX __fbconnx;

        protected static FBConnectX FBConnX
        {
            get => __fbconnx;
            set => __fbconnx = value;
        }

        #endregion ... end of FBConn property ...

        #region ... CommandQuitBase property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandQuitBase;

        public ICommand CommandQuitBase => __commandQuitBase ??= new DelegateCommand(ac => QuitExecuteBase(parameter: ac?.ToString()), fc => GetQuitCanExecuteBase());

        private bool GetQuitCanExecuteBase() => !IsEditing;

        private void QuitExecuteBase(object parameter)
        {
            for (int intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
            {
                var aktform = Application.Current.Windows[intCounter];
                if (parameter != null)
                    if (aktform?.ToString() == parameter.ToString())
                        aktform?.Close();
            }
        }

        #endregion ... end of CommandQuitBase property ...
    }
}
