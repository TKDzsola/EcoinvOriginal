using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using Ecoinv.Pdf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ecoinv.DataContext
{
    public class CreateInvoiceDataContext : DataContextBase
    {
        // --- ADATBÁZIS KEZELŐK ---
        private readonly CLIENTSTable _clientsTable;
        private readonly INVOICE_HEADERSTable _headerTable;
        private readonly INVOICE_DETAILSTable _detailsTable;
        private readonly SERVICESTable _servicesTable;
        private readonly VATRATESTable _vatRatesTable;

        public CreateInvoiceDataContext()
        {
            // 1. Táblák inicializálása
            _clientsTable = new CLIENTSTable();
            _headerTable = new INVOICE_HEADERSTable();
            _detailsTable = new INVOICE_DETAILSTable();
            _servicesTable = new SERVICESTable();
            _vatRatesTable = new VATRATESTable();

            // 2. Listák példányosítása
            InvoiceDetails = new ObservableCollection<INVOICE_DETAILS>();
            Services = new ObservableCollection<SERVICES>();
            VatRates = new ObservableCollection<VATRATES>();
            PaymentMethods = new ObservableCollection<string> { "Átutalás", "Készpénz", "Bankkártya" };

            // 3. PARANCSOK LÉTREHOZÁSA
            CommandSaveInvoice = new DelegateCommand(_ => DoSaveInvoice(), _ => CanSaveInvoice());
            CommandCloseWindow = new DelegateCommand(_ => DoCloseWindow());

            CommandMenuCLIENTSClick = new DelegateCommand(_ => DoOpenClientsWindow());
            CommandMenuSERVICESClick = new DelegateCommand(_ => DoOpenServicesWindow());

            CommandAddItem = new DelegateCommand(_ => ShowAddItemPanel());
            CommandRemoveItem = new DelegateCommand(_ => DoRemoveItem(), _ => SelectedInvoiceDetail != null);

            CommandPrimaryAction = new DelegateCommand(_ => DoSaveItemFromPanel());
            CommandSecondaryAction = new DelegateCommand(_ => HideAddItemPanel());

            // 4. Alapértelmezések beállítása
            IssueDate = DateTime.Today;
            PaymentDeadline = DateTime.Today.AddDays(8);
            SelectedPaymentMethod = "Átutalás";
            InvoiceNumber = "";
            IsAddItemPanelVisible = false;

            // 5. Adatok betöltése
            LoadServices();
            LoadVatRates();
        }

        // -------------------------------------------------------------------------
        // VALIDÁCIÓ
        // -------------------------------------------------------------------------
        private bool CanSaveInvoice()
        {
            bool hasNumber = !string.IsNullOrWhiteSpace(InvoiceNumber);
            bool hasClient = SelectedClient != null;
            bool hasItems = InvoiceDetails != null && InvoiceDetails.Count > 0;

            return hasNumber && hasClient && hasItems;
        }

        private void RefreshSaveButtonState()
        {
            if (CommandSaveInvoice != null)
            {
                ((DelegateCommand)CommandSaveInvoice).RaiseCanExecuteChanged();
            }
        }

        // -------------------------------------------------------------------------
        // PROPERTIES
        // -------------------------------------------------------------------------

        // --- VEVŐ ---
        private CLIENTS _selectedClient;
        public CLIENTS SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetPropertyValue(nameof(SelectedClient), ref _selectedClient, value))
                {
                    OnPropertyChanged(nameof(SelectedClientName));
                    RefreshSaveButtonState();
                }
            }
        }
        public string SelectedClientName => SelectedClient?.NAME ?? "";

        // --- SZÁMLA ADATOK ---
        private string _invoiceNumber;
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set
            {
                if (SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value))
                {
                    RefreshSaveButtonState();
                }
            }
        }

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

        public ObservableCollection<string> PaymentMethods { get; }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set => SetPropertyValue(nameof(SelectedPaymentMethod), ref _selectedPaymentMethod, value);
        }

        // --- TÉTELEK ---
        public ObservableCollection<INVOICE_DETAILS> InvoiceDetails { get; }

        private INVOICE_DETAILS _selectedInvoiceDetail;
        public INVOICE_DETAILS SelectedInvoiceDetail
        {
            get => _selectedInvoiceDetail;
            set
            {
                if (SetPropertyValue(nameof(SelectedInvoiceDetail), ref _selectedInvoiceDetail, value))
                {
                    if (CommandRemoveItem != null)
                        ((DelegateCommand)CommandRemoveItem).RaiseCanExecuteChanged();
                }
            }
        }

        // --- HOZZÁADÓ PANEL ---
        private bool _isAddItemPanelVisible;
        public bool IsAddItemPanelVisible
        {
            get => _isAddItemPanelVisible;
            set => SetPropertyValue(nameof(IsAddItemPanelVisible), ref _isAddItemPanelVisible, value);
        }

        public ObservableCollection<SERVICES> Services { get; }
        public ObservableCollection<VATRATES> VatRates { get; }

        private SERVICES _selectedService;
        public SERVICES SelectedService
        {
            get => _selectedService;
            set
            {
                if (SetPropertyValue(nameof(SelectedService), ref _selectedService, value))
                {
                    if (value != null)
                    {
                        EditNetUnitPrice = value.NETPRICE;
                        SelectedVatRate = VatRates.FirstOrDefault(v => v.ID == value.VATRATE_ID);

                        // JAVÍTÁS: Betöltjük a leírást az adatbázisból (DESCRIPTION mező)
                        // Ha az adatbázisban NULL lenne, akkor üres stringet adunk
                        EditItemComment = value.DESCRIPTION ?? "";
                    }
                }
            }
        }

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

        // Tétel megjegyzés szerkesztése
        private string _editItemComment;
        public string EditItemComment
        {
            get => _editItemComment;
            set => SetPropertyValue(nameof(EditItemComment), ref _editItemComment, value);
        }

        public string PrimaryButtonText => "Hozzáadás";
        public string SecondaryButtonText => "Mégse";

        // --- LÁBJEGYZET ÉS ÖSSZESÍTŐK ---
        private string _footerNote;
        public string FooterNote
        {
            get => _footerNote;
            set => SetPropertyValue(nameof(FooterNote), ref _footerNote, value);
        }

        public decimal TotalNet => InvoiceDetails?.Sum(x => x.LINE_TOTAL_NET) ?? 0;
        public decimal TotalVat => InvoiceDetails?.Sum(x => x.LINE_TOTAL_GROSS - x.LINE_TOTAL_NET) ?? 0;
        public decimal TotalGross => InvoiceDetails?.Sum(x => x.LINE_TOTAL_GROSS) ?? 0;

        // -------------------------------------------------------------------------
        // PARANCSOK
        // -------------------------------------------------------------------------
        public ICommand CommandSaveInvoice { get; }
        public ICommand CommandCloseWindow { get; }
        public ICommand CommandMenuCLIENTSClick { get; }
        public ICommand CommandMenuSERVICESClick { get; }
        public ICommand CommandAddItem { get; }
        public ICommand CommandRemoveItem { get; }
        public ICommand CommandPrimaryAction { get; }
        public ICommand CommandSecondaryAction { get; }

        public Action CloseWindowAction { get; set; }

        // -------------------------------------------------------------------------
        // LOGIKA
        // -------------------------------------------------------------------------

        private void LoadServices()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    Services.Clear();
                    var list = _servicesTable.GetList(conn);
                    foreach (var s in list) Services.Add(s);
                }
                catch (Exception ex) { Logger.LogError(ex, "Szolgáltatások betöltése hiba"); }
            }
        }

        private void LoadVatRates()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    VatRates.Clear();
                    var list = _vatRatesTable.GetList(conn);
                    foreach (var v in list) VatRates.Add(v);
                }
                catch (Exception ex) { Logger.LogError(ex, "ÁFA kulcsok betöltése hiba"); }
            }
        }

        // --- VEVŐ KIVÁLASZTÁS ---
        private void DoOpenClientsWindow()
        {
            DataContextBase.SelectedClientForInvoice = 0;
            var win = new CLIENTSFrm();
            win.ShowDialog();

            if (DataContextBase.SelectedClientForInvoice > 0)
            {
                LoadClientById(DataContextBase.SelectedClientForInvoice);
            }
        }

        private void LoadClientById(int clientId)
        {
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();
                    var allClients = _clientsTable.GetList(conn);
                    var client = allClients.FirstOrDefault(c => c.ID == clientId);
                    if (client != null)
                    {
                        SelectedClient = client;
                    }
                }
                catch (Exception ex) { Logger.LogError(ex, "Vevő betöltése hiba"); }
            }
        }

        // --- SZOLGÁLTATÁSOK ---
        private void DoOpenServicesWindow()
        {
            var win = new SERVICESFrm();
            win.ShowDialog();
            LoadServices();
        }

        // --- TÉTEL KEZELÉS ---
        private void ShowAddItemPanel()
        {
            SelectedService = null;
            SelectedVatRate = null;
            EditQty = 1;
            EditNetUnitPrice = 0;
            EditItemComment = ""; // Töröljük a megjegyzést nyitáskor
            IsAddItemPanelVisible = true;
        }

        private void HideAddItemPanel()
        {
            IsAddItemPanelVisible = false;
        }

        private void DoSaveItemFromPanel()
        {
            if (SelectedService == null)
            {
                MessageBox.Show("Válassz szolgáltatást!");
                return;
            }
            if (SelectedVatRate == null)
            {
                MessageBox.Show("Válassz ÁFA kulcsot!");
                return;
            }

            // Összeállítjuk a nevet: Szolgáltatás neve + sortörés + Megjegyzés
            // Így a számlán és a PDF-en is külön sorba kerül a leírás
            string finalName = SelectedService.NAME;
            if (!string.IsNullOrWhiteSpace(EditItemComment))
            {
                finalName += Environment.NewLine + EditItemComment;
            }

            var newItem = new INVOICE_DETAILS
            {
                SERVICES_ID = SelectedService.ID,
                SERVICE_NAME = finalName,
                VATRATE_ID = SelectedVatRate.ID,
                QTY = EditQty,
                NET_UNIT_PRICE = EditNetUnitPrice,
                VAT_PERCENT = SelectedVatRate.RATES
            };

            InvoiceDetails.Add(newItem);
            RefreshTotals();
            HideAddItemPanel();

            RefreshSaveButtonState();
        }

        private void DoRemoveItem()
        {
            if (SelectedInvoiceDetail != null)
            {
                InvoiceDetails.Remove(SelectedInvoiceDetail);
                RefreshTotals();
                RefreshSaveButtonState();
            }
        }

        private void RefreshTotals()
        {
            OnPropertyChanged(nameof(TotalNet));
            OnPropertyChanged(nameof(TotalVat));
            OnPropertyChanged(nameof(TotalGross));
        }

        // --- MENTÉS ---
        private void DoSaveInvoice()
        {
            if (!CanSaveInvoice()) return;

            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    // 1. Fejléc
                    var header = new INVOICE_HEADERS
                    {
                        CLIENT_ID = SelectedClient.ID,
                        INVOICE_NUMBER = InvoiceNumber,
                        ISSUE_DATE = IssueDate,
                        DUE_DATE = PaymentDeadline,
                        PAYMENT_METHOD = SelectedPaymentMethod,
                        SZLASTAT = "1",
                        FIZSTAT = "0",
                        STORNO_ID = "0"
                    };

                    _headerTable.Insert(header, conn);

                    // 2. Tételek
                    foreach (var item in InvoiceDetails)
                    {
                        item.INVOICEHEADERS_ID = header.ID;
                        _detailsTable.Insert(item, conn);
                    }

                    MessageBox.Show("Számla mentve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 3. PDF Generálás
                    var pdfManager = new InvoiceExportManager();
                    pdfManager.ExportInvoiceById(header.ID, FooterNote);

                    DoCloseWindow();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Számla mentési hiba");
                    MessageBox.Show("Hiba: " + ex.Message);
                }
            }
        }

        private void DoCloseWindow()
        {
            CloseWindowAction?.Invoke();

            foreach (Window win in Application.Current.Windows)
            {
                if (win.DataContext == this)
                {
                    win.Close();
                    break;
                }
            }
        }
    }
}