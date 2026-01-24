using System;
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
            ADRESSESList = new ObservableCollection<ADRESSES>();
            RefreshData();
        }

        private void RefreshData()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    ADRESSESList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list) ADRESSESList.Add(item);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Címek betöltése sikertelen");
                }
            }
        }

        #region ... ADRESSESList ...
        public ObservableCollection<ADRESSES> ADRESSESList { get; private set; }
        #endregion

        #region ... SelectedADRESSES ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ADRESSES __selectedADRESSES;
        public ADRESSES SelectedADRESSES
        {
            get => __selectedADRESSES;
            set
            {
                SetPropertyValue(nameof(SelectedADRESSES), ref __selectedADRESSES, value);
                OnSelectedADRESSESChanged();
            }
        }

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

        // --- MÓDOSÍTÁS ---
        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private ICommand __commandModifyCancel;

        private bool ModifyCancelCanExecute()
        {
            if (IsEditing) return true;
            return (SelectedADRESSES != null) && DataContextBase.IsAdmin;
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
                    // Visszaállítás
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

        // --- ÚJ / MENTÉS ---
        public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private ICommand __commandNewSave;

        private bool NewSaveCanExecute()
        {
            if (IsEditing)
            {
                bool isValid = false;
                if (IsNewRecord)
                    isValid = (SelectedADRESSES != null) && !string.IsNullOrEmpty(SelectedADRESSES.CITY) && !string.IsNullOrEmpty(SelectedADRESSES.ADDRESS);
                else
                    isValid = (SelectedADRESSES != null);

                return isValid && DataContextBase.IsAdmin;
            }
            return DataContextBase.IsAdmin;
        }

        private void NewSaveExecute()
        {
            IsEditing = !IsEditing;
            if (IsEditing) // Új
            {
                IsNewRecord = true;
                var newItem = new ADRESSES { ID = -1, ATYPE = "1", AACTIVE = "1" };
                ADRESSESList.Add(newItem);
                SelectedADRESSES = newItem;
            }
            else // Mentés
            {
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();

                        if (IsNewRecord)
                        {
                            if (SelectedADRESSES != null) alkTable.Insert(SelectedADRESSES, conn);
                        }
                        else
                        {
                            if (SelectedADRESSES != null) alkTable.Update(SelectedADRESSES, conn);
                        }
                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Cím mentési hiba");
                        MessageBox.Show("Mentési hiba: " + ex.Message);
                    }
                }
                IsNewRecord = false;
            }
            ShowDetailPanel();
        }

        // --- TÖRLÉS ---
        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private ICommand __commandDelete;

        private bool DeleteCanExecute() => DataContextBase.IsAdmin && (SelectedADRESSES != null) && (!IsEditing);

        private void DeleteExecute()
        {
            if (MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();
                        if (SelectedADRESSES != null)
                        {
                            alkTable.Delete(SelectedADRESSES, conn);
                            RefreshData();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Cím törlési hiba");
                        MessageBox.Show("Törlési hiba: " + ex.Message);
                    }
                }
                ShowDetailPanel();
            }
        }
    }
}