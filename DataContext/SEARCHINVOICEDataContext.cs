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

            // --- PARANCSOK ---
            CommandSearch = new DelegateCommand(_ => DoSearch());
            CommandOpen = new DelegateCommand(_ => DoOpen(), _ => SelectedINVOICE_HEADERS != null);
            CommandPrint = new DelegateCommand(_ => DoPrint(), _ => SelectedINVOICE_HEADERS != null);

            CommandStorno = new DelegateCommand(
                _ => DoStorno(),
                _ => SelectedINVOICE_HEADERS != null &&
                     SelectedINVOICE_HEADERS.SZLASTAT != "2" &&
                     DataContextBase.IsAdmin
            );

            // ÚJ: TÖRLÉS PARANCS (Csak Admin, ha van kijelölés)
            CommandDelete = new DelegateCommand(
                _ => DoDelete(),
                _ => SelectedINVOICE_HEADERS != null && DataContextBase.IsAdmin
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

        // ÚJ: Property
        public ICommand CommandDelete { get; }

        private void DoSearch()
        {
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
                    Logger.LogError(ex, "Számla keresési hiba");
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
                exportManager.ExportInvoiceById(SelectedINVOICE_HEADERS.ID);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Nyomtatási hiba. ID: {SelectedINVOICE_HEADERS.ID}");
                MessageBox.Show(ex.Message);
            }
        }

        private void DoStorno()
        {
            if (SelectedINVOICE_HEADERS == null) return;

            if (MessageBox.Show("Biztosan sztornózod a számlát?", "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            using (FBConnectX localConn = new FBConnectX())
            {
                try
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();

                    Logger.Log($"Sztornózás indítása. Eredeti ID: {SelectedINVOICE_HEADERS.ID}");

                    _invoiceTable.SetStorno(SelectedINVOICE_HEADERS.ID, localConn);
                    int newStornoInvoiceId = _invoiceTable.InsertStorno(SelectedINVOICE_HEADERS, localConn);
                    _detailsTable.CopyItems(SelectedINVOICE_HEADERS.ID, newStornoInvoiceId, localConn);

                    DoSearch();

                    MessageBox.Show("Sikeres sztornózás!");

                    var exportManager = new InvoiceExportManager();
                    exportManager.ExportInvoiceById(newStornoInvoiceId);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Sztornózási folyamat hiba. Eredeti ID: {SelectedINVOICE_HEADERS.ID}");
                    MessageBox.Show($"Hiba: {ex.Message}");
                }
            }
        }

        // --- ÚJ METÓDUS: TÖRLÉS VÉGREHAJTÁSA ---
        private void DoDelete()
        {
            if (SelectedINVOICE_HEADERS == null) return;

            string msg = $"FIGYELEM! Véglegesen törölni fogod a következő számlát:\n\n" +
                         $"Sorszám: {SelectedINVOICE_HEADERS.INVOICE_NUMBER}\n" +
                         $"Vevő: {SelectedINVOICE_HEADERS.CLIENT_NAME}\n\n" +
                         "Ez sorszám-hiányt okozhat az adatbázisban, ami adóügyi kockázat!\n" +
                         "Biztosan folytatod?";

            if (MessageBox.Show(msg, "VÉGLEGES TÖRLÉS", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
            {
                using (FBConnectX localConn = new FBConnectX())
                {
                    try
                    {
                        localConn.GetConnectionX();
                        localConn.FBConnOpenX();

                        // Törlés hívása (Adatbázis réteg)
                        _invoiceTable.Delete(SelectedINVOICE_HEADERS.ID, localConn);

                        Logger.LogInfo($"Számla véglegesen törölve ADMIN által. Sorszám: {SelectedINVOICE_HEADERS.INVOICE_NUMBER}");

                        MessageBox.Show("A számla sikeresen törölve.");

                        // Lista frissítése
                        DoSearch();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Számla törlési hiba");
                        MessageBox.Show($"Nem sikerült a törlés: {ex.Message}");
                    }
                }
            }
        }
    }
}