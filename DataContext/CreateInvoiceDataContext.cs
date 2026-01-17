using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using Ecoinv.Pdf.Services; // <--- FONTOS: Ez kell a PDF generáláshoz!
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
        // DB KAPCSOLAT
        // =====================================================
        private readonly FBConnectX _conn;

        // =====================================================
        // KONSTRUKTOR
        // =====================================================
        public CreateInvoiceDataContext()
        {
            // Kapcsolat inicializálása
            _conn = new FBConnectX();
            _conn.GetConnectionX();

            // Listák inicializálása
            InvoiceDetails = new ObservableCollection<INVOICE_DETAILS>();
            Services = new ObservableCollection<SERVICES>();
            VatRates = new ObservableCollection<VATRATES>();

            // Adatok betöltése
            LoadServices();
            LoadVatRates();

            // Parancsok (Command) létrehozása
            CommandPrimaryAction = new DelegateCommand(_ => PrimaryAction());
            CommandSecondaryAction = new DelegateCommand(_ => SecondaryAction());
            CommandSaveInvoice = new DelegateCommand(_ => SaveInvoice(), _ => CanSaveInvoice());
            CommandCancelInvoice = new DelegateCommand(_ => CancelInvoice());
            CommandPreviewInvoice = new DelegateCommand(_ => PreviewInvoice());

            // Gombszövegek alapállapota
            UpdateActionButtonTexts();

            // Ha volt kiválasztva ügyfél a kliens listából, azt betöltjük
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

            // Bezárás után: olvassuk vissza, mit választott a user
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

                // Név lekérése DB-ből a megjelenítéshez
                var ct = new CLIENTSTable();
                var list = ct.GetList(_conn);

                var cli = list?.FirstOrDefault(x => x.ID == id);
                SelectedClientName = cli?.NAME ?? $"(ID: {id})";
            }
            catch
            {
                SelectedClientId = 0;
                SelectedClientName = string.Empty;
            }
        }

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
        // TÉTELEK LISTA
        // =====================================================
        public ObservableCollection<INVOICE_DETAILS> InvoiceDetails { get; }

        private INVOICE_DETAILS _selectedInvoiceDetail;
        public INVOICE_DETAILS SelectedInvoiceDetail
        {
            get => _selectedInvoiceDetail;
            set => SetPropertyValue(nameof(SelectedInvoiceDetail), ref _selectedInvoiceDetail, value);
        }

        // =====================================================
        // TÖRZSADATOK (Szolgáltatások, ÁFA)
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
                    // Ha kiválasztunk egy szolgáltatást, töltsük ki az árat automatikusan
                    if (value != null)
                        EditNetUnitPrice = value.NETPRICE;
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

        // =====================================================
        // SZERKESZTÉS MEZŐI (Panelen)
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
        // ÖSSZESÍTÉS MEZŐI
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
        // UI ÁLLAPOT ÉS GOMBSZÖVEGEK
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

        // A gombok szövegei (direkt property, hogy mindig frissüljön a UI)
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
            // Itt történik a szöveg váltása a panel láthatósága alapján
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
        // TÉTEL HOZZÁADÁS / TÖRLÉS LOGIKA
        // =====================================================
        private void PrimaryAction()
        {
            // Ha nincs nyitva a panel -> Megnyitjuk
            if (!IsAddItemPanelVisible)
            {
                IsAddItemPanelVisible = true;
                SelectedService = null;
                SelectedVatRate = null;
                EditQty = 1;
                EditNetUnitPrice = 0;
                return;
            }

            // Ha nyitva van -> Hozzáadás a listához
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
            IsAddItemPanelVisible = false; // Panel bezárása
        }

        private void SecondaryAction()
        {
            // Ha nyitva van a panel -> Mégse (bezárás)
            if (IsAddItemPanelVisible)
            {
                IsAddItemPanelVisible = false;
                return;
            }

            // Ha zárva van -> Kijelölt sor törlése
            if (SelectedInvoiceDetail != null)
            {
                InvoiceDetails.Remove(SelectedInvoiceDetail);
                RecalculateTotals();
            }
        }

        // =====================================================
        // MENTÉS – ADATBÁZIS + PDF GENERÁLÁS
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

                // --- 1. MENTÉS ADATBÁZISBA ---
                var headerTable = new INVOICE_HEADERSTable();

                var header = new INVOICE_HEADERS
                {
                    CLIENT_ID = SelectedClientId,
                    INVOICE_NUMBER = InvoiceNumber,
                    ISSUE_DATE = DateTime.Today,
                    DUE_DATE = DateTime.Today.AddDays(14),
                    CREATED = DateTime.Now,
                    PAYMENT_METHOD = "Készpénz", // Ez alapértelmezett, később UI-ról jöhet
                    SZLASTAT = "0",
                    FIZSTAT = "0",
                    STORNO_ID = "0"
                };

                // Fejléc beszúrása
                headerTable.Insert(header, _conn);

                // Tételek beszúrása
                var detailTable = new INVOICE_DETAILSTable();

                foreach (var item in InvoiceDetails)
                {
                    item.INVOICEHEADERS_ID = header.ID;
                    detailTable.Insert(item, _conn);
                }

                MessageBox.Show("Számla sikeresen mentve az adatbázisba.", "Kész", MessageBoxButton.OK, MessageBoxImage.Information);

                // =========================================================
                // 🚀 2. PDF GENERÁLÁS AUTOMATIKUS INDÍTÁSA
                // =========================================================
                try
                {
                    // Példányosítjuk a PDF kezelőt
                    var exportManager = new InvoiceExportManager();

                    // Meghívjuk a generálást az új számla ID-jával
                    exportManager.ExportInvoiceById(header.ID);

                    // Ha a PDF generálás lefutott, törölhetjük az űrlapot a következő számlához
                    CancelInvoice();
                }
                catch (Exception pdfEx)
                {
                    MessageBox.Show($"Az adatbázisba mentés sikerült, de a PDF generálás során hiba történt:\n{pdfEx.Message}",
                                    "PDF Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // =========================================================
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mentési hiba:\n{ex.Message}", "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================
        // EGYEBEK (Mégse, Előnézet)
        // =====================================================
        private void CancelInvoice()
        {
            InvoiceDetails.Clear();
            InvoiceNumber = string.Empty;
            RecalculateTotals();
            // Panel alaphelyzetbe
            IsAddItemPanelVisible = false;
        }

        private void PreviewInvoice()
        {
            MessageBox.Show("PDF előnézet funkció hamarosan...");
        }

        // =====================================================
        // BETÖLTÉSEK (Szolgáltatások, ÁFA)
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