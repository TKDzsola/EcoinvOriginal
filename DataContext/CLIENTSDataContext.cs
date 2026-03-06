using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;

namespace Ecoinv.DataContext
{
    public class CLIENTSDataContext : DataContextBase
    {
        private readonly CLIENTSTable _clientsTable = new CLIENTSTable();
        private readonly CLIENT_TYPESTable _typesTable = new CLIENT_TYPESTable();
        private readonly ADRESSESTable _addressTable = new ADRESSESTable();
        private readonly INVOICE_HEADERSTable _invoiceTable = new INVOICE_HEADERSTable();

        public ObservableCollection<CLIENTS> ClientsList { get; } = new ObservableCollection<CLIENTS>();
        public ObservableCollection<CLIENT_TYPES> ClientTypes { get; } = new ObservableCollection<CLIENT_TYPES>();
        public System.Collections.Generic.List<string> AddressTypes { get; } = new System.Collections.Generic.List<string> { "Számlázási", "Levelezési", "Telephely" };

        public CLIENTSDataContext()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject())) { LoadTypes(); RefreshData(); }
            IsEditing = false;
        }

        private void LoadTypes()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    ClientTypes.Clear();
                    var list = _typesTable.GetList(conn);
                    foreach (var t in list) ClientTypes.Add(t);
                }
                catch (Exception ex) { Logger.LogError(ex, "Típusok betöltése hiba"); }
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
                    ClientsList.Clear();
                    var list = _clientsTable.GetList(conn);
                    var filtered = list.AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        string searchLower = SearchText.ToLower();
                        filtered = filtered.Where(x => (x.NAME != null && x.NAME.ToLower().Contains(searchLower)) ||
                                                       (x.ID.ToString().Contains(searchLower)));
                    }

                    foreach (var c in filtered) ClientsList.Add(c);
                }
                catch (Exception ex) { Logger.LogError(ex, "Ügyfél betöltési hiba"); }
            }
        }

        private CLIENTS _selectedClient;
        public CLIENTS SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetPropertyValue(nameof(SelectedClient), ref _selectedClient, value))
                {
                    if (value != null) LoadDetails(value.ID);
                    else { CurrentClient = null; CurrentAddress = null; CustomerDebt = 0; }
                    IsEditing = false;
                    DeleteButtonText = "Töröl";
                }
            }
        }

        private decimal _customerDebt;
        public decimal CustomerDebt
        {
            get => _customerDebt;
            set => SetPropertyValue(nameof(CustomerDebt), ref _customerDebt, value);
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

        private string _deleteButtonText = "Töröl";
        public string DeleteButtonText
        {
            get => _deleteButtonText;
            set => SetPropertyValue(nameof(DeleteButtonText), ref _deleteButtonText, value);
        }

        private void LoadDetails(int clientId)
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    CurrentClient = _clientsTable.GetList(conn).FirstOrDefault(x => x.ID == clientId);
                    CurrentAddress = _addressTable.GetList(conn, clientId).FirstOrDefault() ?? new ADRESSES { ID = -1, CLIENT_ID = clientId };

                    if (CurrentClient != null)
                    {
                        var invoices = _invoiceTable.SearchInvoices(conn, CurrentClient.NAME, null, null, null, null);
                        CustomerDebt = Math.Round(invoices.Sum(x => x.DEBT_AMOUNT), 2);
                    }
                }
                catch (Exception ex) { Logger.LogError(ex, "Részletek betöltése hiba"); }
            }
        }

        public ICommand CommandSearch => new DelegateCommand(_ => RefreshData());

        public ICommand CommandNew => new DelegateCommand(_ => {
            SelectedClient = null;

            // JAVÍTVA: Az új ügyfél kap egy alapértelmezett típust a listából Erika kérése szerint
            string defaultType = ClientTypes.FirstOrDefault()?.TYPE_NAME ?? "";

            CurrentClient = new CLIENTS
            {
                ID = -1,
                CACTIVE = "I",
                CLIENT_TYPE = defaultType
            };

            CurrentAddress = new ADRESSES { ID = -1, CLIENT_ID = -1, AACTIVE = "I", ATYPE = "Számlázási" };
            IsEditing = true;
            DeleteButtonText = "Mégse";
        }, _ => !IsEditing);

        public ICommand CommandModify => new DelegateCommand(_ => {
            IsEditing = true;
            DeleteButtonText = "Mégse";
        }, _ => SelectedClient != null && !IsEditing);

        public ICommand CommandSave => new DelegateCommand(_ => {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX(); conn.FBConnOpenX();

                    if (CurrentClient != null)
                    {
                        _clientsTable.Save(CurrentClient, conn);
                    }

                    if (CurrentAddress != null && CurrentClient != null)
                    {
                        CurrentAddress.CLIENT_ID = CurrentClient.ID;

                        string originalType = CurrentAddress.ATYPE;
                        if (originalType == "Számlázási") CurrentAddress.ATYPE = "1";
                        else if (originalType == "Levelezési") CurrentAddress.ATYPE = "2";
                        else if (originalType == "Telephely") CurrentAddress.ATYPE = "3";

                        _addressTable.Save(CurrentAddress, conn);

                        CurrentAddress.ATYPE = originalType;
                    }

                    IsEditing = false;
                    DeleteButtonText = "Töröl";
                    RefreshData();
                    MessageBox.Show("Sikeres mentés!", "Mentés", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show("Hiba: " + ex.Message); }
            }
        }, _ => IsEditing);

        public ICommand CommandDelete => new DelegateCommand(_ => {
            if (IsEditing)
            {
                IsEditing = false; DeleteButtonText = "Töröl";
                if (SelectedClient != null) LoadDetails(SelectedClient.ID);
            }
            else
            {
                if (MessageBox.Show("Biztosan törli?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (FBConnectX conn = new FBConnectX())
                    {
                        conn.GetConnectionX(); conn.FBConnOpenX();
                        if (SelectedClient != null) _clientsTable.Delete(SelectedClient, conn);
                        RefreshData();
                    }
                }
            }
        }, _ => SelectedClient != null || IsEditing);

        public ICommand CommandSelect => new DelegateCommand(p => DoSelect(p), _ => SelectedClient != null && !IsEditing);

        private void DoSelect(object param)
        {
            if (SelectedClient != null)
            {
                if (CustomerDebt > 0)
                {
                    string msg = $"FIGYELEM!\n\nEnnek az ügyfélnek {CustomerDebt:N2} € kintlévősége van!\n\nBiztosan folytatni szeretné a számlázást?";
                    if (MessageBox.Show(msg, "Kintlévőség figyelmeztetés", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;
                }
                DataContextBase.SelectedClientForInvoice = SelectedClient.ID;
                if (param is Window win) win.Close();
            }
        }

        public new ICommand CommandQuitBase => new DelegateCommand(p => { if (p is Window win) win.Close(); }, _ => true);
    }
}