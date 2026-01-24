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
    public class EUSERSDataContext : DataContextBase
    {
        private readonly EUSERSTable alkTable;

        public EUSERSDataContext()
        {
            alkTable = new EUSERSTable();
            EUSERSList = new ObservableCollection<EUSERS>();

            // VÉDELEM AZ ÖSSZEOMLÁS ELLEN
            try
            {
                RefreshData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Users init hiba");
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

                    EUSERSList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list) EUSERSList.Add(item);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Felhasználók betöltése sikertelen");
                }
            }
        }

        public ObservableCollection<EUSERS> EUSERSList { get; private set; }

        private EUSERS _selectedEUSERS;
        public EUSERS SelectedEUSERS
        {
            get => _selectedEUSERS;
            set
            {
                if (SetPropertyValue(nameof(SelectedEUSERS), ref _selectedEUSERS, value))
                {
                    IsEditing = false;
                }
            }
        }

        // --- PARANCSOK ---

        public ICommand CommandNew => new DelegateCommand(_ => DoNew(), _ => IsAdmin);
        public ICommand CommandSave => new DelegateCommand(_ => DoSave(), _ => IsEditing && IsAdmin);
        public ICommand CommandDelete => new DelegateCommand(_ => DoDelete(), _ => SelectedEUSERS != null && IsAdmin && !IsEditing);
        public ICommand CommandModify => new DelegateCommand(_ => IsEditing = true, _ => SelectedEUSERS != null && IsAdmin);

        private void DoNew()
        {
            SelectedEUSERS = new EUSERS { UACTIVE = "I", ISADMIN = "N" };
            IsEditing = true;
        }

        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(SelectedEUSERS.UNAME))
            {
                MessageBox.Show("Felhasználónév kötelező!");
                return;
            }
            // Ha új felhasználó és nincs jelszó megadva
            if (SelectedEUSERS.ID <= 0 && string.IsNullOrEmpty(SelectedEUSERS.UPSSW))
            {
                MessageBox.Show("Új felhasználónál a jelszó kötelező!");
                return;
            }

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    alkTable.Save(SelectedEUSERS, conn);

                    MessageBox.Show("Sikeres mentés!");
                    IsEditing = false;
                    RefreshData();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "User mentési hiba");
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

                        alkTable.Delete(SelectedEUSERS, conn);
                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "User törlési hiba");
                        MessageBox.Show("Hiba: " + ex.Message);
                    }
                }
            }
        }
    }
}