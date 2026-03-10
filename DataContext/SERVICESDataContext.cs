using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;

namespace Ecoinv.DataContext
{
    public class SERVICESDataContext : DataContextBase
    {
        private readonly SERVICESTable alkTable;
        private readonly VATRATESTable vatTable;

        public SERVICESDataContext()
        {
            alkTable = new SERVICESTable();
            vatTable = new VATRATESTable();
            SERVICESList = new ObservableCollection<SERVICES>();

            UpdateLabels();
            try { RefreshData(); } catch { }
        }

        private void UpdateLabels()
        {
            if (IsEditing)
            {
                ContentBtnNewSave = "Mentés";
                ContentBtnModifyCancel = "Mégse";
            }
            else
            {
                ContentBtnNewSave = "Új";
                ContentBtnModifyCancel = "Módosít";
            }
        }

        private void RefreshData()
        {
            try
            {
                alkTable.InvalidateCache();
                vatTable.InvalidateCache();
                DatabaseHelper.Execute(conn =>
                {
                    var vats = vatTable.GetList(conn);
                    VatRates = new ObservableCollection<VATRATES>(vats);
                    OnPropertyChanged(nameof(VatRates));

                    SERVICESList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list) SERVICESList.Add(item);
                }, "Szolgáltatások betöltése sikertelen");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Szolgáltatások betöltése sikertelen");
            }
        }

        public ObservableCollection<SERVICES> SERVICESList { get; private set; }
        public ObservableCollection<VATRATES> VatRates { get; private set; }

        private SERVICES _selectedSERVICES;
        public SERVICES SelectedSERVICES
        {
            get => _selectedSERVICES;
            set
            {
                if (SetPropertyValue(nameof(SelectedSERVICES), ref _selectedSERVICES, value))
                {
                    if (!IsEditing)
                    {
                        if (value != null)
                        {
                            _selectedVatRate = VatRates?.FirstOrDefault(v => v.ID == value.VATRATE_ID);
                            OnPropertyChanged(nameof(SelectedVatRate));
                            OnPropertyChanged(nameof(EditNetPrice));
                            OnPropertyChanged(nameof(EditGrossPrice));
                        }
                    }
                }
            }
        }

        public decimal EditNetPrice
        {
            get => SelectedSERVICES?.NETPRICE ?? 0;
            set { if (SelectedSERVICES != null) { SelectedSERVICES.NETPRICE = (int)value; OnPropertyChanged(nameof(EditNetPrice)); CalculateGross(); } }
        }

        public decimal EditGrossPrice
        {
            get => SelectedSERVICES?.GROSSPRICE ?? 0;
            set { if (SelectedSERVICES != null) { SelectedSERVICES.GROSSPRICE = (int)value; OnPropertyChanged(nameof(EditGrossPrice)); } }
        }

        private VATRATES _selectedVatRate;
        public VATRATES SelectedVatRate
        {
            get => _selectedVatRate;
            set { if (SetPropertyValue(nameof(SelectedVatRate), ref _selectedVatRate, value)) { if (SelectedSERVICES != null && value != null) { SelectedSERVICES.VATRATE_ID = value.ID; CalculateGross(); } } }
        }

        private void CalculateGross()
        {
            if (SelectedSERVICES != null && SelectedVatRate != null)
            {
                decimal multiplier = 1 + (SelectedVatRate.RATES / 100m);
                EditGrossPrice = (int)Math.Round(EditNetPrice * multiplier);
            }
        }

        // --- PARANCSOK (cache-elve ??= operátorral) ---

        private ICommand _commandNewSave;
        public ICommand CommandNewSave => _commandNewSave ??= new DelegateCommand(_ => DoNewSave(), _ => IsAdmin);

        private void DoNewSave()
        {
            if (IsEditing) // MENTÉS
            {
                if (string.IsNullOrWhiteSpace(SelectedSERVICES.NAME)) { MessageBox.Show("Név kötelező!"); return; }
                if (SelectedVatRate == null) { MessageBox.Show("ÁFA kulcs kötelező!"); return; }

                try
                {
                    DatabaseHelper.Execute(conn =>
                    {
                        alkTable.Save(SelectedSERVICES, conn);
                    }, "Szolgáltatás mentési hiba");

                    int savedId = SelectedSERVICES.ID;

                    MessageBox.Show("Sikeres mentés!");
                    RefreshData();

                    IsEditing = false;
                    UpdateLabels();

                    var saved = SERVICESList.FirstOrDefault(x => x.ID == savedId);
                    if (saved != null) SelectedSERVICES = saved;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba: " + ex.Message);
                }
            }
            else // ÚJ
            {
                SelectedSERVICES = new SERVICES { SACTIVE = "I" };
                _selectedVatRate = null;
                OnPropertyChanged(nameof(SelectedVatRate));

                IsEditing = true;
                UpdateLabels();
            }
        }

        private ICommand _commandModifyCancel;
        public ICommand CommandModifyCancel => _commandModifyCancel ??= new DelegateCommand(_ => DoModifyCancel(), _ => SelectedSERVICES != null && IsAdmin);

        private void DoModifyCancel()
        {
            if (IsEditing) // MÉGSE
            {
                IsEditing = false;
                UpdateLabels();
                RefreshData();
            }
            else // MÓDOSÍT
            {
                IsEditing = true;
                UpdateLabels();
            }
        }

        private ICommand _commandDelete;
        public ICommand CommandDelete => _commandDelete ??= new DelegateCommand(_ => DoDelete(), _ => SelectedSERVICES != null && IsAdmin && !IsEditing);

        private void DoDelete()
        {
            if (MessageBox.Show("Biztosan törölni szeretnéd?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseHelper.Execute(conn =>
                    {
                        alkTable.Delete(SelectedSERVICES, conn);
                    }, "Szolgáltatás törlési hiba");

                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba: " + ex.Message);
                }
            }
        }
    }
}