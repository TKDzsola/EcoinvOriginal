using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ecoinv.Pdf.Models;
using System.Globalization;
using System.IO;

namespace Ecoinv.Pdf.Documents
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly InvoicePdfModel _model;
        private readonly CultureInfo _culture;

        public InvoicePdfDocument(InvoicePdfModel model)
        {
            _model = model;
            _culture = new CultureInfo("de-AT");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35, Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // FEJLÉC
                    col.Item().Row(row =>
                    {
                        // BAL OLDAL
                        row.RelativeItem().Column(c =>
                        {
                            string logoPath = @"c:\Users\prozs\source\repos\Ecoinv\Images\econtologo.jpg";
                            if (File.Exists(logoPath)) c.Item().Height(50).Image(logoPath);

                            c.Item().Text(_model.SellerName).FontSize(14).Bold();
                            if (!string.IsNullOrEmpty(_model.SellerAddress))
                            {
                                var parts = _model.SellerAddress.Split(',');
                                if (parts.Length > 1) { c.Item().Text(parts[1].Trim()); c.Item().Text(parts[0].Trim()); }
                                else { c.Item().Text(_model.SellerAddress); }
                            }
                            c.Item().Text("Österreich");
                            c.Item().PaddingTop(5);
                            if (!string.IsNullOrEmpty(_model.SellerEuTaxNumber)) c.Item().Text($"UID-Nummer: {_model.SellerEuTaxNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });

                        // JOBB OLDAL (EZT FIGYELD!)
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            if (_model.IsStorno)
                            {
                                // --- HA SZTORNÓ ---
                                c.Item().Text("STORNORECHNUNG")
                                    .FontSize(20).Bold().FontColor(Colors.Red.Medium);
                                c.Item().Text($"Nr.: {_model.InvoiceNumber}").FontSize(12);

                                if (!string.IsNullOrEmpty(_model.OriginalInvoiceNumber))
                                {
                                    c.Item().PaddingTop(5);
                                    c.Item().Text("Korrektur zu Rechnung:").FontSize(10).FontColor(Colors.Red.Medium);
                                    c.Item().Text($"Nr.: {_model.OriginalInvoiceNumber}").FontSize(10).Bold().FontColor(Colors.Red.Medium);
                                }
                            }
                            else
                            {
                                // --- HA SIMA ---
                                c.Item().Text("RECHNUNG")
                                    .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                c.Item().Text($"Nr.: {_model.InvoiceNumber}").FontSize(12);
                            }
                        });
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // DÁTUMOK
                    col.Item().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Ausstellungsdatum: {_model.IssueDate.ToString("d", _culture)}");
                        c.Item().Text($"Fälligkeitsdatum: {_model.DueDate.ToString("d", _culture)}");
                    });

                    col.Spacing(20);

                    // VEVŐ
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Item().Text("Empfänger:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(_model.ClientName).Bold();
                        if (!string.IsNullOrEmpty(_model.ClientAddress))
                        {
                            var parts = _model.ClientAddress.Split(',');
                            if (parts.Length > 1) { c.Item().Text(parts[1].Trim()); c.Item().Text(parts[0].Trim()); }
                            else { c.Item().Text(_model.ClientAddress); }
                        }
                        if (!string.IsNullOrEmpty(_model.ClientTaxNumber)) c.Item().Text($"Steuernummer/UID: {_model.ClientTaxNumber}");
                    });

                    col.Spacing(20);

                    // TÁBLÁZAT
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns => { columns.RelativeColumn(4); columns.RelativeColumn(); columns.RelativeColumn(); columns.RelativeColumn(); });
                        table.Header(header => {
                            header.Cell().Element(HeaderStyle).Text("Beschreibung");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Menge");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Einzelpreis");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Netto");
                        });

                        foreach (var item in _model.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.Description);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("0.##", _culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(item.NetUnitPrice.ToString("N2", _culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(item.NetTotal.ToString("N2", _culture));
                        }
                    });

                    // LÁBLÉC (ÖSSZESÍTÉS)
                    col.Spacing(10);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("Zahlungsinformationen:").Bold();
                            c.Item().Text($"Zahlungsart: {_model.PaymentMethod}");
                            c.Item().Text($"Bankverbindung (IBAN): {_model.SellerBankAccount}");
                        });

                        row.RelativeItem().AlignRight().Column(c => {
                            c.Item().Row(r => { r.RelativeItem().Text("Netto:").AlignRight(); r.RelativeItem().Text($"{_model.TotalNet.ToString("N2", _culture)} €").AlignRight(); });
                            c.Item().Row(r => { r.RelativeItem().Text("USt. (MwSt.):").AlignRight(); r.RelativeItem().Text($"{_model.TotalVat.ToString("N2", _culture)} €").AlignRight(); });
                            c.Item().PaddingVertical(5).LineHorizontal(1);
                            c.Item().Row(r => { r.RelativeItem().Text("Gesamtbetrag:").Bold().FontSize(12).AlignRight(); r.RelativeItem().Text($"{_model.TotalGross.ToString("N2", _culture)} €").Bold().FontSize(12).AlignRight(); });
                        });
                    });
                });
            });
        }

        static IContainer HeaderStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Darken1).PaddingVertical(5).PaddingHorizontal(2).DefaultTextStyle(x => x.SemiBold());
        static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(2);
    }
}