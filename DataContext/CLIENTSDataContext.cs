using System;
using System.Collections.ObjectModel;
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
        private readonly CLIENTSTable _clientsTable;
        private readonly CLIENT_TYPESTable _typesTable;
        private readonly ADRESSESTable _addressTable;
        private readonly INVOICE_HEADERSTable _invoiceTable;

        public CLIENTSDataContext()
        {
            _clientsTable = new CLIENTSTable();
            _typesTable = new CLIENT_TYPESTable();
            _addressTable = new ADRESSESTable();
            _invoiceTable = new INVOICE_HEADERSTable();

            ClientsList = new ObservableCollection<CLIENTS>();
            ClientTypes = new ObservableCollection<CLIENT_TYPES>();

            LoadTypes();
            RefreshData();
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
                        filtered = filtered.Where(x => x.NAME.ToLower().Contains(SearchText.ToLower()));
                    foreach (var c in filtered) ClientsList.Add(c);
                }
                catch (Exception ex) { Logger.LogError(ex, "Ügyfél betöltési hiba"); }
            }
        }

        public ObservableCollection<CLIENTS> ClientsList { get; private set; }
        public ObservableCollection<CLIENT_TYPES> ClientTypes { get; private set; }

        private CLIENTS _selectedClient;
        public CLIENTS SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetPropertyValue(nameof(SelectedClient), ref _selectedClient, value))
                {
                    if (value != null) LoadDetails(value.ID);
                    else CustomerDebt = 0;
                    IsEditing = false;
                }
            }
        }

        private decimal _customerDebt;
        public decimal CustomerDebt
        {
            get => _customerDebt;
            set => SetPropertyValue(nameof(CustomerDebt), ref _customerDebt, value);
        }

        public CLIENTS CurrentClient { get; set; }
        public ADRESSES CurrentAddress { get; set; }
        public string SearchText { get; set; }

        private void LoadDetails(int clientId)
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    CurrentClient = _clientsTable.GetList(conn).FirstOrDefault(x => x.ID == clientId);
                    CurrentAddress = _addressTable.GetList(conn).FirstOrDefault(x => x.CLIENT_ID == clientId) ?? new ADRESSES { CLIENT_ID = clientId };

                    var invoices = _invoiceTable.SearchInvoices(conn, CurrentClient.NAME, null, null, null, null);
                    CustomerDebt = Math.Round(invoices.Sum(x => x.DEBT_AMOUNT), 2);
                }
                catch (Exception ex) { Logger.LogError(ex, "Részletek betöltése hiba"); }
            }
        }

        public ICommand CommandSearch => new DelegateCommand(_ => RefreshData());
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