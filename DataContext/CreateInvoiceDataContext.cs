using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using Ecoinv.Pdf.Services;
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
        private readonly FBConnectX _conn;

        public CreateInvoiceDataContext()
        {
            _conn = new FBConnectX();
            _conn.GetConnectionX();

            InvoiceDetails = new ObservableCollection<INVOICE_DETAILS>();
            Services = new ObservableCollection<SERVICES>();
            VatRates = new ObservableCollection<VATRATES>();

            LoadServices();
            LoadVatRates();

            CommandPrimaryAction = new DelegateCommand(_ => PrimaryAction());

            // --- JAVÍTÁS 1: Itt adjuk meg a feltételt (CanSecondaryAction) ---
            CommandSecondaryAction = new DelegateCommand(_ => SecondaryAction(), _ => CanSecondaryAction());
            // -----------------------------------------------------------------

            CommandSaveInvoice = new DelegateCommand(_ => SaveInvoice(), _ => CanSaveInvoice());
            CommandCancelInvoice = new DelegateCommand(_ => CancelInvoice());
            CommandPreviewInvoice = new DelegateCommand(_ => PreviewInvoice());

            CommandMenuSERVICESClick = new DelegateCommand(_ => MenuSERVICESExecute());

            UpdateActionButtonTexts();
            LoadSelectedClientFromStatic();
            RecalculateTotals();
        }

        // ... (A kód eleje változatlan) ...

        // =====================================================
        // TÉTELEK LISTA & KIJELÖLÉS KEZELÉSE
        // =====================================================
        public ObservableCollection<INVOICE_DETAILS> InvoiceDetails { get; }

        private INVOICE_DETAILS _selectedInvoiceDetail;
        public INVOICE_DETAILS SelectedInvoiceDetail
        {
            get => _selectedInvoiceDetail;
            set
            {
                if (SetPropertyValue(nameof(SelectedInvoiceDetail), ref _selectedInvoiceDetail, value))
                {
                    // --- JAVÍTÁS 2: Ha változik a kijelölés, szólunk a gombnak, hogy frissüljön! ---
                    (CommandSecondaryAction as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // ... (Törzsadatok, Services, VatRates részek változatlanok) ...

        // Ez a rész kell a másoláshoz, hogy ne szakadjon meg a kód folyamatossága:
        public ObservableCollection<SERVICES> Services { get; }
        private SERVICES _selectedService;
        public SERVICES SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetPropertyValue(nameof(SelectedService), ref _selectedService, value))
                {
                    if (value != null) EditNetUnitPrice = value.NETPRICE;
                }
            }
        }
        public ObservableCollection<VATRATES> VatRates { get; }
        private VATRATES _selectedVatRate;
        public VATRATES SelectedVatRate
        {
            get => _selectedVatRate;
            set => SetPropertyValue(nameof(SelectedVatRate), ref _selectedVatRate, value);
        }
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

        // ... (Összesítők és Totals változatlanok) ...

        private decimal _totalNet;
        public decimal TotalNet { get => _totalNet; private set => SetPropertyValue(nameof(TotalNet), ref _totalNet, value); }
        private decimal _totalVat;
        public decimal TotalVat { get => _totalVat; private set => SetPropertyValue(nameof(TotalVat), ref _totalVat, value); }
        private decimal _totalGross;
        public decimal TotalGross { get => _totalGross; private set => SetPropertyValue(nameof(TotalGross), ref _totalGross, value); }
        private void RecalculateTotals()
        {
            TotalNet = InvoiceDetails.Sum(i => i.LINE_TOTAL_NET);
            TotalVat = InvoiceDetails.Sum(i => i.VAT_AMOUNT);
            TotalGross = InvoiceDetails.Sum(i => i.LINE_TOTAL_GROSS);
        }

        // =====================================================
        // UI ÁLLAPOT FRISSÍTÉS
        // =====================================================
        private bool _isAddItemPanelVisible;
        public bool IsAddItemPanelVisible
        {
            get => _isAddItemPanelVisible;
            set
            {
                if (SetPropertyValue(nameof(IsAddItemPanelVisible), ref _isAddItemPanelVisible, value))
                {
                    UpdateActionButtonTexts();
                    // Ha a panel állapota változik (nyitva/zárva), a gomb állapota is változhat!
                    (CommandSecondaryAction as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // ... (Gombszövegek property-k változatlanok) ...
        private string _primaryButtonText;
        public string PrimaryButtonText { get => _primaryButtonText; private set => SetPropertyValue(nameof(PrimaryButtonText), ref _primaryButtonText, value); }
        private string _secondaryButtonText;
        public string SecondaryButtonText { get => _secondaryButtonText; private set => SetPropertyValue(nameof(SecondaryButtonText), ref _secondaryButtonText, value); }
        private void UpdateActionButtonTexts()
        {
            PrimaryButtonText = IsAddItemPanelVisible ? "✔ Hozzáadás" : "Új tétel";
            SecondaryButtonText = IsAddItemPanelVisible ? "✖ Mégse" : "Tétel törlése";
        }

        // COMMANDOK
        public ICommand CommandPrimaryAction { get; }
        public ICommand CommandSecondaryAction { get; }
        public ICommand CommandSaveInvoice { get; }
        public ICommand CommandCancelInvoice { get; }
        public ICommand CommandPreviewInvoice { get; }
        public ICommand CommandMenuCLIENTSClick { get => __commandMenuCLIENTSClick ??= new DelegateCommand(ac => MenuCLIENTSExecute(), fc => GetMenuCLIENTSCanExecute()); }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private ICommand __commandMenuCLIENTSClick;
        private bool GetMenuCLIENTSCanExecute() => true;

        // ... (Kliens kiválasztás és Szolgáltatás karbantartó gomb részek változatlanok) ...

        private void MenuCLIENTSExecute()
        {
            for (var i = Application.Current.Windows.Count - 1; i >= 0; i--)
            {
                var w = Application.Current.Windows[i];
                if (w?.ToString() == "Ecoinv.Forms.CLIENTSFrm") { w.Activate(); return; }
            }
            var frm = new CLIENTSFrm();
            frm.ShowDialog();
            LoadSelectedClientFromStatic();
        }
        public ICommand CommandMenuSERVICESClick { get; }
        private void MenuSERVICESExecute()
        {
            var frm = new SERVICESFrm();
            frm.ShowDialog();
            LoadServices();
        }

        // ... (SelectedClient részek) ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _selectedClientId;
        public int SelectedClientId { get => _selectedClientId; set => SetPropertyValue(nameof(SelectedClientId), ref _selectedClientId, value); }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _selectedClientName;
        public string SelectedClientName { get => _selectedClientName; set => SetPropertyValue(nameof(SelectedClientName), ref _selectedClientName, value); }
        private void LoadSelectedClientFromStatic()
        {
            try
            {
                var id = DataContextBase.SelectedClientForInvoice;
                if (id <= 0) { SelectedClientId = 0; SelectedClientName = string.Empty; return; }
                SelectedClientId = id;
                var ct = new CLIENTSTable();
                var list = ct.GetList(_conn);
                var cli = list?.FirstOrDefault(x => x.ID == id);
                SelectedClientName = cli?.NAME ?? $"(ID: {id})";
            }
            catch { SelectedClientId = 0; SelectedClientName = string.Empty; }
        }
        private string _invoiceNumber;
        public string InvoiceNumber { get => _invoiceNumber; set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value); }


        // =====================================================
        // --- JAVÍTÁS 3: A logika, ami eldönti, aktív-e a gomb ---
        // =====================================================
        private bool CanSecondaryAction()
        {
            // Ha nyitva van a panel, akkor a gomb funkciója "Mégse" -> MINDIG aktív legyen
            if (IsAddItemPanelVisible) return true;

            // Ha nincs nyitva, akkor a funkciója "Törlés" -> CSAK akkor aktív, ha van kijelölt sor
            return SelectedInvoiceDetail != null;
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

        private void PrimaryAction()
        {
            if (!IsAddItemPanelVisible)
            {
                IsAddItemPanelVisible = true; SelectedService = null; SelectedVatRate = null; EditQty = 1; EditNetUnitPrice = 0;
                return;
            }
            if (SelectedService == null || SelectedVatRate == null)
            {
                MessageBox.Show("Kérlek válassz szolgáltatást és áfa kulcsot!");
                return;
            }
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

        // ... (Mentés és egyéb metódusok változatlanok) ...

        private bool CanSaveInvoice() => !string.IsNullOrWhiteSpace(InvoiceNumber) && InvoiceDetails.Any() && SelectedClientId > 0;
        private void SaveInvoice()
        {
            try
            {
                if (SelectedClientId <= 0) { MessageBox.Show("Kérlek válassz ügyfelet!", "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                var headerTable = new INVOICE_HEADERSTable();
                var header = new INVOICE_HEADERS { CLIENT_ID = SelectedClientId, INVOICE_NUMBER = InvoiceNumber, ISSUE_DATE = DateTime.Today, DUE_DATE = DateTime.Today.AddDays(14), CREATED = DateTime.Now, PAYMENT_METHOD = "Készpénz", SZLASTAT = "0", FIZSTAT = "0", STORNO_ID = "0" };
                headerTable.Insert(header, _conn);
                var detailTable = new INVOICE_DETAILSTable();
                foreach (var item in InvoiceDetails) { item.INVOICEHEADERS_ID = header.ID; detailTable.Insert(item, _conn); }
                MessageBox.Show("Számla mentve.", "Kész", MessageBoxButton.OK, MessageBoxImage.Information);
                try { var exportManager = new InvoiceExportManager(); exportManager.ExportInvoiceById(header.ID); CancelInvoice(); }
                catch (Exception pdfEx) { MessageBox.Show($"PDF Hiba: {pdfEx.Message}", "PDF Hiba", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            catch (Exception ex) { MessageBox.Show($"Mentési hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CancelInvoice() { InvoiceDetails.Clear(); InvoiceNumber = string.Empty; RecalculateTotals(); IsAddItemPanelVisible = false; }
        private void PreviewInvoice() { MessageBox.Show("PDF előnézet hamarosan..."); }
        private void LoadServices() { Services.Clear(); var t = new SERVICESTable(); foreach (var s in t.GetList(_conn)) Services.Add(s); }
        private void LoadVatRates() { var t = new VATRATESTable(); foreach (var v in t.GetList(_conn)) VatRates.Add(v); }
    }
}