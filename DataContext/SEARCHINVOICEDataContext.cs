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

            INVOICE_HEADERSList = new ObservableCollection<INVOICE_HEADERS>();

            StatusList = new ObservableCollection<StatusItem>();
            LoadStatusList();

            CommandSearch = new DelegateCommand(_ => DoSearch());
            CommandOpen = new DelegateCommand(_ => DoOpen(), _ => SelectedINVOICE_HEADERS != null);
            CommandPrint = new DelegateCommand(_ => DoPrint(), _ => SelectedINVOICE_HEADERS != null);

            // Sztornó gomb feltételei
            CommandStorno = new DelegateCommand(
                _ => DoStorno(),
                _ => SelectedINVOICE_HEADERS != null &&
                     SelectedINVOICE_HEADERS.SZLASTAT != "2" &&
                     DataContextBase.IsAdmin // Csak Admin
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

        // --- SZŰRŐK ---
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

        public ICommand CommandSearch { get; }
        public ICommand CommandOpen { get; }
        public ICommand CommandPrint { get; }
        public ICommand CommandStorno { get; }

        // =====================================================
        // JAVÍTOTT KERESÉS (Hibatűrés és Dátum fix)
        // =====================================================
        private void DoSearch()
        {
            FBConnectX localConn = new FBConnectX();

            try
            {
                localConn.GetConnectionX();
                localConn.FBConnOpenX();

                // JAVÍTÁS: Dátum kezelés
                // Ha nincs megadva dátum, akkor null. 
                // Ha meg van adva, biztosítjuk a helyes formátumot.
                DateTime? from = FromDate.HasValue
                    ? FromDate.Value.Date // .Date levágja az időt (00:00:00)
                    : (DateTime?)null;

                DateTime? to = ToDate.HasValue
                    // JAVÍTÁS: A Firebird néha elhasal a 23:59:59.999-en. 
                    // Biztonságosabb, ha a következő nap éjféljét nézzük, és a lekérdezésben < (kisebb) jelet használunk,
                    // DE mivel a TableBaseClass-t nem látom, maradunk a nap végénél, de milliszekundum nélkül.
                    ? ToDate.Value.Date.AddDays(1).AddSeconds(-1) // 23:59:59
                    : (DateTime?)null;

                var result = _invoiceTable.SearchInvoices(
                    localConn,
                    SearchClientName?.Trim(),
                    SearchInvoiceNumber?.Trim(),
                    from,
                    to,
                    SelectedStatus
                );

                INVOICE_HEADERSList.Clear();

                foreach (var item in result)
                {
                    INVOICE_HEADERSList.Add(item);
                }

                if (INVOICE_HEADERSList.Count == 0)
                {
                    // JAVÍTÁS: Barátságos üzenet, ha nincs találat (üres lista)
                    MessageBox.Show("A megadott feltételekkel nem található számla.", "Keresés eredménye", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // JAVÍTÁS: Barátságos hibaüzenet összeomlás esetén
                // Ha a felhasználó ADMIN, akkor látja a technikai részletet is (zárójelben), hogy tudja jelezni a fejlesztőnek.
                // Ha NEM ADMIN, akkor csak egy szép üzenetet kap.

                string msg = "Nem sikerült végrehajtani a keresést.\n\n" +
                             "Lehetséges okok:\n" +
                             "- Nincs találat az adott időszakban.\n" +
                             "- Adatbázis kapcsolódási hiba.\n" +
                             "- Érvénytelen dátum formátum.";

                if (DataContextBase.IsAdmin)
                {
                    msg += $"\n\n(Technikai hiba: {ex.Message})";
                }

                MessageBox.Show(msg, "Keresési hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                localConn.FBConnCloseX();
            }
        }

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