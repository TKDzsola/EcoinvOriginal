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
using QuestPDF.Infrastructure; // <--- EZT NE FELEJTSD EL HOZZÁADNI!

namespace Ecoinv.Pdf.Services
{
    public class InvoiceExportManager
    {
        private readonly InvoicePdfService _pdfService;

        public InvoiceExportManager()
        {
            // =================================================================
            // 🛑 JAVÍTÁS: QuestPDF Licenc beállítása (Kötelező!)
            // =================================================================
            // Ha éles üzleti környezetben használod nagy árbevételű cégnél, 
            // akkor LicenseType.Commercial kell, de fejlesztéshez/kisebb cégeknél:
            QuestPDF.Settings.License = LicenseType.Community;

            _pdfService = new InvoicePdfService();
        }

        public void ExportInvoiceById(int invoiceId)
        {
            // 1. KAPCSOLAT (FBConnectX)
            FBConnectX conn = new FBConnectX();

            try
            {
                conn.GetConnectionX();
                conn.FBConnOpenX();

                if (conn.GetConStateX() != System.Data.ConnectionState.Open)
                {
                    MessageBox.Show("Nem sikerült kapcsolódni az adatbázishoz!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2. TÁBLÁK PÉLDÁNYOSÍTÁSA
                var headerTable = new INVOICE_HEADERSTable();
                var detailTable = new INVOICE_DETAILSTable();
                var clientTable = new CLIENTSTable();
                var ecsysTable = new ECSYSTable();
                var addressTable = new ADRESSESTable();
                var serviceTable = new SERVICESTable();

                // 3. ADATOK LEKÉRÉSE

                // A. Számla fejléc
                var allHeaders = headerTable.GetList(conn);
                var header = allHeaders.FirstOrDefault(x => x.ID == invoiceId);

                if (header == null)
                {
                    MessageBox.Show("A keresett számla nem található az adatbázisban!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // B. Tételek
                var allDetails = detailTable.GetList(conn);
                var details = allDetails.Where(x => x.INVOICEHEADERS_ID == invoiceId).ToList();

                // C. Szolgáltatás nevek betöltése
                var allServices = serviceTable.GetList(conn);
                foreach (var item in details)
                {
                    var serv = allServices.FirstOrDefault(s => s.ID == item.SERVICES_ID);
                    if (serv != null)
                    {
                        item.SERVICE_NAME = serv.NAME;
                    }
                }

                // D. Ügyfél és Címe
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

                // E. Saját Cégadatok (ECSYS)
                var allEcsys = ecsysTable.GetList(conn);
                var sellerData = allEcsys.FirstOrDefault();

                if (sellerData == null)
                {
                    // MessageBox.Show("Figyelem: Az ECSYS tábla üres...", ...); // Opcionális figyelmeztetés
                }

                // 4. PDF MODELL ÉPÍTÉSE
                InvoicePdfModel pdfModel = _pdfService.BuildInvoicePdfModel(header, details, client, address, sellerData);

                // 5. MENTÉS
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF dokumentum (*.pdf)|*.pdf",
                    FileName = $"Rechnung_{header.INVOICE_NUMBER.Replace("/", "-")}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // 6. GENERÁLÁS
                    var document = new InvoicePdfDocument(pdfModel);
                    document.GeneratePdf(saveDialog.FileName);

                    // 7. SIKER
                    var result = MessageBox.Show("A számla PDF exportálása sikeres!\nSzeretnéd most megnyitni?",
                                                 "Kész",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a PDF generálás során:\n{ex.Message}\n{ex.StackTrace}",
                                "Kritikus hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                conn?.FBConnCloseX();
            }
        }
    }
}