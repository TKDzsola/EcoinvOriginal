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
            StatusList = new ObservableCollection<StatusItem>();
            LoadStatusList();

            CommandSearch = new DelegateCommand(_ => DoSearch());
            CommandOpen = new DelegateCommand(_ => DoOpen(), _ => SelectedINVOICE_HEADERS != null);
            CommandPrint = new DelegateCommand(_ => DoPrint(), _ => SelectedINVOICE_HEADERS != null);

            CommandStorno = new DelegateCommand(
                _ => DoStorno(),
                _ => SelectedINVOICE_HEADERS != null &&
                     SelectedINVOICE_HEADERS.SZLASTAT != "2" &&
                     DataContextBase.IsAdmin
            );
        }

        public ObservableCollection<INVOICE_HEADERS> INVOICE_HEADERSList { get; }
        public ObservableCollection<StatusItem> StatusList { get; }

        private INVOICE_HEADERS _selectedInvoice;
        public INVOICE_HEADERS SelectedINVOICE_HEADERS
        {
            get => _selectedInvoice;
            set => SetPropertyValue(nameof(SelectedINVOICE_HEADERS), ref _selectedInvoice, value);
        }

        public string SearchClientName { get; set; }
        public string SearchInvoiceNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SelectedStatus { get; set; }

        private void LoadStatusList()
        {
            StatusList.Clear();
            StatusList.Add(new StatusItem { Code = "", Text = "Mindegyik" });
            StatusList.Add(new StatusItem { Code = "0", Text = "Készítés alatt" });
            StatusList.Add(new StatusItem { Code = "1", Text = "Kész" });
            StatusList.Add(new StatusItem { Code = "2", Text = "Sztornózott" });
            SelectedStatus = "";
        }

        public ICommand CommandSearch { get; }
        public ICommand CommandOpen { get; }
        public ICommand CommandPrint { get; }
        public ICommand CommandStorno { get; }

        private void DoSearch()
        {
            // ÚJ: AUTOMATIKUS LEZÁRÁS 'USING'-GAL
            using (FBConnectX localConn = new FBConnectX())
            {
                try
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();

                    DateTime? from = FromDate.HasValue ? FromDate.Value.Date : (DateTime?)null;
                    DateTime? to = ToDate.HasValue ? ToDate.Value.Date.AddDays(1).AddSeconds(-1) : (DateTime?)null;

                    var result = _invoiceTable.SearchInvoices(localConn, SearchClientName?.Trim(), SearchInvoiceNumber?.Trim(), from, to, SelectedStatus);

                    INVOICE_HEADERSList.Clear();
                    foreach (var item in result) INVOICE_HEADERSList.Add(item);

                    if (INVOICE_HEADERSList.Count == 0)
                        MessageBox.Show("Nincs találat.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DoOpen() => DoPrint();

        private void DoPrint()
        {
            if (SelectedINVOICE_HEADERS == null) return;
            try
            {
                var exportManager = new InvoiceExportManager();
                // Itt a manager belső 'using' blokkja intézi a kapcsolatot
                exportManager.ExportInvoiceById(SelectedINVOICE_HEADERS.ID);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void DoStorno()
        {
            if (SelectedINVOICE_HEADERS == null) return;

            if (MessageBox.Show("Biztosan sztornózod a számlát?\nEz véglegesen érvényteleníti és létrehoz egy korrekciós bizonylatot.",
                                "Sztornó megerősítése",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            // ÚJ: AUTOMATIKUS LEZÁRÁS 'USING'-GAL
            using (FBConnectX localConn = new FBConnectX())
            {
                try
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();

                    // 1. Eredeti státusz frissítése
                    _invoiceTable.SetStorno(SelectedINVOICE_HEADERS.ID, localConn);
                    SelectedINVOICE_HEADERS.SZLASTAT = "2";

                    // 2. Új sztornó számla létrehozása
                    int newStornoInvoiceId = _invoiceTable.InsertStorno(SelectedINVOICE_HEADERS, localConn);

                    // 3. Tételek másolása
                    _detailsTable.CopyItems(SelectedINVOICE_HEADERS.ID, newStornoInvoiceId, localConn);

                    // 4. Lista frissítése
                    DoSearch();

                    MessageBox.Show("A számla sztornózása sikeres!\nMost elkészítjük a sztornó bizonylatot.",
                                    "Kész", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 5. Automatikus PDF generálás
                    var exportManager = new InvoiceExportManager();
                    exportManager.ExportInvoiceById(newStornoInvoiceId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a sztornózás közben:\n{ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}