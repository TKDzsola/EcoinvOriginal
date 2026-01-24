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
    public class VATRATESDataContext : DataContextBase
    {
        private readonly VATRATESTable alkTable;

        public VATRATESDataContext()
        {
            alkTable = new VATRATESTable();
            VATRATESList = new ObservableCollection<VATRATES>();

            // VÉDELEM: Ha itt hiba van, nem omlik össze az ablak, csak üres lesz a lista.
            try
            {
                RefreshData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ÁFA init hiba");
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

                    VATRATESList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list.OrderBy(x => x.NAME))
                    {
                        VATRATESList.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ÁFA betöltése sikertelen");
                }
            }
        }

        public ObservableCollection<VATRATES> VATRATESList { get; private set; }

        private VATRATES _selectedVATRATES;
        public VATRATES SelectedVATRATES
        {
            get => _selectedVATRATES;
            set
            {
                if (SetPropertyValue(nameof(SelectedVATRATES), ref _selectedVATRATES, value))
                {
                    IsEditing = false;
                }
            }
        }

        // --- PARANCSOK ---

        public ICommand CommandNew => new DelegateCommand(_ => DoNew(), _ => IsAdmin);
        public ICommand CommandSave => new DelegateCommand(_ => DoSave(), _ => IsEditing && IsAdmin);
        public ICommand CommandDelete => new DelegateCommand(_ => DoDelete(), _ => SelectedVATRATES != null && IsAdmin && !IsEditing);
        public ICommand CommandModify => new DelegateCommand(_ => IsEditing = true, _ => SelectedVATRATES != null && IsAdmin);

        private void DoNew()
        {
            SelectedVATRATES = new VATRATES { VACTIVE = "1" };
            IsEditing = true;
        }

        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(SelectedVATRATES.NAME))
            {
                MessageBox.Show("A név megadása kötelező!");
                return;
            }

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    // Modern mentés
                    alkTable.Save(SelectedVATRATES, conn);

                    MessageBox.Show("Sikeres mentés!");
                    IsEditing = false;
                    RefreshData(); // Lista frissítése
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ÁFA mentési hiba");
                    MessageBox.Show("Hiba: " + ex.Message);
                }
            }
        }

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

                        alkTable.Delete(SelectedVATRATES, conn);
                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "ÁFA törlési hiba");
                        MessageBox.Show("Nem törölhető (használatban van?): " + ex.Message);
                    }
                }
            }
        }
    }
}