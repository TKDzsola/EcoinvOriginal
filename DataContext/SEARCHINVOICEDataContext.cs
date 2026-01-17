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

        public SEARCHINVOICEDataContext()
        {
            _invoiceTable = new INVOICE_HEADERSTable();

            // FONTOS: Csak egyszer hozzuk létre a kollekciót!
            INVOICE_HEADERSList = new ObservableCollection<INVOICE_HEADERS>();

            StatusList = new ObservableCollection<StatusItem>();
            LoadStatusList();

            CommandSearch = new DelegateCommand(_ => DoSearch());
            CommandOpen = new DelegateCommand(_ => DoOpen(), _ => SelectedINVOICE_HEADERS != null);
            CommandPrint = new DelegateCommand(_ => DoPrint(), _ => SelectedINVOICE_HEADERS != null);
            CommandStorno = new DelegateCommand(_ => DoStorno(), _ => SelectedINVOICE_HEADERS != null && SelectedINVOICE_HEADERS.SZLASTAT != "2");
        }

        // =====================================================
        // LISTA (Getter only - nem cseréljük le a példányt, csak az elemeit!)
        // =====================================================
        public ObservableCollection<INVOICE_HEADERS> INVOICE_HEADERSList { get; }

        public ObservableCollection<StatusItem> StatusList { get; }

        private INVOICE_HEADERS _selectedInvoice;
        public INVOICE_HEADERS SelectedINVOICE_HEADERS
        {
            get => _selectedInvoice;
            set => SetPropertyValue(nameof(SelectedINVOICE_HEADERS), ref _selectedInvoice, value);
        }

        // =====================================================
        // SZŰRŐK
        // =====================================================
        private string _searchClientName;
        public string SearchClientName
        {
            get => _searchClientName;
            set => SetPropertyValue(nameof(SearchClientName), ref _searchClientName, value);
        }

        private string _searchInvoiceNumber;
        public string SearchInvoiceNumber
        {
            get => _searchInvoiceNumber;
            set => SetPropertyValue(nameof(SearchInvoiceNumber), ref _searchInvoiceNumber, value);
        }

        private DateTime? _fromDate;
        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetPropertyValue(nameof(FromDate), ref _fromDate, value);
        }

        private DateTime? _toDate;
        public DateTime? ToDate
        {
            get => _toDate;
            set => SetPropertyValue(nameof(ToDate), ref _toDate, value);
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetPropertyValue(nameof(SelectedStatus), ref _selectedStatus, value);
        }

        private void LoadStatusList()
        {
            StatusList.Clear();
            StatusList.Add(new StatusItem { Code = "", Text = "Mindegyik" });
            StatusList.Add(new StatusItem { Code = "0", Text = "Készítés alatt" });
            StatusList.Add(new StatusItem { Code = "1", Text = "Kész" });
            StatusList.Add(new StatusItem { Code = "2", Text = "Sztornózott" });
            SelectedStatus = "";
        }

        // =====================================================
        // COMMANDOK
        // =====================================================
        public ICommand CommandSearch { get; }
        public ICommand CommandOpen { get; }
        public ICommand CommandPrint { get; }
        public ICommand CommandStorno { get; }

        // =====================================================
        // KERESÉS (DIAGNOSZTIKAI VERZIÓ)
        // =====================================================
        private void DoSearch()
        {
            // SAJÁT KAPCSOLAT LÉTREHOZÁSA (Hogy biztosan nyitva legyen)
            FBConnectX localConn = new FBConnectX();

            try
            {
                // DEBUG: Látszódjon, hogy elindult a folyamat
                // Ha ez sem jelenik meg, akkor a Gomb nincs bekötve a XAML-ben!
                // MessageBox.Show("Keresés indítása...", "Debug"); 

                localConn.GetConnectionX();
                localConn.FBConnOpenX();

                // Dátum logika
                DateTime? from = FromDate.HasValue
                    ? new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0)
                    : (DateTime?)null;

                DateTime? to = ToDate.HasValue
                    ? new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59)
                    : (DateTime?)null;

                // Lekérdezés futtatása
                var result = _invoiceTable.SearchInvoices(
                    localConn, // A saját, biztosan nyitott kapcsolatot használjuk
                    SearchClientName?.Trim(),
                    SearchInvoiceNumber?.Trim(),
                    from,
                    to,
                    SelectedStatus
                );

                // DEBUG: Mennyi találat van?
                // MessageBox.Show($"Találatok száma: {result.Count}", "Debug Info");

                // Lista frissítése (Clear + Add a legbiztosabb WPF-ben)
                INVOICE_HEADERSList.Clear();

                foreach (var item in result)
                {
                    INVOICE_HEADERSList.Add(item);
                }

                if (INVOICE_HEADERSList.Count == 0)
                {
                    // Ha nincs találat, jelezzük
                    MessageBox.Show("Nincs találat a megadott feltételekre.", "Információ", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt a keresésnél:\n" + ex.Message + "\n" + ex.StackTrace,
                                "Kritikus Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Mindig zárjuk be a helyi kapcsolatot
                localConn.FBConnCloseX();
            }
        }

        // =====================================================
        // EGYÉB MŰVELETEK
        // =====================================================
        private void DoOpen() => DoPrint();

        private void DoPrint()
        {
            if (SelectedINVOICE_HEADERS == null)
            {
                MessageBox.Show("Válassz ki egy számlát a listából!", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            if (MessageBox.Show("Biztosan sztornózod?", "Sztornó", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            FBConnectX localConn = new FBConnectX();
            try
            {
                localConn.GetConnectionX();
                localConn.FBConnOpenX();

                _invoiceTable.SetStorno(SelectedINVOICE_HEADERS.ID, localConn);

                SelectedINVOICE_HEADERS.SZLASTAT = "2";

                // UI frissítés trükk
                var tmp = SelectedINVOICE_HEADERS;
                SelectedINVOICE_HEADERS = null;
                SelectedINVOICE_HEADERS = tmp;

                MessageBox.Show("Sikeres sztornózás!", "Kész", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { localConn.FBConnCloseX(); }
        }
    }
}