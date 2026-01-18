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
    public class SERVICESDataContext : DataContextBase
    {
        private readonly SERVICESTable alkTable;

        public SERVICESDataContext()
        {
            alkTable = new SERVICESTable();
            SERVICESList = alkTable.GetList(FBConnX);

            // ÁFA kulcsok betöltése
            var vatTable = new VATRATESTable();
            VatRates = new ObservableCollection<VATRATES>(vatTable.GetList(FBConnX));
        }

        public ObservableCollection<VATRATES> VatRates { get; }

        #region ... SERVICESList ...
        private ObservableCollection<SERVICES> __SERVICESList = new ObservableCollection<SERVICES>();
        public ObservableCollection<SERVICES> SERVICESList
        {
            get => __SERVICESList;
            set => SetPropertyValue(nameof(SERVICESList), ref __SERVICESList, value);
        }
        #endregion

        #region ... SelectedSERVICES ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SERVICES __selectedSERVICES;

        public SERVICES SelectedSERVICES
        {
            get => __selectedSERVICES;
            set
            {
                if (SetPropertyValue(nameof(SelectedSERVICES), ref __selectedSERVICES, value))
                {
                    OnSelectedSERVICESChanged();
                }
            }
        }

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

                // UI segédmezők frissítése
                _selectedVatRate = VatRates.FirstOrDefault(v => v.ID == SelectedSERVICES.VATRATE_ID);
                OnPropertyChanged(nameof(SelectedVatRate));
                OnPropertyChanged(nameof(EditNetPrice));
                OnPropertyChanged(nameof(EditGrossPrice));
            }
        }
        #endregion

        #region ... AUTO SZÁMOLÁS ...

        // Nettó ár (Int konverzióval javítva)
        public decimal EditNetPrice
        {
            get => SelectedSERVICES?.NETPRICE ?? 0;
            set
            {
                if (SelectedSERVICES != null && SelectedSERVICES.NETPRICE != (int)value)
                {
                    // JAVÍTÁS: (int) konverzió
                    SelectedSERVICES.NETPRICE = (int)value;
                    OnPropertyChanged(nameof(EditNetPrice));
                    CalculateGross();
                }
            }
        }

        // Bruttó ár (Int konverzióval javítva)
        public decimal EditGrossPrice
        {
            get => SelectedSERVICES?.GROSSPRICE ?? 0;
            set
            {
                if (SelectedSERVICES != null)
                {
                    // JAVÍTÁS: (int) konverzió
                    SelectedSERVICES.GROSSPRICE = (int)value;
                    OnPropertyChanged(nameof(EditGrossPrice));
                }
            }
        }

        private VATRATES _selectedVatRate;
        public VATRATES SelectedVatRate
        {
            get => _selectedVatRate;
            set
            {
                if (SetPropertyValue(nameof(SelectedVatRate), ref _selectedVatRate, value))
                {
                    if (SelectedSERVICES != null && value != null)
                    {
                        SelectedSERVICES.VATRATE_ID = value.ID;
                        CalculateGross();
                    }
                }
            }
        }

        private void CalculateGross()
        {
            if (SelectedSERVICES != null && SelectedVatRate != null)
            {
                decimal multiplier = 1 + (SelectedVatRate.RATES / 100m);
                // JAVÍTÁS: A számolt decimal értéket int-re kényszerítjük
                EditGrossPrice = (int)Math.Round(EditNetPrice * multiplier);
            }
        }

        #endregion

        #region ... OLDSERVICES ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SERVICES __oldSERVICES;
        public SERVICES OLDSERVICES
        {
            get => __oldSERVICES;
            set => SetPropertyValue(nameof(OLDSERVICES), ref __oldSERVICES, value);
        }
        #endregion

        // --- COMMANDOK ---
        #region ... CommandModifyCancel ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;
        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute()
        {
            if (IsEditing) return true;
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
                    if (SERVICESList.IndexOf(_actrec) != -1) SERVICESList.Remove(_actrec);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedSERVICES != null) alkTable.ReUpdateservices_M(OLDSERVICES);
                }
            }
            IsEditing = !IsEditing;
            ShowDetailPanel();
            OnSelectedSERVICESChanged();
        }
        #endregion

        #region ... CommandNewSave ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandNewSave;
        public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());

        private bool NewSaveCanExecute()
        {
            if (IsEditing)
            {
                bool isValid = (SelectedSERVICES != null) &&
                               !string.IsNullOrEmpty(SelectedSERVICES.NAME) &&
                               SelectedSERVICES.VATRATE_ID > 0;
                return isValid && DataContextBase.IsAdmin;
            }
            return DataContextBase.IsAdmin;
        }

        private void NewSaveExecute()
        {
            IsEditing = !IsEditing;
            if (IsEditing)
            {
                IsNewRecord = true;
                SelectedSERVICES = alkTable.Newservices_M();
                _selectedVatRate = null;
                OnPropertyChanged(nameof(SelectedVatRate));
                OnPropertyChanged(nameof(EditNetPrice));
                OnPropertyChanged(nameof(EditGrossPrice));
            }
            else
            {
                if (IsNewRecord)
                {
                    if (SelectedSERVICES != null) alkTable.Insert(SelectedSERVICES, FBConnX);
                    IsNewRecord = false;
                }
                else
                {
                    if (SelectedSERVICES != null) alkTable.Update(SelectedSERVICES, FBConnX);
                }
            }
            ShowDetailPanel();
        }
        #endregion

        #region ... CommandDelete ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandDelete;
        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

        private bool DeleteCanExecute() => (IsAdmin) && (SelectedSERVICES != null) && (!IsEditing);

        private void DeleteExecute()
        {
            if (MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedSERVICES != null) alkTable.Delete(SelectedSERVICES.ID, FBConnX);
                ShowDetailPanel();
            }
        }
        #endregion
    }
}