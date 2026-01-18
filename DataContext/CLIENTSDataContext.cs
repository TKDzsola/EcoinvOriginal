using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
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

            // 5. Kiválasztás (Számlához) és Keresés: BÁRKI (Ezek maradnak)
            CommandSelect = new DelegateCommand(_ => DoSelect(), _ => SelectedClient != null);
            CommandSearch = new DelegateCommand(_ => DoSearch());

            // ----------------------------------------

            // Alaphelyzet
            IsEditing = false;

            // Alapértelmezetten legyen HAMIS a szűrés
            IsActiveOnly = false;
            SearchText = "";

            // Induláskor betöltés
            DoSearch();
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
        // METÓDUSOK
        // =====================================================

        private void DoSearch()
        {
            if (FBConnX.GetConStateX() != System.Data.ConnectionState.Open)
                FBConnX.FBConnOpenX();

            var fullList = _clientsTable.GetList(FBConnX);

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

            if (ClientsList.Count == 0 && string.IsNullOrEmpty(SearchText) && !IsActiveOnly && fullList.Count > 0)
            {
                MessageBox.Show("Hiba: Az adatok beolvasása sikeres, de a megjelenítés nem sikerült.");
            }

            // Alaphelyzetbe állítás
            CurrentClient = new CLIENTS();
            CurrentAddress = new ADRESSES();
            IsEditing = false;
        }

        private void LoadClientDetails(CLIENTS client)
        {
            CurrentClient = client;

            var addresses = _addressesTable.GetList(FBConnX, client.ID);
            var address = addresses.FirstOrDefault();

            if (address == null)
            {
                address = new ADRESSES { CLIENT_ID = client.ID };
            }
            CurrentAddress = address;
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

            try
            {
                // Null értékek kezelése
                if (string.IsNullOrEmpty(CurrentClient.CACTIVE)) CurrentClient.CACTIVE = "0";

                _clientsTable.Save(CurrentClient, FBConnX);

                CurrentAddress.CLIENT_ID = CurrentClient.ID;
                _addressesTable.Save(CurrentAddress, FBConnX);

                MessageBox.Show("Sikeres mentés!", "Infó", MessageBoxButton.OK, MessageBoxImage.Information);

                DoSearch();

                var savedItem = ClientsList.FirstOrDefault(x => x.ID == CurrentClient.ID);
                if (savedItem != null) SelectedClient = savedItem;

                IsEditing = false;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message);
            }
        }

        private void DoDelete()
        {
            if (SelectedClient == null) return;

            if (MessageBox.Show("Biztosan törölni szeretnéd ezt az ügyfelet?", "Törlés",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var addresses = _addressesTable.GetList(FBConnX, SelectedClient.ID);
                    foreach (var addr in addresses) _addressesTable.Delete(addr, FBConnX);

                    _clientsTable.Delete(SelectedClient, FBConnX);

                    DoSearch();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Törlési hiba: " + ex.Message);
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