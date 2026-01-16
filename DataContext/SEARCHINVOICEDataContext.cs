using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Components;
using Ecoinv.Common;

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
            CommandStorno = new DelegateCommand(_ => DoStorno(),
                _ => SelectedINVOICE_HEADERS != null && SelectedINVOICE_HEADERS.SZLASTAT != "2");
        }

        // =====================================================
        // LISTÁK
        // =====================================================
        public ObservableCollection<INVOICE_HEADERS> INVOICE_HEADERSList { get; set; }

        private INVOICE_HEADERS _selectedInvoice;
        public INVOICE_HEADERS SelectedINVOICE_HEADERS
        {
            get => _selectedInvoice;
            set => SetPropertyValue(nameof(SelectedINVOICE_HEADERS), ref _selectedInvoice, value);
        }

        // =====================================================
        // SZŰRŐK
        // =====================================================
        public string SearchClientName { get; set; }
        public string SearchInvoiceNumber { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string SelectedStatus { get; set; }

        public ObservableCollection<StatusItem> StatusList { get; }

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
        // KERESÉS (🔥 JAVÍTOTT)
        // =====================================================
        private void DoSearch()
        {
            try
            {
                // 🔒 Firebird-biztos dátumkezelés
                DateTime? from = FromDate.HasValue
                    ? new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0)
                    : (DateTime?)null;

                DateTime? to = ToDate.HasValue
                    ? new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59)
                    : (DateTime?)null;

                var result = _invoiceTable.SearchInvoices(
                    FBConnX,
                    SearchClientName?.Trim(),
                    SearchInvoiceNumber?.Trim(),
                    from,
                    to,
                    SelectedStatus
                );

                INVOICE_HEADERSList = new ObservableCollection<INVOICE_HEADERS>(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hiba történt a keresés során:\n" + ex.Message,
                    "Hiba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =====================================================
        // MŰVELETEK
        // =====================================================
        private void DoOpen()
        {
            MessageBox.Show("Megnyitás később.");
        }

        private void DoPrint()
        {
            MessageBox.Show("PDF / Nyomtatás később.");
        }

        private void DoStorno()
        {
            if (SelectedINVOICE_HEADERS == null)
                return;

            if (MessageBox.Show("Biztosan sztornózod?",
                "Sztornó", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                _invoiceTable.SetStorno(SelectedINVOICE_HEADERS.ID, FBConnX);
                SelectedINVOICE_HEADERS.SZLASTAT = "2";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hiba");
            }
        }
    }
}
