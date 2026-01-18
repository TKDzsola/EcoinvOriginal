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
    public class ADRESSESDataContext : DataContextBase
    {
        private readonly ADRESSESTable alkTable;

        public ADRESSESDataContext()
        {
            alkTable = new ADRESSESTable();
            ADRESSESList = alkTable.GetList(FBConnX);
        }

        #region ... ADRESSESList ...
        private ObservableCollection<ADRESSES> __ADRESSESList = new ObservableCollection<ADRESSES>();
        public ObservableCollection<ADRESSES> ADRESSESList
        {
            get => __ADRESSESList;
            set => SetPropertyValue(nameof(ADRESSESList), ref __ADRESSESList, value);
        }
        #endregion

        #region ... SelectedADRESSES ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ADRESSES __selectedADRESSES;
        public ADRESSES SelectedADRESSES
        {
            get => __selectedADRESSES;
            set
            {
                OnSelectedADRESSESChanging(value);
                SetPropertyValue(nameof(SelectedADRESSES), ref __selectedADRESSES, value);
                OnSelectedADRESSESChanged();
            }
        }
        private void OnSelectedADRESSESChanging(ADRESSES value) { }
        private void OnSelectedADRESSESChanged()
        {
            OLDADRESSES ??= new ADRESSES();
            if (SelectedADRESSES != null)
            {
                OLDADRESSES.ID = SelectedADRESSES.ID;
                OLDADRESSES.CLIENT_ID = SelectedADRESSES.CLIENT_ID;
                OLDADRESSES.POSTALCODE = SelectedADRESSES.POSTALCODE;
                OLDADRESSES.CITY = SelectedADRESSES.CITY;
                OLDADRESSES.ADDRESS = SelectedADRESSES.ADDRESS;
                OLDADRESSES.ATYPE = SelectedADRESSES.ATYPE;
                OLDADRESSES.AACTIVE = SelectedADRESSES.AACTIVE;
            }
        }
        #endregion

        #region ... OLDADRESSES ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ADRESSES __oldADRESSES;
        public ADRESSES OLDADRESSES
        {
            get => __oldADRESSES;
            set => SetPropertyValue(nameof(OLDADRESSES), ref __oldADRESSES, value);
        }
        #endregion

        // --- MÓDOSÍTÁS JOGOSULTSÁG ---
        #region ... CommandModifyCancel ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;

        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute()
        {
            if (IsEditing) return true; // Cancel mindig mehet
            return (SelectedADRESSES != null) && DataContextBase.IsAdmin; // Modify csak Admin
        }

        private void ModifyCancelExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    var _actrec = ADRESSESList.FirstOrDefault(r => r.ID <= 0);
                    if (_actrec != null) ADRESSESList.Remove(_actrec);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedADRESSES != null)
                    {
                        SelectedADRESSES.POSTALCODE = OLDADRESSES.POSTALCODE;
                        SelectedADRESSES.CITY = OLDADRESSES.CITY;
                        SelectedADRESSES.ADDRESS = OLDADRESSES.ADDRESS;
                        SelectedADRESSES.ATYPE = OLDADRESSES.ATYPE;
                        SelectedADRESSES.AACTIVE = OLDADRESSES.AACTIVE;
                    }
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
                    isValid = (SelectedADRESSES != null) && !string.IsNullOrEmpty(SelectedADRESSES.CITY) && !string.IsNullOrEmpty(SelectedADRESSES.ADDRESS);
                else
                    isValid = (SelectedADRESSES != null);

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
                var newItem = new ADRESSES { ID = -1, ATYPE = "1", AACTIVE = "1" };
                ADRESSESList.Add(newItem);
                SelectedADRESSES = newItem;
            }
            else
            {
                if (IsNewRecord)
                {
                    if (SelectedADRESSES != null) alkTable.Insert(SelectedADRESSES, FBConnX);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedADRESSES != null) alkTable.Update(SelectedADRESSES, FBConnX);
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

        private bool DeleteCanExecute() => DataContextBase.IsAdmin && (SelectedADRESSES != null) && (!IsEditing);

        private void DeleteExecute()
        {
            if (MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (SelectedADRESSES != null)
                {
                    alkTable.Delete(SelectedADRESSES, FBConnX);
                    ADRESSESList.Remove(SelectedADRESSES);
                }
                ShowDetailPanel();
            }
        }
        #endregion
    }
}