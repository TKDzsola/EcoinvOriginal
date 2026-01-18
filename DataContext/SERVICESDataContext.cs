using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;

namespace Ecoinv.DataContext
{
    public class SERVICESDataContext : DataContextBase
    {
        public SERVICESDataContext()
        {
            alkTable = new SERVICESTable();
            SERVICESList = alkTable.GetList(FBConnX);
        }

        private readonly SERVICESTable alkTable;

        #region ... SERVICESList ObservableCollection<SERVICES> property ...

        private ObservableCollection<SERVICES> __SERVICESList = new ObservableCollection<SERVICES>();

        public ObservableCollection<SERVICES> SERVICESList
        {
            get => __SERVICESList;
            set => SetPropertyValue(nameof(SERVICESList), ref __SERVICESList, value);
        }

        #endregion

        #region ... SelectedSERVICES property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SERVICES __selectedSERVICES;

        public SERVICES SelectedSERVICES
        {
            get => __selectedSERVICES;
            set
            {
                OnSelectedSERVICESChanging(value);
                SetPropertyValue(nameof(SelectedSERVICES), ref __selectedSERVICES, value);
                OnSelectedSERVICESChanged();
            }
        }

        private void OnSelectedSERVICESChanging(SERVICES value) { }

        private void OnSelectedSERVICESChanged()
        {
            OLDSERVICES ??= new SERVICES();

            if (SelectedSERVICES != null)
            {
                OLDSERVICES.ID = SelectedSERVICES.ID;
                OLDSERVICES.NAME = SelectedSERVICES.NAME;
                OLDSERVICES.DESCRIPTION = SelectedSERVICES.DESCRIPTION;
                OLDSERVICES.NETPRICE = SelectedSERVICES.NETPRICE;
                OLDSERVICES.GROSSPRICE = SelectedSERVICES.GROSSPRICE;
                OLDSERVICES.VATRATE_ID = SelectedSERVICES.VATRATE_ID;
            }
        }

        #endregion

        #region ... OLDSERVICES property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SERVICES __oldSERVICES;

        public SERVICES OLDSERVICES
        {
            get => __oldSERVICES;
            set => SetPropertyValue(nameof(OLDSERVICES), ref __oldSERVICES, value);
        }

        #endregion

        // =================================================================
        // JOGOSULTSÁG KEZELÉS (Módosít / Mégse)
        // =================================================================
        #region ... CommandModifyCancel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;

        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute()
        {
            // Ha szerkesztünk (Cancel funkció), akkor engedjük (hogy ki lehessen lépni)
            if (IsEditing) return true;

            // Ha nem szerkesztünk (Modify funkció), akkor CSAK ADMIN
            return (SelectedSERVICES != null) && DataContextBase.IsAdmin;
        }

        private void ModifyCancelExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    alkTable.DelNewservices_M();
                    var _actrec = SERVICESList.FirstOrDefault(r => r.ID == -1);
                    if (SERVICESList.IndexOf(_actrec) != -1)
                        SERVICESList.Remove(_actrec);

                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedSERVICES != null)
                        alkTable.ReUpdateservices_M(OLDSERVICES);
                }
            }

            IsEditing = !IsEditing;
            ShowDetailPanel();
        }

        #endregion

        // =================================================================
        // JOGOSULTSÁG KEZELÉS (Új / Mentés)
        // =================================================================
        #region ... CommandNewSave property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandNewSave;

        public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());

        private bool NewSaveCanExecute()
        {
            if (IsEditing)
            {
                // MENTÉS MÓD: Validáció + Admin jog ellenőrzése (biztonságból)
                bool isValid = false;
                if (IsNewRecord)
                {
                    isValid = !string.IsNullOrEmpty(SelectedSERVICES.NAME) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.DESCRIPTION) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.NETPRICE.ToString()) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.GROSSPRICE.ToString()) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.VATRATE_ID.ToString());
                }
                else
                {
                    isValid = (SelectedSERVICES != null) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.NAME) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.DESCRIPTION) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.NETPRICE.ToString()) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.GROSSPRICE.ToString()) &&
                              !string.IsNullOrEmpty(SelectedSERVICES.VATRATE_ID.ToString());
                }

                return isValid && DataContextBase.IsAdmin;
            }
            else
            {
                // ÚJ REKORD MÓD: Csak Admin
                return DataContextBase.IsAdmin;
            }
        }

        private void NewSaveExecute()
        {
            IsEditing = !IsEditing;
            if (IsEditing)
            {
                IsNewRecord = true;
                SelectedSERVICES = alkTable.Newservices_M();
            }
            else
            {
                if (IsNewRecord)
                {
                    if (SelectedSERVICES != null)
                        alkTable.Insert(SelectedSERVICES, FBConnX);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedSERVICES != null)
                        alkTable.Update(SelectedSERVICES, FBConnX);
                }
            }
            ShowDetailPanel();
        }

        #endregion

        // =================================================================
        // JOGOSULTSÁG KEZELÉS (Törlés)
        // =================================================================
        #region ... CommandDelete property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandDelete;

        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

        // CSAK ADMIN TÖRÖLHET
        private bool DeleteCanExecute() => DataContextBase.IsAdmin && (SelectedSERVICES != null) && (!IsEditing);

        private void DeleteExecute()
        {
            MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo);
            if (MessageBoxResult.Yes == dres)
            {
                if (SelectedSERVICES != null)
                    alkTable.Delete(SelectedSERVICES.ID, FBConnX);

                ShowDetailPanel();
            }
        }

        #endregion
    }
}