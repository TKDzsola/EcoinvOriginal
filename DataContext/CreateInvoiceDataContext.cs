using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using Ecoinv.Pdf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ecoinv.DataContext
{
    public class CreateInvoiceDataContext : DataContextBase
    {
        public CreateInvoiceDataContext()
        {
            InvoiceDetails = new ObservableCollection<INVOICE_DETAILS>();
            Services = new ObservableCollection<SERVICES>();
            VatRates = new ObservableCollection<VATRATES>();

            // FIZETÉSI MÓDOK FELTÖLTÉSE
            PaymentMethods = new ObservableCollection<string>
            {
                "Készpénz",
                "Átutalás",
                "Bankkártya"
            };
            // Alapértelmezett kiválasztás
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault();

            // ALAPÉRTELMEZETT DÁTUMOK
            IssueDate = DateTime.Today; // Mai nap
            PaymentDeadline = DateTime.Today.AddDays(8); // +8 nap

            LoadServices();
            LoadVatRates();

            // 1. Primary Action (Hozzáadás) -> Csak ha IsAdmin
            CommandPrimaryAction = new DelegateCommand(
                _ => PrimaryAction(),
                _ => DataContextBase.IsAdmin
            );

            // 2. Secondary Action (Törlés/Mégse)
            CommandSecondaryAction = new DelegateCommand(
                _ => SecondaryAction(),
                _ => CanSecondaryAction()
            );

            // 3. Mentés -> Csak ha IsAdmin
            CommandSaveInvoice = new DelegateCommand(
                _ => SaveInvoice(),
                _ => CanSaveInvoice() && DataContextBase.IsAdmin
            );

            CommandCancelInvoice = new DelegateCommand(_ => CancelInvoice());
            CommandPreviewInvoice = new DelegateCommand(_ => PreviewInvoice());
            CommandMenuSERVICESClick = new DelegateCommand(_ => MenuSERVICESExecute());

            UpdateActionButtonTexts();
            LoadSelectedClientFromStatic();
            RecalculateTotals();
        }

        // =====================================================
        // ÚJ PROPERTY-K (DÁTUMOK ÉS FIZETÉSI MÓD)
        // =====================================================

        public ObservableCollection<string> PaymentMethods { get; }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetPropertyValue(nameof(SelectedPaymentMethod), ref _selectedPaymentMethod, value);
        }

        // --- ÚJ: KIÁLLÍTÁS DÁTUMA ---
        private DateTime _issueDate;
        public DateTime IssueDate
        {
            get => _issueDate;
            set => SetPropertyValue(nameof(IssueDate), ref _issueDate, value);
        }

        private DateTime _paymentDeadline;
        public DateTime PaymentDeadline
        {
            get => _paymentDeadline;
            set => SetPropertyValue(nameof(PaymentDeadline), ref _paymentDeadline, value);
        }

        // =====================================================
        // LISTÁK ÉS KIJELÖLÉSEK
        // =====================================================
        public ObservableCollection<INVOICE_DETAILS> InvoiceDetails { get; }
        public ObservableCollection<SERVICES> Services { get; }
        public ObservableCollection<VATRATES> VatRates { get; }

        private INVOICE_DETAILS _selectedInvoiceDetail;
        public INVOICE_DETAILS SelectedInvoiceDetail
        {
            get => _selectedInvoiceDetail;
            set
            {
                if (SetPropertyValue(nameof(SelectedInvoiceDetail), ref _selectedInvoiceDetail, value))
                {
                    (CommandSecondaryAction as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

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

        private VATRATES _selectedVatRate;
        public VATRATES SelectedVatRate
        {
            get => _selectedVatRate;
            set => SetPropertyValue(nameof(SelectedVatRate), ref _selectedVatRate, value);
        }

        // =====================================================
        // SZERKESZTŐ MEZŐK ÉS ÖSSZESÍTŐK
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
        // PANEL ÉS GOMBOK
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
                    (CommandSecondaryAction as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _primaryButtonText;
        public string PrimaryButtonText { get => _primaryButtonText; private set => SetPropertyValue(nameof(PrimaryButtonText), ref _primaryButtonText, value); }

        private string _secondaryButtonText;
        public string SecondaryButtonText { get => _secondaryButtonText; private set => SetPropertyValue(nameof(SecondaryButtonText), ref _secondaryButtonText, value); }

        private void UpdateActionButtonTexts()
        {
            PrimaryButtonText = IsAddItemPanelVisible ? "✔ Hozzáadás" : "Új tétel";
            SecondaryButtonText = IsAddItemPanelVisible ? "✖ Mégse" : "Tétel törlése";
        }

        // =====================================================
        // COMMANDS
        // =====================================================
        public ICommand CommandPrimaryAction { get; }
        public ICommand CommandSecondaryAction { get; }
        public ICommand CommandSaveInvoice { get; }
        public ICommand CommandCancelInvoice { get; }
        public ICommand CommandPreviewInvoice { get; }
        public ICommand CommandMenuSERVICESClick { get; }

        public ICommand CommandMenuCLIENTSClick { get => __commandMenuCLIENTSClick ??= new DelegateCommand(_ => MenuCLIENTSExecute()); }
        private ICommand __commandMenuCLIENTSClick;

        private void MenuCLIENTSExecute()
        {
            var frm = new CLIENTSFrm();
            frm.ShowDialog();
            LoadSelectedClientFromStatic();
        }

        private void MenuSERVICESExecute()
        {
            var frm = new SERVICESFrm();
            frm.ShowDialog();
            LoadServices();
        }

        private int _selectedClientId;
        public int SelectedClientId { get => _selectedClientId; set => SetPropertyValue(nameof(SelectedClientId), ref _selectedClientId, value); }

        private string _selectedClientName;
        public string SelectedClientName { get => _selectedClientName; set => SetPropertyValue(nameof(SelectedClientName), ref _selectedClientName, value); }

        private string _invoiceNumber;
        public string InvoiceNumber { get => _invoiceNumber; set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value); }

        // =====================================================
        // ADATBÁZIS MŰVELETEK (Using + Logger)
        // =====================================================
        private void LoadSelectedClientFromStatic()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    var id = DataContextBase.SelectedClientForInvoice;
                    if (id <= 0) { SelectedClientId = 0; SelectedClientName = string.Empty; return; }

                    SelectedClientId = id;
                    var ct = new CLIENTSTable();
                    var cli = ct.GetList(conn).FirstOrDefault(x => x.ID == id);
                    SelectedClientName = cli?.NAME ?? $"(ID: {id})";
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Kliens betöltési hiba a számlázónál");
                }
            }
        }

        private void PrimaryAction()
        {
            if (!IsAddItemPanelVisible) { IsAddItemPanelVisible = true; return; }
            if (SelectedService == null || SelectedVatRate == null) return;

            InvoiceDetails.Add(new INVOICE_DETAILS
            {
                SERVICES_ID = SelectedService.ID,
                SERVICE_NAME = SelectedService.NAME,
                QTY = EditQty,
                NET_UNIT_PRICE = EditNetUnitPrice,
                VAT_PERCENT = SelectedVatRate.RATES,
                VATRATE_ID = SelectedVatRate.ID
            });
            RecalculateTotals();
            IsAddItemPanelVisible = false;
        }

        private bool CanSecondaryAction() => IsAddItemPanelVisible || (SelectedInvoiceDetail != null && DataContextBase.IsAdmin);

        private void SecondaryAction()
        {
            if (IsAddItemPanelVisible) { IsAddItemPanelVisible = false; return; }
            if (SelectedInvoiceDetail != null) { InvoiceDetails.Remove(SelectedInvoiceDetail); RecalculateTotals(); }
        }

        private bool CanSaveInvoice() => !string.IsNullOrWhiteSpace(InvoiceNumber) && InvoiceDetails.Any() && SelectedClientId > 0;

        private void SaveInvoice()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    Logger.Log($"Számla mentése indítva. Sorszám: {InvoiceNumber}");

                    // 1. HEADER mentése (MOST MÁR A VALÓS ADATOKKAL)
                    var header = new INVOICE_HEADERS
                    {
                        CLIENT_ID = SelectedClientId,
                        INVOICE_NUMBER = InvoiceNumber,

                        // JAVÍTVA: A felhasználó által választott kiállítási dátum
                        ISSUE_DATE = IssueDate,

                        // JAVÍTVA: A felhasználó által választott határidő
                        DUE_DATE = PaymentDeadline,

                        CREATED = DateTime.Now,

                        // JAVÍTVA: A felhasználó által választott fizetési mód
                        PAYMENT_METHOD = SelectedPaymentMethod ?? "Készpénz",

                        SZLASTAT = "1", // Kiállítva
                        FIZSTAT = "0",  // Még nem fizetett
                        STORNO_ID = "0"
                    };
                    new INVOICE_HEADERSTable().Insert(header, conn);

                    // 2. TÉTELEK mentése
                    var detailTable = new INVOICE_DETAILSTable();
                    foreach (var item in InvoiceDetails)
                    {
                        item.INVOICEHEADERS_ID = header.ID;
                        detailTable.Insert(item, conn);
                    }

                    Logger.Log("Számla mentése sikeres.");
                    MessageBox.Show("Számla mentve.");

                    // 3. PDF GENERÁLÁS
                    var exportManager = new InvoiceExportManager();
                    exportManager.ExportInvoiceById(header.ID);

                    CancelInvoice();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Kritikus hiba a számla mentésekor");
                    MessageBox.Show($"Mentési hiba: {ex.Message}");
                }
            }
        }

        private void LoadServices()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try { Services.Clear(); foreach (var s in new SERVICESTable().GetList(conn)) Services.Add(s); }
                catch (Exception ex) { Logger.LogError(ex, "Szolgáltatások betöltése sikertelen"); }
            }
        }

        private void LoadVatRates()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try { VatRates.Clear(); foreach (var v in new VATRATESTable().GetList(conn)) VatRates.Add(v); }
                catch (Exception ex) { Logger.LogError(ex, "Áfa kulcsok betöltése sikertelen"); }
            }
        }

        private void CancelInvoice()
        {
            InvoiceDetails.Clear();
            InvoiceNumber = string.Empty;
            RecalculateTotals();
            IsAddItemPanelVisible = false;

            // Visszaállítás alaphelyzetbe
            IssueDate = DateTime.Today;
            PaymentDeadline = DateTime.Today.AddDays(8);
            SelectedPaymentMethod = PaymentMethods.FirstOrDefault();
        }

        private void PreviewInvoice() => MessageBox.Show("Előnézet funkció fejlesztés alatt.");
    }
}