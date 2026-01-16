using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ecoinv.DataContext
{
    public class CreateInvoiceDataContext : DataContextBase
    {
        // =====================================================
        // DB
        // =====================================================
        private readonly FBConnectX _conn;

        // =====================================================
        // CTOR
        // =====================================================
        public CreateInvoiceDataContext()
        {
            _conn = DatabaseHelper.CreateConnection();

            InvoiceDetails = new ObservableCollection<INVOICE_DETAILS>();
            Services = new ObservableCollection<SERVICES>();
            VatRates = new ObservableCollection<VATRATES>();

            LoadServices();
            LoadVatRates();

            CommandPrimaryAction = new DelegateCommand(_ => PrimaryAction());
            CommandSecondaryAction = new DelegateCommand(_ => SecondaryAction());
            CommandSaveInvoice = new DelegateCommand(_ => SaveInvoice(), _ => CanSaveInvoice());
            CommandCancelInvoice = new DelegateCommand(_ => CancelInvoice());
            CommandPreviewInvoice = new DelegateCommand(_ => PreviewInvoice());

            // Gombszövegek induló állapot
            UpdateActionButtonTexts();

            // induláskor felvesszük, ha esetleg már volt kiválasztva ügyfél
            LoadSelectedClientFromStatic();

            RecalculateTotals();
        }

        // =====================================================
        // ÜGYFÉL FELÜLET MEGNYITÁSA (KERESÉS)
        // =====================================================
        #region ... CommandMenuCLIENTSClick property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandMenuCLIENTSClick;

        public ICommand CommandMenuCLIENTSClick =>
            __commandMenuCLIENTSClick ??= new DelegateCommand(ac => MenuCLIENTSExecute(), fc => GetMenuCLIENTSCanExecute());

        private bool GetMenuCLIENTSCanExecute() => true;

        private void MenuCLIENTSExecute()
        {
            // Ha már nyitva van, ne nyissuk meg újra, inkább aktiváljuk
            for (var i = Application.Current.Windows.Count - 1; i >= 0; i--)
            {
                var w = Application.Current.Windows[i];
                if (w?.ToString() == "Ecoinv.Forms.CLIENTSFrm")
                {
                    w.Activate();
                    return;
                }
            }

            // MODAL megnyitás -> bezárás után vissza tudunk olvasni statikusból
            var frm = new CLIENTSFrm();
            frm.ShowDialog();

            // bezárás után: olvassuk vissza, mit választott a user
            LoadSelectedClientFromStatic();
        }

        #endregion ... end of CommandMenuCLIENTSClick property ...

        // =====================================================
        // KIVÁLASZTOTT ÜGYFÉL (SZÁMLÁZÁSHOZ)
        // =====================================================
        #region ... SelectedClient (Invoice) properties ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _selectedClientId;

        public int SelectedClientId
        {
            get => _selectedClientId;
            set => SetPropertyValue(nameof(SelectedClientId), ref _selectedClientId, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _selectedClientName;

        public string SelectedClientName
        {
            get => _selectedClientName;
            set => SetPropertyValue(nameof(SelectedClientName), ref _selectedClientName, value);
        }

        #endregion ... end of SelectedClient (Invoice) properties ...

        // =====================================================
        // KIVÁLASZTOTT ÜGYFÉL BETÖLTÉSE (STATIC -> LOCAL)
        // =====================================================
        #region ... LoadSelectedClientFromStatic() ...

        private void LoadSelectedClientFromStatic()
        {
            try
            {
                // FONTOS: a statikus property a DataContextBase-ben van!
                var id = DataContextBase.SelectedClientForInvoice;

                if (id <= 0)
                {
                    SelectedClientId = 0;
                    SelectedClientName = string.Empty;
                    return;
                }

                SelectedClientId = id;

                // név lekérése DB-ből
                var ct = new CLIENTSTable();
                var list = ct.GetList(_conn);

                var cli = list?.FirstOrDefault(x => x.ID == id);
                SelectedClientName = cli?.NAME ?? $"(ID: {id})";
            }
            catch
            {
                // Ne dobjunk UI crash-t
                SelectedClientId = 0;
                SelectedClientName = string.Empty;
            }
        }

        #endregion ... end of LoadSelectedClientFromStatic() ...

        // =====================================================
        // SZÁMLASZÁM
        // =====================================================
        private string _invoiceNumber;
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value);
        }

        // =====================================================
        // TÉTELEK
        // =====================================================
        public ObservableCollection<INVOICE_DETAILS> InvoiceDetails { get; }

        private INVOICE_DETAILS _selectedInvoiceDetail;
        public INVOICE_DETAILS SelectedInvoiceDetail
        {
            get => _selectedInvoiceDetail;
            set => SetPropertyValue(nameof(SelectedInvoiceDetail), ref _selectedInvoiceDetail, value);
        }

        // =====================================================
        // SZOLGÁLTATÁSOK
        // =====================================================
        public ObservableCollection<SERVICES> Services { get; }

        private SERVICES _selectedService;
        public SERVICES SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetPropertyValue(nameof(SelectedService), ref _selectedService, value))
                {
                    if (value != null)
                        EditNetUnitPrice = value.NETPRICE;
                }
            }
        }

        // =====================================================
        // ÁFA
        // =====================================================
        public ObservableCollection<VATRATES> VatRates { get; }

        private VATRATES _selectedVatRate;
        public VATRATES SelectedVatRate
        {
            get => _selectedVatRate;
            set => SetPropertyValue(nameof(SelectedVatRate), ref _selectedVatRate, value);
        }

        // =====================================================
        // SZERKESZTÉS
        // =====================================================
        private decimal _editQty = 1;
        public decimal EditQty
        {
            get => _editQty;
            set => SetPropertyValue(nameof(EditQty), ref _editQty, value);
        }

        private decimal _editNetUnitPrice;
        public decimal EditNetUnitPrice
        {
            get => _editNetUnitPrice;
            set => SetPropertyValue(nameof(EditNetUnitPrice), ref _editNetUnitPrice, value);
        }

        // =====================================================
        // ÖSSZESÍTÉS
        // =====================================================
        private decimal _totalNet;
        public decimal TotalNet
        {
            get => _totalNet;
            private set => SetPropertyValue(nameof(TotalNet), ref _totalNet, value);
        }

        private decimal _totalVat;
        public decimal TotalVat
        {
            get => _totalVat;
            private set => SetPropertyValue(nameof(TotalVat), ref _totalVat, value);
        }

        private decimal _totalGross;
        public decimal TotalGross
        {
            get => _totalGross;
            private set => SetPropertyValue(nameof(TotalGross), ref _totalGross, value);
        }

        private void RecalculateTotals()
        {
            TotalNet = InvoiceDetails.Sum(i => i.LINE_TOTAL_NET);
            TotalVat = InvoiceDetails.Sum(i => i.VAT_AMOUNT);
            TotalGross = InvoiceDetails.Sum(i => i.LINE_TOTAL_GROSS);
        }

        // =====================================================
        // UI ÁLLAPOT
        // =====================================================
        private bool _isAddItemPanelVisible;
        public bool IsAddItemPanelVisible
        {
            get => _isAddItemPanelVisible;
            set
            {
                if (SetPropertyValue(nameof(IsAddItemPanelVisible), ref _isAddItemPanelVisible, value))
                {
                    // Gombszövegek frissítése (biztosan, computed nélkül)
                    UpdateActionButtonTexts();
                }
            }
        }

        // A gombok szövegei (direkt property, hogy mindig frissüljön)
        private string _primaryButtonText;
        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            private set => SetPropertyValue(nameof(PrimaryButtonText), ref _primaryButtonText, value);
        }

        private string _secondaryButtonText;
        public string SecondaryButtonText
        {
            get => _secondaryButtonText;
            private set => SetPropertyValue(nameof(SecondaryButtonText), ref _secondaryButtonText, value);
        }

        private void UpdateActionButtonTexts()
        {
            PrimaryButtonText = IsAddItemPanelVisible ? "✔ Hozzáadás" : "Új tétel";
            SecondaryButtonText = IsAddItemPanelVisible ? "✖ Mégse" : "Tétel törlése";
        }

        // =====================================================
        // COMMANDOK
        // =====================================================
        public ICommand CommandPrimaryAction { get; }
        public ICommand CommandSecondaryAction { get; }
        public ICommand CommandSaveInvoice { get; }
        public ICommand CommandCancelInvoice { get; }
        public ICommand CommandPreviewInvoice { get; }

        // =====================================================
        // TÉTEL KEZELÉS
        // =====================================================
        private void PrimaryAction()
        {
            if (!IsAddItemPanelVisible)
            {
                IsAddItemPanelVisible = true;
                SelectedService = null;
                SelectedVatRate = null;
                EditQty = 1;
                EditNetUnitPrice = 0;
                return;
            }

            if (SelectedService == null || SelectedVatRate == null)
                return;

            var item = new INVOICE_DETAILS
            {
                SERVICES_ID = SelectedService.ID,
                SERVICE_NAME = SelectedService.NAME,
                QTY = EditQty,
                NET_UNIT_PRICE = EditNetUnitPrice,
                VAT_PERCENT = SelectedVatRate.RATES,
                VATRATE_ID = SelectedVatRate.ID
            };

            InvoiceDetails.Add(item);
            RecalculateTotals();
            IsAddItemPanelVisible = false;
        }

        private void SecondaryAction()
        {
            if (IsAddItemPanelVisible)
            {
                IsAddItemPanelVisible = false;
                return;
            }

            if (SelectedInvoiceDetail != null)
            {
                InvoiceDetails.Remove(SelectedInvoiceDetail);
                RecalculateTotals();
            }
        }

        // =====================================================
        // MENTÉS – FBConnectX kompatibilis
        // =====================================================
        private bool CanSaveInvoice() =>
            !string.IsNullOrWhiteSpace(InvoiceNumber)
            && InvoiceDetails.Any()
            && SelectedClientId > 0;

        private void SaveInvoice()
        {
            try
            {
                if (SelectedClientId <= 0)
                {
                    MessageBox.Show("Kérlek válassz ügyfelet a számlához!", "Hiányzó adat",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var headerTable = new INVOICE_HEADERSTable();

                var header = new INVOICE_HEADERS
                {
                    CLIENT_ID = SelectedClientId, // <<< FONTOS!
                    INVOICE_NUMBER = InvoiceNumber,
                    ISSUE_DATE = DateTime.Today,
                    DUE_DATE = DateTime.Today.AddDays(14),
                    CREATED = DateTime.Now,
                    PAYMENT_METHOD = "Készpénz",
                    SZLASTAT = "0",
                    FIZSTAT = "0",
                    STORNO_ID = "0"
                };

                headerTable.Insert(header, _conn);

                var detailTable = new INVOICE_DETAILSTable();

                foreach (var item in InvoiceDetails)
                {
                    item.INVOICEHEADERS_ID = header.ID;
                    detailTable.Insert(item, _conn);
                }

                MessageBox.Show("Számla sikeresen mentve.", "OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mentési hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================
        // EGYEBEK
        // =====================================================
        private void CancelInvoice()
        {
            InvoiceDetails.Clear();
            InvoiceNumber = string.Empty;
            RecalculateTotals();
        }

        private void PreviewInvoice()
        {
            MessageBox.Show("PDF generálás következő lépés.");
        }

        // =====================================================
        // BETÖLTÉSEK
        // =====================================================
        private void LoadServices()
        {
            var t = new SERVICESTable();
            foreach (var s in t.GetList(_conn))
                Services.Add(s);
        }

        private void LoadVatRates()
        {
            var t = new VATRATESTable();
            foreach (var v in t.GetList(_conn))
                VatRates.Add(v);
        }
    }
}
