using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ecoinv.Pdf.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ecoinv.Pdf.Documents
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly InvoicePdfModel _model;

        public InvoicePdfDocument(InvoicePdfModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                // --- BAL OLDAL: LOGÓ (OKOS KERESÉS) ---
                row.RelativeItem().Column(column =>
                {
                    bool logoFound = false;

                    // 1. PRÓBA: Beágyazott erőforrás keresése (Névtől függetlenül)
                    try
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        // Megkeressük az első olyan erőforrást, aminek a vége "econtologo.jpg"
                        var resourceName = assembly.GetManifestResourceNames()
                                                   .FirstOrDefault(x => x.EndsWith("econtologo.jpg", StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(resourceName))
                        {
                            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                            {
                                if (stream != null)
                                {
                                    column.Item().Width(150).Image(stream);
                                    logoFound = true;
                                }
                            }
                        }
                    }
                    catch { /* Ha hiba van, továbbmegyünk a fájl keresésre */ }

                    // 2. PRÓBA: Ha nem volt beágyazva, keressük fájlként
                    if (!logoFound)
                    {
                        string basePath = AppDomain.CurrentDomain.BaseDirectory;
                        string logoPath = Path.Combine(basePath, "econtologo.jpg");

                        if (File.Exists(logoPath))
                        {
                            try
                            {
                                byte[] bytes = File.ReadAllBytes(logoPath);
                                column.Item().Width(150).Image(bytes);
                                logoFound = true;
                            }
                            catch { }
                        }
                    }

                    // HA MÉG MINDIG NINCS: Szöveges helyettesítő
                    if (!logoFound)
                    {
                        column.Item().Text("ECONT").Style(titleStyle);
                    }

                    column.Item().Height(10);

                    // Eladó adatai
                    column.Item().Text(_model.SellerName).Bold();
                    column.Item().Text(_model.SellerAddress);
                    column.Item().Text($"Steuernummer: {_model.SellerTaxNumber}");
                    if (!string.IsNullOrEmpty(_model.SellerEuTaxNumber))
                        column.Item().Text($"UID-Nr: {_model.SellerEuTaxNumber}");
                });

                // --- JOBB OLDAL ---
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("RECHNUNG").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().AlignRight().Text($"Nr.: {_model.InvoiceNumber}").FontSize(14).Bold();
                    column.Item().Height(20);

                    column.Item().AlignRight().Text("Kunde (Vevő):").FontColor(Colors.Grey.Medium).FontSize(9);
                    column.Item().AlignRight().Text(_model.ClientName).Bold();
                    if (!string.IsNullOrEmpty(_model.ClientAddress))
                        column.Item().AlignRight().Text(_model.ClientAddress);
                    if (!string.IsNullOrEmpty(_model.ClientTaxNumber))
                        column.Item().AlignRight().Text($"Steuernummer: {_model.ClientTaxNumber}");
                    if (!string.IsNullOrEmpty(_model.ClientEuTaxNumber))
                        column.Item().AlignRight().Text($"UID-Nr: {_model.ClientEuTaxNumber}");

                    column.Item().Height(15);
                    column.Item().AlignRight().Text($"Ausstellungsdatum: {_model.IssueDate:yyyy.MM.dd}");
                    column.Item().AlignRight().Text($"Fälligkeitsdatum: {_model.DueDate:yyyy.MM.dd}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Item().Element(ComposeTable);

                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Zahlungsmethode: {_model.PaymentMethod}");
                        c.Item().Height(10);
                        if (!string.IsNullOrEmpty(_model.SellerBankAccount) || !string.IsNullOrEmpty(_model.SellerIBAN))
                        {
                            c.Item().Text("Bankverbindung:").Bold();
                            if (!string.IsNullOrEmpty(_model.SellerIBAN)) c.Item().Text($"IBAN: {_model.SellerIBAN}");
                            if (!string.IsNullOrEmpty(_model.SellerBIC)) c.Item().Text($"BIC: {_model.SellerBIC}");
                            if (string.IsNullOrEmpty(_model.SellerIBAN) && !string.IsNullOrEmpty(_model.SellerBankAccount))
                                c.Item().Text($"Konto: {_model.SellerBankAccount}");
                        }
                        if (!string.IsNullOrEmpty(_model.Comment))
                        {
                            c.Item().Height(20);
                            c.Item().Text("Bemerkung / Megjegyzés:").Bold().FontSize(9);
                            c.Item().Text(_model.Comment).Italic();
                        }
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("Netto:"); r.RelativeItem().AlignRight().Text($"{_model.TotalNet:N2} €"); });
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("USt. (MwSt.):"); r.RelativeItem().AlignRight().Text($"{_model.TotalVat:N2} €"); });
                        c.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        c.Item().PaddingTop(5).Row(r => { r.RelativeItem().AlignRight().Text("Gesamtbetrag:").FontSize(14).Bold(); r.RelativeItem().AlignRight().Text($"{_model.TotalGross:N2} €").FontSize(14).Bold(); });
                    });
                });
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").Bold();
                    header.Cell().Element(CellStyle).Text("Bezeichnung").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Menge").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Einzel").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Netto").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("MwSt").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Brutto").Bold();
                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                });

                foreach (var item in _model.Items.Select((x, i) => new { Item = x, Index = i + 1 }))
                {
                    table.Cell().Element(CellStyle).Text(item.Index.ToString());

                    // --- NÉV ÉS MEGJEGYZÉS KEZELÉSE ---
                    table.Cell().Element(CellStyle).Column(c =>
                    {
                        var rawName = item.Item.Name ?? "";
                        var normalizedName = rawName.Replace("\r\n", "\n").Replace("\r", "\n");
                        var parts = normalizedName.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length > 0)
                        {
                            // 1. SOR: Név (Vastag)
                            c.Item().Text(parts[0]).Bold();

                            // TÖBBI SOR: Megjegyzés (Szürke, Dőlt)
                            for (int j = 1; j < parts.Length; j++)
                            {
                                c.Item().Text(parts[j]).FontSize(9).FontColor(Colors.Grey.Darken1).Italic();
                            }
                        }
                    });

                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.Quantity:N0}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.NetUnitPrice:N2} €");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.NetTotal:N2} €");
                    table.Cell().Element(CellStyle).AlignRight().Column(c => { c.Item().Text($"{item.Item.VatAmount:N2} €"); c.Item().Text($"({item.Item.VatPercent}%)").FontSize(8).Italic(); });
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.GrossTotal:N2} €").Bold();

                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text => { text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); });
        }
    }
}