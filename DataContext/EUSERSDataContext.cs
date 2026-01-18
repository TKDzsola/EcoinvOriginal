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
    public class EUSERSDataContext : DataContextBase
    {
        public EUSERSDataContext()
        {
            alkTable = new EUSERSTable();
            EUSERSList = alkTable.GetList(FBConnX);
        }

        private readonly EUSERSTable alkTable;

        #region ... EUSERSList ...
        private ObservableCollection<EUSERS> __eusersList = new ObservableCollection<EUSERS>();
        public ObservableCollection<EUSERS> EUSERSList
        {
            get => __eusersList;
            set => SetPropertyValue(nameof(EUSERSList), ref __eusersList, value);
        }
        #endregion

        #region ... SelectedEUSERS ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EUSERS __selectedEUSERS;
        public EUSERS SelectedEUSERS
        {
            get => __selectedEUSERS;
            set
            {
                OnSelectedEUSERSChanging(value);
                SetPropertyValue(nameof(SelectedEUSERS), ref __selectedEUSERS, value);
                OnSelectedEUSERSChanged();
            }
        }
        private void OnSelectedEUSERSChanging(EUSERS value) { }
        private void OnSelectedEUSERSChanged()
        {
            OLDEUSERS ??= new EUSERS();
            if (SelectedEUSERS != null)
            {
                OLDEUSERS.ID = SelectedEUSERS.ID;
                OLDEUSERS.UNAME = SelectedEUSERS.UNAME;
                OLDEUSERS.UPSSW = SelectedEUSERS.UPSSW;
                OLDEUSERS.FULNAME = SelectedEUSERS.FULNAME;
                OLDEUSERS.JELSZO_NO_MD5 = SelectedEUSERS.JELSZO_NO_MD5;
                OLDEUSERS.UACTIVE = SelectedEUSERS.UACTIVE;
            }
        }
        #endregion

        #region ... OLDEUSERS ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EUSERS __oldeusers;
        public EUSERS OLDEUSERS
        {
            get => __oldeusers;
            set => SetPropertyValue(nameof(OLDEUSERS), ref __oldeusers, value);
        }
        #endregion

        // --- MÓDOSÍTÁS JOGOSULTSÁG ---
        #region ... CommandModifyCancel ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;

        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute()
        {
            if (IsEditing) return true;

            // FONTOS: Csak és kizárólag ADMIN módosíthat!
            // Kivettem azt, hogy (|| SelectedEUSERS.UNAME == LoginUserName), 
            // mert kérted, hogy sima user semmit se érjen el.
            return (SelectedEUSERS != null) && DataContextBase.IsAdmin;
        }

        private void ModifyCancelExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    alkTable.DelNewEUSERS_M();
                    var _actrec = EUSERSList.FirstOrDefault(r => r.ID == -1);
                    if (EUSERSList.IndexOf(_actrec) != -1)
                        EUSERSList.Remove(_actrec);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedEUSERS != null)
                        alkTable.ReUpdateEUSERS_M(OLDEUSERS);
                }
            }
            IsEditing = !IsEditing;
            ShowDetailPanel();
        }
        #endregion

        // --- ÚJ / MENTÉS JOGOSULTSÁG ---
        #region ... CommandNewSave ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandNewSave;

        public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());

        private bool NewSaveCanExecute()
        {
            if (IsEditing)
            {
                // Validáció + Admin jog
                bool isValid = false;
                if (IsNewRecord)
                {
                    isValid = !string.IsNullOrEmpty(SelectedEUSERS.UNAME) &&
                              !string.IsNullOrEmpty(SelectedEUSERS.UPSSW) &&
                              !string.IsNullOrEmpty(SelectedEUSERS.JELSZO_NO_MD5);
                }
                else
                {
                    isValid = (SelectedEUSERS != null) &&
                              !string.IsNullOrEmpty(SelectedEUSERS.UPSSW) &&
                              !string.IsNullOrEmpty(SelectedEUSERS.JELSZO_NO_MD5);
                }
                return isValid && DataContextBase.IsAdmin;
            }
            else
            {
                // Új gomb: Csak Admin
                return DataContextBase.IsAdmin;
            }
        }

        private void NewSaveExecute()
        {
            IsEditing = !IsEditing;
            if (IsEditing)
            {
                IsNewRecord = true;
                SelectedEUSERS = alkTable.NewEUSERS_M();
            }
            else
            {
                if (IsNewRecord)
                {
                    if (SelectedEUSERS != null)
                        alkTable.Insert(SelectedEUSERS, FBConnX);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedEUSERS != null)
                        alkTable.Update(SelectedEUSERS, FBConnX);
                }
            }
            ShowDetailPanel();
        }
        #endregion

        // --- TÖRLÉS JOGOSULTSÁG ---
        #region ... CommandDelete ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandDelete;

        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

        // CSAK ADMIN TÖRÖLHET
        private bool DeleteCanExecute() => DataContextBase.IsAdmin && (SelectedEUSERS != null) && (!IsEditing);

        private void DeleteExecute()
        {
            if (MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedEUSERS != null)
                    alkTable.Delete(SelectedEUSERS.ID, FBConnX);
                ShowDetailPanel();
            }
        }
        #endregion
    }
}