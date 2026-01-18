using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Pdf.Documents;
using Ecoinv.Pdf.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Ecoinv.Pdf.Services
{
    public class InvoiceExportManager
    {
        private readonly InvoicePdfService _pdfService;

        public InvoiceExportManager()
        {
            // Licenc beállítása
            QuestPDF.Settings.License = LicenseType.Community;
            _pdfService = new InvoicePdfService();
        }

        public void ExportInvoiceById(int invoiceId)
        {
            // =============================================================
            // ÚJ: 'using' blokk használata -> Automatikus lezárás!
            // =============================================================
            using (FBConnectX conn = new FBConnectX())
            {
                try
                {
                    conn.GetConnectionX();
                    conn.FBConnOpenX();

                    if (conn.GetConStateX() != System.Data.ConnectionState.Open)
                    {
                        return; // A hibaüzenet már lement a FBConnectX-ben
                    }

                    // --- ADATOK LEKÉRÉSE ---
                    var headerTable = new INVOICE_HEADERSTable();
                    var detailTable = new INVOICE_DETAILSTable();
                    var clientTable = new CLIENTSTable();
                    var ecsysTable = new ECSYSTable();
                    var addressTable = new ADRESSESTable();
                    var serviceTable = new SERVICESTable();

                    var allHeaders = headerTable.GetList(conn);
                    var header = allHeaders.FirstOrDefault(x => x.ID == invoiceId);

                    if (header == null)
                    {
                        MessageBox.Show("A számla nem található!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var allDetails = detailTable.GetList(conn);
                    var details = allDetails.Where(x => x.INVOICEHEADERS_ID == invoiceId).ToList();

                    var allServices = serviceTable.GetList(conn);
                    foreach (var item in details)
                    {
                        var serv = allServices.FirstOrDefault(s => s.ID == item.SERVICES_ID);
                        if (serv != null) item.SERVICE_NAME = serv.NAME;
                    }

                    var allClients = clientTable.GetList(conn);
                    CLIENTS client = null;
                    ADRESSES address = null;

                    if (header.CLIENT_ID > 0)
                    {
                        client = allClients.FirstOrDefault(x => x.ID == header.CLIENT_ID);
                        if (client != null)
                        {
                            var addresses = addressTable.GetList(conn, client.ID);
                            address = addresses.FirstOrDefault();
                        }
                    }

                    var allEcsys = ecsysTable.GetList(conn);
                    var sellerData = allEcsys.FirstOrDefault();

                    // --- MODELL ÉPÍTÉSE ---
                    InvoicePdfModel pdfModel = _pdfService.BuildInvoicePdfModel(header, details, client, address, sellerData);

                    // =============================================================
                    // SZTORNÓ KEZELÉS (Javított)
                    // =============================================================
                    bool isStornoStatus = (header.SZLASTAT != null && header.SZLASTAT.Trim() == "2");
                    bool isStornoNumber = (header.INVOICE_NUMBER != null && header.INVOICE_NUMBER.Trim().ToUpper().StartsWith("ST-"));

                    if (isStornoStatus || isStornoNumber)
                    {
                        pdfModel.IsStorno = true;

                        if (!string.IsNullOrEmpty(header.STORNO_ID) && int.TryParse(header.STORNO_ID.Trim(), out int originalId))
                        {
                            var originalHeader = allHeaders.FirstOrDefault(h => h.ID == originalId);
                            if (originalHeader != null)
                            {
                                pdfModel.OriginalInvoiceNumber = originalHeader.INVOICE_NUMBER;
                            }
                        }

                        // Mínuszolás
                        pdfModel.TotalNet = Math.Abs(pdfModel.TotalNet) * -1;
                        pdfModel.TotalVat = Math.Abs(pdfModel.TotalVat) * -1;
                        pdfModel.TotalGross = Math.Abs(pdfModel.TotalGross) * -1;

                        foreach (var item in pdfModel.Items)
                        {
                            item.NetUnitPrice = Math.Abs(item.NetUnitPrice) * -1;
                            item.NetTotal = Math.Abs(item.NetTotal) * -1;
                            item.VatAmount = Math.Abs(item.VatAmount) * -1;
                            item.GrossTotal = Math.Abs(item.GrossTotal) * -1;
                        }
                    }

                    // --- MENTÉS ---
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF dokumentum (*.pdf)|*.pdf",
                        FileName = $"Rechnung_{header.INVOICE_NUMBER.Replace("/", "-")}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        var document = new InvoicePdfDocument(pdfModel);
                        document.GeneratePdf(saveDialog.FileName);

                        if (MessageBox.Show("A PDF elkészült!\nSzeretnéd megnyitni?", "Kész", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // NINCS FINALLY BLOKK! A using automatikusan lezárja a kapcsolatot.
            }
        }
    }
}