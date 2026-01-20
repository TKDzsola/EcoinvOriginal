using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ecoinv.DataContext
{
    public class CLIENTSDataContext : DataContextBase
    {
        private readonly CLIENTSTable _clientsTable;
        private readonly ADRESSESTable _addressesTable;

        public CLIENTSDataContext()
        {
            _clientsTable = new CLIENTSTable();
            _addressesTable = new ADRESSESTable();

            ClientsList = new ObservableCollection<CLIENTS>();

            // 1. Új ügyfél: CSAK ADMIN
            CommandNew = new DelegateCommand(
                _ => DoNew(),
                _ => DataContextBase.IsAdmin
            );

            // 2. Módosítás: Van kijelölés ÉS Admin
            CommandModify = new DelegateCommand(
                _ => DoModify(),
                _ => SelectedClient != null && DataContextBase.IsAdmin
            );

            // 3. Mentés: Szerkesztés módban van ÉS Admin
            CommandSave = new DelegateCommand(
                _ => DoSave(),
                _ => IsEditing && DataContextBase.IsAdmin
            );

            // 4. Törlés: Van kijelölés ÉS Admin
            CommandDelete = new DelegateCommand(
                _ => DoDelete(),
                _ => SelectedClient != null && DataContextBase.IsAdmin
            );

            // 5. Kiválasztás (Számlához) és Keresés: BÁRKI
            CommandSelect = new DelegateCommand(_ => DoSelect(), _ => SelectedClient != null);
            CommandSearch = new DelegateCommand(_ => DoSearch());

            // ----------------------------------------

            // Alaphelyzet
            IsEditing = false;
            IsActiveOnly = false;
            SearchText = "";

            // BIZTONSÁGOS INDÍTÁS: Try-Catch blokkba tesszük, hogy ne omoljon össze az ablak nyitáskor
            try
            {
                DoSearch();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Hiba az ügyfelek betöltésekor (Konstruktor)");
                // Itt nem dobunk MessageBox-ot, mert az ablak még nem jött létre teljesen, 
                // de a logba beírjuk a hibát.
            }
        }

        // =====================================================
        // KERESÉSI VÁLTOZÓK
        // =====================================================

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetPropertyValue(nameof(SearchText), ref _searchText, value);
        }

        private bool _isActiveOnly;
        public bool IsActiveOnly
        {
            get => _isActiveOnly;
            set => SetPropertyValue(nameof(IsActiveOnly), ref _isActiveOnly, value);
        }

        // =====================================================
        // TULAJDONSÁGOK
        // =====================================================

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetPropertyValue(nameof(IsEditing), ref _isEditing, value);
        }

        public ObservableCollection<CLIENTS> ClientsList { get; private set; }

        private CLIENTS _selectedClient;
        public CLIENTS SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetPropertyValue(nameof(SelectedClient), ref _selectedClient, value))
                {
                    if (value != null)
                    {
                        IsEditing = false;
                        LoadClientDetails(value);
                    }
                }
            }
        }

        private CLIENTS _currentClient;
        public CLIENTS CurrentClient
        {
            get => _currentClient;
            set => SetPropertyValue(nameof(CurrentClient), ref _currentClient, value);
        }

        private ADRESSES _currentAddress;
        public ADRESSES CurrentAddress
        {
            get => _currentAddress;
            set => SetPropertyValue(nameof(CurrentAddress), ref _currentAddress, value);
        }

        // =====================================================
        // PARANCSOK
        // =====================================================
        public ICommand CommandNew { get; }
        public ICommand CommandModify { get; }
        public ICommand CommandSave { get; }
        public ICommand CommandDelete { get; }
        public ICommand CommandSelect { get; }
        public ICommand CommandSearch { get; }

        // =====================================================
        // METÓDUSOK - JAVÍTOTT (USING BLOKKOS) VERZIÓK
        // =====================================================

        private void DoSearch()
        {
            // JAVÍTÁS: Mindig új kapcsolatot hozunk létre a 'using' blokkal!
            using (FBConnectX conn = new FBConnectX())
            {
                conn.GetConnectionX();
                conn.FBConnOpenX(); // Kapcsolat nyitása

                if (conn.GetConStateX() != System.Data.ConnectionState.Open)
                    throw new Exception("Nem sikerült megnyitni az adatbázis kapcsolatot.");

                var fullList = _clientsTable.GetList(conn);

                // Biztonságosabb szűrés (Null Check)
                var filtered = fullList.Where(x =>
                    (string.IsNullOrEmpty(SearchText) || (x.NAME != null && x.NAME.ToLower().Contains(SearchText.ToLower())))
                    &&
                    (!IsActiveOnly || (x.CACTIVE != null && x.CACTIVE.Trim() == "1"))
                ).ToList();

                ClientsList.Clear();
                foreach (var item in filtered)
                {
                    ClientsList.Add(item);
                }

                // Ha nincs adat, de a kapcsolat jó volt, nem kell hibaüzenet, csak üres a lista.

                // Alaphelyzetbe állítás
                // Csak akkor nullázzuk le, ha nincs kiválasztott elem, vagy a lista frissült
                if (CurrentClient == null) CurrentClient = new CLIENTS();
                if (CurrentAddress == null) CurrentAddress = new ADRESSES();
                IsEditing = false;
            } // Itt automatikusan lezárul a kapcsolat (Dispose)
        }

        private void LoadClientDetails(CLIENTS client)
        {
            CurrentClient = client;

            // JAVÍTÁS: Itt is saját kapcsolatot használunk
            using (FBConnectX conn = new FBConnectX())
            {
                conn.GetConnectionX();
                conn.FBConnOpenX();

                var addresses = _addressesTable.GetList(conn, client.ID);
                var address = addresses.FirstOrDefault();

                if (address == null)
                {
                    address = new ADRESSES { CLIENT_ID = client.ID };
                }
                CurrentAddress = address;
            }
        }

        private void DoNew()
        {
            SelectedClient = null;
            CurrentClient = new CLIENTS { CACTIVE = "1" };
            CurrentAddress = new ADRESSES { AACTIVE = "1", ATYPE = "1" };
            IsEditing = true;
        }

        private void DoModify()
        {
            if (CurrentClient != null && CurrentClient.ID > 0)
            {
                IsEditing = true;
            }
        }

        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentClient.NAME))
            {
                MessageBox.Show("A név megadása kötelező!");
                return;
            }

            // JAVÍTÁS: Saját kapcsolat a mentéshez
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    // Null értékek kezelése
                    if (string.IsNullOrEmpty(CurrentClient.CACTIVE)) CurrentClient.CACTIVE = "0";

                    _clientsTable.Save(CurrentClient, conn);

                    CurrentAddress.CLIENT_ID = CurrentClient.ID;
                    _addressesTable.Save(CurrentAddress, conn);

                    MessageBox.Show("Sikeres mentés!", "Infó", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Frissítjük a listát (ez nyit majd egy újabb kapcsolatot, ami rendben van)
                    DoSearch();

                    var savedItem = ClientsList.FirstOrDefault(x => x.ID == CurrentClient.ID);
                    if (savedItem != null) SelectedClient = savedItem;

                    IsEditing = false;
                }
                catch (System.Exception ex)
                {
                    Logger.LogError(ex, "Hiba az ügyfél mentésekor");
                    MessageBox.Show("Hiba a mentéskor: " + ex.Message);
                }
            }
        }

        private void DoDelete()
        {
            if (SelectedClient == null) return;

            if (MessageBox.Show("Biztosan törölni szeretnéd ezt az ügyfelet?", "Törlés",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // JAVÍTÁS: Saját kapcsolat a törléshez
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();

                        var addresses = _addressesTable.GetList(conn, SelectedClient.ID);
                        foreach (var addr in addresses) _addressesTable.Delete(addr, conn);

                        _clientsTable.Delete(SelectedClient, conn);

                        DoSearch();
                    }
                    catch (System.Exception ex)
                    {
                        Logger.LogError(ex, "Hiba az ügyfél törlésekor");
                        MessageBox.Show("Törlési hiba: " + ex.Message);
                    }
                }
            }
        }

        private void DoSelect()
        {
            if (SelectedClient != null)
            {
                DataContextBase.SelectedClientForInvoice = SelectedClient.ID;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.Close(); break; }
                }
            }
            else
            {
                MessageBox.Show("Nincs kiválasztva ügyfél!");
            }
        }
    }
}