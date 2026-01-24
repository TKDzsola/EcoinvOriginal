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

            // Induláskor gombok beállítása
            UpdateLabels();
            try { RefreshData(); } catch { }
        }

        // --- DINAMIKUS GOMB FELIRATOK ---
        private string _contentBtnNewSave;
        public string ContentBtnNewSave
        {
            get => _contentBtnNewSave;
            set => SetPropertyValue(nameof(ContentBtnNewSave), ref _contentBtnNewSave, value);
        }

        private string _contentBtnModifyCancel;
        public string ContentBtnModifyCancel
        {
            get => _contentBtnModifyCancel;
            set => SetPropertyValue(nameof(ContentBtnModifyCancel), ref _contentBtnModifyCancel, value);
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
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    var vats = vatTable.GetList(conn);
                    VatRates = new ObservableCollection<VATRATES>(vats);
                    OnPropertyChanged(nameof(VatRates));

                    SERVICESList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list) SERVICESList.Add(item);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Hiba");
                }
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

        // --- ÁRKALKULÁCIÓ ---
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

        // --- OKOS KOMBINÁLT PARANCSOK ---

        public ICommand CommandNewSave => new DelegateCommand(_ => DoNewSave(), _ => IsAdmin);

        private void DoNewSave()
        {
            if (IsEditing) // MENTÉS
            {
                if (string.IsNullOrWhiteSpace(SelectedSERVICES.NAME)) { MessageBox.Show("Név kötelező!"); return; }
                if (SelectedVatRate == null) { MessageBox.Show("ÁFA kulcs kötelező!"); return; }

                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();
                        alkTable.Save(SelectedSERVICES, conn);

                        // JAVÍTÁS ITT: Elmentjük a számot, mielőtt a RefreshData törölné a kijelölést!
                        int savedId = SelectedSERVICES.ID;

                        MessageBox.Show("Sikeres mentés!");
                        RefreshData(); // Ez nullázza a SelectedSERVICES-t, de a savedId megvan!

                        IsEditing = false;
                        UpdateLabels();

                        // Most már a biztonságos számot (savedId) használjuk a kereséshez
                        var saved = SERVICESList.FirstOrDefault(x => x.ID == savedId);
                        if (saved != null) SelectedSERVICES = saved;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hiba: " + ex.Message);
                    }
                }
            }
            else // ÚJ FELVITELE
            {
                SelectedSERVICES = new SERVICES { SACTIVE = "I" };
                _selectedVatRate = null;
                OnPropertyChanged(nameof(SelectedVatRate));

                IsEditing = true;
                UpdateLabels();
            }
        }

        public ICommand CommandModifyCancel => new DelegateCommand(_ => DoModifyCancel(), _ => SelectedSERVICES != null && IsAdmin);

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

        public ICommand CommandDelete => new DelegateCommand(_ => DoDelete(), _ => SelectedSERVICES != null && IsAdmin && !IsEditing);

        private void DoDelete()
        {
            if (MessageBox.Show("Biztosan törölni szeretnéd?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();
                        alkTable.Delete(SelectedSERVICES, conn);
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
}