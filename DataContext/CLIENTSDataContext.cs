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
        private readonly CLIENT_TYPESTable _typeTable; // Dinamikus típusokhoz

        public CLIENTSDataContext()
        {
            // Példányosítjuk az adatbázis-kezelőket
            _clientsTable = new CLIENTSTable();
            _addressesTable = new ADRESSESTable();
            _typeTable = new CLIENT_TYPESTable();

            ClientsList = new ObservableCollection<CLIENTS>();
            ClientTypes = new ObservableCollection<CLIENT_TYPES>(); // Adatbázis objektumok listája

            // PARANCSOK (Gombok működése)
            CommandNew = new DelegateCommand(_ => DoNew(), _ => DataContextBase.IsAdmin);
            CommandModify = new DelegateCommand(_ => DoModify(), _ => SelectedClient != null && DataContextBase.IsAdmin);
            CommandSave = new DelegateCommand(_ => DoSave(), _ => IsEditing && DataContextBase.IsAdmin);
            CommandDelete = new DelegateCommand(_ => DoDelete(), _ => SelectedClient != null && DataContextBase.IsAdmin);
            CommandSelect = new DelegateCommand(_ => DoSelect(), _ => SelectedClient != null);
            CommandSearch = new DelegateCommand(_ => DoSearch());

            // Alapértelmezett állapot
            IsEditing = false;
            IsActiveOnly = false;
            SearchText = "";

            // Kezdeti adatok betöltése (Típusok és Ügyfelek)
            LoadInitialData();
        }

        // --- ÚJ: Adatbázis-alapú betöltés ---
        private void LoadInitialData()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    // Kliens típusok betöltése a táblából
                    var types = _typeTable.GetList(conn);
                    ClientTypes.Clear();
                    foreach (var t in types)
                    {
                        ClientTypes.Add(t);
                    }

                    // Ügyféllista betöltése
                    DoSearch();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Kezdeti betöltési hiba");
                }
            }
        }

        // --- TULAJDONSÁGOK (Properties) ---

        public ObservableCollection<CLIENTS> ClientsList { get; private set; }
        public ObservableCollection<CLIENT_TYPES> ClientTypes { get; private set; } // Adatbázisból jövő típusok

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

        // --- ICOMMAND definíciók ---
        public ICommand CommandNew { get; }
        public ICommand CommandModify { get; }
        public ICommand CommandSave { get; }
        public ICommand CommandDelete { get; }
        public ICommand CommandSelect { get; }
        public ICommand CommandSearch { get; }

        // --- METÓDUSOK ---

        private void DoSearch()
        {
            // --- EASTER EGG (Titkos kód) ---
            if (!string.IsNullOrEmpty(SearchText) && SearchText.Trim().ToLower() == "creators")
            {
                MessageBox.Show("Ecoinv Rendszer\nVerzió: 2.0\nFejlesztette: [TE NEVED]", "Névjegy");
                SearchText = "";
                return;
            }

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    // Lekérjük az összes ügyfelet
                    var fullList = _clientsTable.GetList(conn);

                    // Szűrés a memóriában (Név + Megjegyzés alapú keresés)
                    var filtered = fullList.Where(x =>
                        (string.IsNullOrEmpty(SearchText) ||
                         (x.NAME != null && x.NAME.ToLower().Contains(SearchText.ToLower())) ||
                         (x.INTERNAL_NOTE != null && x.INTERNAL_NOTE.ToLower().Contains(SearchText.ToLower())))
                        &&
                        (!IsActiveOnly || (x.CACTIVE != null && x.CACTIVE.Trim() == "1"))
                    ).OrderBy(x => x.NAME).ToList();

                    // Lista frissítése
                    ClientsList.Clear();
                    foreach (var item in filtered)
                    {
                        ClientsList.Add(item);
                    }

                    // Reset
                    if (CurrentClient == null) CurrentClient = new CLIENTS();
                    if (CurrentAddress == null) CurrentAddress = new ADRESSES();
                    IsEditing = false;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Keresési hiba");
                }
            }
        }

        private void LoadClientDetails(CLIENTS client)
        {
            CurrentClient = client;
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    // Cím betöltése
                    var addresses = _addressesTable.GetList(conn, client.ID);
                    var address = addresses.FirstOrDefault();

                    CurrentAddress = address ?? new ADRESSES { CLIENT_ID = client.ID, AACTIVE = "1", ATYPE = "1" };
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Részletek betöltése hiba");
                }
            }
        }

        private void DoNew()
        {
            SelectedClient = null;

            CurrentClient = new CLIENTS
            {
                CACTIVE = "1"
            };

            // Alapértelmezett típus beállítása az első adatbázis-elemre
            if (ClientTypes.Count > 0)
            {
                CurrentClient.CLIENT_TYPE = ClientTypes[0].TYPE_NAME;
            }

            CurrentAddress = new ADRESSES
            {
                AACTIVE = "1",
                ATYPE = "1"
            };

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

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    if (string.IsNullOrEmpty(CurrentClient.CACTIVE)) CurrentClient.CACTIVE = "0";

                    // Mentés (Insert/Update kezelve a Save metóduson belül)
                    _clientsTable.Save(CurrentClient, conn);

                    CurrentAddress.CLIENT_ID = CurrentClient.ID;
                    if (CurrentAddress.ID <= 0)
                    {
                        _addressesTable.Insert(CurrentAddress, conn);
                    }
                    else
                    {
                        _addressesTable.Update(CurrentAddress, conn);
                    }

                    MessageBox.Show("Sikeres mentés!");
                    DoSearch();

                    var savedItem = ClientsList.FirstOrDefault(x => x.ID == CurrentClient.ID);
                    if (savedItem != null) SelectedClient = savedItem;

                    IsEditing = false;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Mentési hiba");
                    MessageBox.Show("Hiba a mentés során: " + ex.Message);
                }
            }
        }

        private void DoDelete()
        {
            if (SelectedClient == null) return;

            if (MessageBox.Show("Biztosan törölni szeretnéd az ügyfelet?", "Törlés megerősítése", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX();
                        conn.FBConnOpenX();

                        var addresses = _addressesTable.GetList(conn, SelectedClient.ID);
                        foreach (var addr in addresses)
                        {
                            _addressesTable.Delete(addr, conn);
                        }

                        _clientsTable.Delete(SelectedClient, conn);

                        DoSearch();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Törlési hiba");
                        MessageBox.Show("Nem sikerült törölni: " + ex.Message);
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
                    if (window.DataContext == this)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Nincs kiválasztva ügyfél!");
            }
        }
    }
}