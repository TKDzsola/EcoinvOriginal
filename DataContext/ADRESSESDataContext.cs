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
            // Itt az összes címet betöltjük (vagy szűrhetnénk is, ha kellene)
            ADRESSESList = alkTable.GetList(FBConnX);
        }

        #region ... ADRESSESList ObservableCollection<ADRESSES> property ...

        private ObservableCollection<ADRESSES> __ADRESSESList = new ObservableCollection<ADRESSES>();

        public ObservableCollection<ADRESSES> ADRESSESList
        {
            get => __ADRESSESList;
            set => SetPropertyValue(nameof(ADRESSESList), ref __ADRESSESList, value);
        }

        #endregion

        #region ... SelectedADRESSES property ...

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
                // Biztonsági másolat készítése (Mégse gombhoz)
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

        #region ... OLDADRESSES property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ADRESSES __oldADRESSES;

        public ADRESSES OLDADRESSES
        {
            get => __oldADRESSES;
            set => SetPropertyValue(nameof(OLDADRESSES), ref __oldADRESSES, value);
        }

        #endregion

        #region ... CommandModifyCancel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;

        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute() => ((SelectedADRESSES != null) && (IsAdmin));

        private void ModifyCancelExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    // --- CANCEL (ÚJ REKORDNÁL) ---
                    // Nem kell adatbázis hívás (DelNewADRESSES_M), 
                    // csak kivesszük a listából a még el nem mentett elemet.

                    var _actrec = ADRESSESList.FirstOrDefault(r => r.ID <= 0);
                    if (_actrec != null)
                        ADRESSESList.Remove(_actrec);

                    IsNewRecord = false;
                }
                else
                {
                    // --- CANCEL (MÓDOSÍTÁSNÁL) ---
                    // Visszaállítjuk az eredeti értékeket a memóriában
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
            else
            {
                // MODIFY megnyomása - Nincs teendő, csak UI váltás
            }

            IsEditing = !IsEditing;
            ShowDetailPanel();
        }

        #endregion

        #region ... CommandNewSave property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandNewSave;

        public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());

        private bool NewSaveCanExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    // I N S E R T - Validáció
                    return (SelectedADRESSES != null) &&
                           !string.IsNullOrEmpty(SelectedADRESSES.CITY) &&
                           !string.IsNullOrEmpty(SelectedADRESSES.ADDRESS);
                    // POSTALCODE int, ezért nem null, hanem 0 lehet, de azt itt nem ellenőrizzük szigorúan
                }
                else
                {
                    // U P D A T E - Validáció
                    return (SelectedADRESSES != null);
                }
            }
            else
            {
                return IsAdmin;
            }
        }

        private void NewSaveExecute()
        {
            IsEditing = !IsEditing;
            if (IsEditing)
            {
                // --- NEW GOMB MEGNYOMÁSA ---
                IsNewRecord = true;

                // Nem hívunk adatbázist (NewADRESSES_M), csak létrehozunk egy üreset
                var newItem = new ADRESSES
                {
                    ID = -1, // Jelöljük, hogy új
                    ATYPE = "1",
                    AACTIVE = "1"
                };

                // Hozzáadjuk a listához és kijelöljük
                ADRESSESList.Add(newItem);
                SelectedADRESSES = newItem;
            }
            else
            {
                // --- SAVE GOMB MEGNYOMÁSA ---
                if (IsNewRecord)
                {
                    if (SelectedADRESSES != null)
                    {
                        // INSERT hívása a szabványos módon
                        alkTable.Insert(SelectedADRESSES, FBConnX);

                        // ID frissül az Insertben, de ha nem, itt újraolvashatnánk
                    }
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedADRESSES != null)
                    {
                        // UPDATE hívása a szabványos módon
                        alkTable.Update(SelectedADRESSES, FBConnX);
                    }
                }
            }

            ShowDetailPanel();
        }

        #endregion

        #region ... CommandDelete property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandDelete;

        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

        private bool DeleteCanExecute() => (IsAdmin) && (SelectedADRESSES != null) && (!IsEditing);

        private void DeleteExecute()
        {
            MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (MessageBoxResult.Yes == dres)
            {
                if (SelectedADRESSES != null)
                {
                    // DELETE hívása
                    alkTable.Delete(SelectedADRESSES, FBConnX);

                    // Kivesszük a listából
                    ADRESSESList.Remove(SelectedADRESSES);
                }
                ShowDetailPanel();
            }
        }

        #endregion
    }
}