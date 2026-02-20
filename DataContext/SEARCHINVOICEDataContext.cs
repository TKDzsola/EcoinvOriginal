using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Components;
using Ecoinv.Common;
using Ecoinv.Pdf.Services;

namespace Ecoinv.DataContext
{
    public class StatusItem
    {
        public string Code { get; set; }
        public string Text { get; set; }
    }

    public class SEARCHINVOICEDataContext : DataContextBase
    {
        private readonly INVOICE_HEADERSTable _invoiceTable;
        private readonly INVOICE_DETAILSTable _detailsTable;

        public SEARCHINVOICEDataContext()
        {
            _invoiceTable = new INVOICE_HEADERSTable();
            _detailsTable = new INVOICE_DETAILSTable();
            INVOICE_HEADERSList = new ObservableCollection<INVOICE_HEADERS>();

            INVOICE_HEADERSList.CollectionChanged += (s, e) => {
                if (e.NewItems != null) foreach (INVOICE_HEADERS item in e.NewItems) item.PropertyChanged += Item_PropertyChanged;
                UpdateTotals();
            };

            StatusList = new ObservableCollection<StatusItem>();
            LoadStatusList();

            CommandSearch = new DelegateCommand(_ => DoSearch());
            CommandPrint = new DelegateCommand(_ => DoPrint(), _ => SelectedINVOICE_HEADERS != null);
            CommandStorno = new DelegateCommand(_ => DoStorno(), _ => SelectedINVOICE_HEADERS != null && SelectedINVOICE_HEADERS.SZLASTAT != "2" && IsAdmin);
            CommandDelete = new DelegateCommand(_ => DoDelete(), _ => SelectedINVOICE_HEADERS != null && IsAdmin);

            // TECH LEAD JAVÍTÁS: A gomb legyen aktív, ha van kiválasztott számla és a hátralék nem nulla
            CommandSetPaid = new DelegateCommand(_ => DoSetPaid(), _ => SelectedINVOICE_HEADERS != null && SelectedINVOICE_HEADERS.DEBT_AMOUNT != 0);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(INVOICE_HEADERS.PAID_AMOUNT))
            {
                if (sender is INVOICE_HEADERS item)
                {
                    using (FBConnectX conn = new FBConnectX())
                    {
                        conn.GetConnectionX(); conn.FBConnOpenX();
                        _invoiceTable.UpdatePaidAmount(item.ID, item.PAID_AMOUNT, conn);
                        UpdateTotals();

                        // Kényszerített frissítés a gombok állapotára
                        RefreshCommandStates();
                    }
                }
            }
        }

        private void DoSetPaid()
        {
            if (SelectedINVOICE_HEADERS == null) return;
            // Kiegyenlítés: a befizetett összeg legyen egyenlő a bruttóval
            SelectedINVOICE_HEADERS.PAID_AMOUNT = SelectedINVOICE_HEADERS.TOTAL_GROSS;
        }

        private void RefreshCommandStates()
        {
            ((DelegateCommand)CommandStorno).RaiseCanExecuteChanged();
            ((DelegateCommand)CommandDelete).RaiseCanExecuteChanged();
            ((DelegateCommand)CommandPrint).RaiseCanExecuteChanged();
            ((DelegateCommand)CommandSetPaid).RaiseCanExecuteChanged();
        }

        private void UpdateTotals() => OnPropertyChanged(nameof(TotalDebt));

        public ObservableCollection<INVOICE_HEADERS> INVOICE_HEADERSList { get; }
        public ObservableCollection<StatusItem> StatusList { get; }
        public ICommand CommandSearch { get; }
        public ICommand CommandPrint { get; }
        public ICommand CommandStorno { get; }
        public ICommand CommandDelete { get; }
        public ICommand CommandSetPaid { get; }

        private INVOICE_HEADERS _selectedInvoice;
        public INVOICE_HEADERS SelectedINVOICE_HEADERS
        {
            get => _selectedInvoice;
            set
            {
                if (SetPropertyValue(nameof(SelectedINVOICE_HEADERS), ref _selectedInvoice, value))
                {
                    RefreshCommandStates();
                }
            }
        }

        public string SearchClientName { get; set; }
        public string SearchInvoiceNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SelectedStatus { get; set; }
        public bool OnlyUnpaid { get; set; }
        public decimal TotalDebt => INVOICE_HEADERSList.Sum(x => x.DEBT_AMOUNT);

        private void LoadStatusList()
        {
            StatusList.Add(new StatusItem { Code = "", Text = "Mindegyik" });
            StatusList.Add(new StatusItem { Code = "0", Text = "Készítés alatt" });
            StatusList.Add(new StatusItem { Code = "1", Text = "Kész" });
            StatusList.Add(new StatusItem { Code = "2", Text = "Sztornózott" });
        }

        private void DoSearch()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX(); conn.FBConnOpenX();
                    var res = _invoiceTable.SearchInvoices(conn, SearchClientName, SearchInvoiceNumber, FromDate, ToDate, SelectedStatus);
                    INVOICE_HEADERSList.Clear();
                    var filtered = OnlyUnpaid ? res.Where(x => x.DEBT_AMOUNT != 0) : res;
                    foreach (var i in filtered) INVOICE_HEADERSList.Add(i);
                    UpdateTotals();
                }
                catch (Exception ex) { Logger.LogError(ex, "Keresési hiba"); }
            }
        }

        private void DoPrint()
        {
            if (SelectedINVOICE_HEADERS == null) return;
            try
            {
                var exportManager = new InvoiceExportManager();
                exportManager.ExportInvoiceById(SelectedINVOICE_HEADERS.ID);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DoStorno()
        {
            if (SelectedINVOICE_HEADERS == null) return;
            if (MessageBox.Show("Biztosan sztornózod a számlát?", "Megerősítés", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX(); conn.FBConnOpenX();
                    _invoiceTable.SetStorno(SelectedINVOICE_HEADERS.ID, conn);
                    int newId = _invoiceTable.InsertStorno(SelectedINVOICE_HEADERS, conn);
                    _detailsTable.CopyItems(SelectedINVOICE_HEADERS.ID, newId, conn);
                    DoSearch();
                    MessageBox.Show("Sikeres sztornózás!");
                    new InvoiceExportManager().ExportInvoiceById(newId);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void DoDelete()
        {
            if (SelectedINVOICE_HEADERS == null) return;
            if (MessageBox.Show("Végleges törlés?", "FIGYELEM", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (FBConnectX conn = new FBConnectX())
                {
                    try
                    {
                        conn.GetConnectionX(); conn.FBConnOpenX();
                        _invoiceTable.Delete(SelectedINVOICE_HEADERS.ID, conn);
                        DoSearch();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }
    }
}