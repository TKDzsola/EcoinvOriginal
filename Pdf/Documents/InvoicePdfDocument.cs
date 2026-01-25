using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ecoinv.Pdf.Models;
using System.Globalization;
using System.IO;
using System;

namespace Ecoinv.Pdf.Documents
{
    public class InvoicePdfDocument : IDocument
    {
        private readonly InvoicePdfModel _model;
        private readonly CultureInfo _culture;

        public InvoicePdfDocument(InvoicePdfModel model)
        {
            _model = model;
            // Német formátum (Ausztria), hogy az euró és a számok helyesek legyenek
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

                    // =========================================================
                    // 1. FEJLÉC (Logó, Eladó, Vevő, Számlaadatok)
                    // =========================================================
                    col.Item().Row(row =>
                    {
                        // BAL OLDAL: Logó és Eladó adatai
                        row.RelativeItem().Column(c =>
                        {
                            // --- LOGÓ JAVÍTÁS START ---
                            // Megkeressük az EXE mappáját, és onnan töltjük be a képet
                            string basePath = AppDomain.CurrentDomain.BaseDirectory;
                            string logoPath = Path.Combine(basePath, "econtologo.jpg");

                            if (File.Exists(logoPath))
                            {
                                c.Item().Height(60).Image(logoPath);
                            }
                            else
                            {
                                // Ha véletlenül mégsem lenne ott, szövegesen kiírjuk
                                c.Item().Text("ECONT").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            }
                            // --- LOGÓ JAVÍTÁS END ---

                            c.Item().PaddingTop(10).Text(_model.SellerName).Bold().FontSize(11);
                            c.Item().Text(_model.SellerAddress);
                            c.Item().Text($"Steuernummer: {_model.SellerTaxNumber}");
                            if (!string.IsNullOrEmpty(_model.SellerEuTaxNumber))
                                c.Item().Text($"UID-Nr: {_model.SellerEuTaxNumber}");
                        });

                        // JOBB OLDAL: Címzett (Vevő) és Számla infók
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("RECHNUNG").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                            c.Item().PaddingTop(5).Text($"Nr.: {_model.InvoiceNumber}").FontSize(12).Bold();

                            c.Item().PaddingTop(15).Text("Kunde (Vevő):").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(_model.ClientName).Bold();
                            c.Item().Text(_model.ClientAddress);
                            if (!string.IsNullOrEmpty(_model.ClientTaxNumber))
                                c.Item().Text($"Steuernummer: {_model.ClientTaxNumber}");

                            c.Item().PaddingTop(10).Text($"Ausstellungsdatum: {_model.IssueDate:yyyy.MM.dd}");
                            c.Item().Text($"Fälligkeitsdatum: {_model.DueDate:yyyy.MM.dd}");

                            // Sztornó jelzés, ha van
                            if (_model.IsStorno)
                            {
                                c.Item().PaddingTop(5).Text("STORNO RECHNUNG").FontColor(Colors.Red.Medium).Bold();
                                c.Item().Text($"Original: {_model.OriginalInvoiceNumber}").FontColor(Colors.Red.Medium);
                            }
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // =========================================================
                    // 2. TÁBLÁZAT (Tételek)
                    // =========================================================
                    col.Item().Table(table =>
                    {
                        // Oszlopok definíciója
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);  // Sorszám
                            columns.RelativeColumn(4);   // Megnevezés
                            columns.RelativeColumn(1);   // Menny.
                            columns.RelativeColumn(2);   // Egységár
                            columns.RelativeColumn(2);   // Nettó
                            columns.RelativeColumn(2);   // ÁFA
                            columns.RelativeColumn(2);   // Bruttó
                        });

                        // Fejléc
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Bezeichnung");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Menge");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Einzel");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Netto");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("MwSt");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Brutto");
                        });

                        // Sorok
                        int i = 1;
                        foreach (var item in _model.Items)
                        {
                            table.Cell().Element(CellStyle).Text($"{i}.");
                            table.Cell().Element(CellStyle).Column(c =>
                            {
                                c.Item().Text(item.Name).SemiBold();
                                if (!string.IsNullOrEmpty(item.Description))
                                    c.Item().Text(item.Description).FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity:N0}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.NetUnitPrice.ToString("N2", _culture)} €");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.NetTotal.ToString("N2", _culture)} €");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.VatAmount.ToString("N2", _culture)} €\n({item.VatPercent:0}%)").FontSize(9);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.GrossTotal.ToString("N2", _culture)} €").Bold();
                            i++;
                        }
                    });

                    // =========================================================
                    // 3. LÁBLÉC (Összesítő és Bank adatok)
                    // =========================================================
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // Bal oldal: Fizetési mód és Bank adatok
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Zahlungsmethode: {_model.PaymentMethod}");

                            c.Item().PaddingTop(10).Text("Bankverbindung:").Bold();
                            if (!string.IsNullOrEmpty(_model.SellerIBAN))
                                c.Item().Text($"IBAN: {_model.SellerIBAN}");
                            if (!string.IsNullOrEmpty(_model.SellerBIC))
                                c.Item().Text($"BIC: {_model.SellerBIC}");

                            // Ha nincs IBAN, de van régi bankszámla
                            if (string.IsNullOrEmpty(_model.SellerIBAN) && !string.IsNullOrEmpty(_model.SellerBankAccount))
                                c.Item().Text($"Konto: {_model.SellerBankAccount}");

                            if (!string.IsNullOrEmpty(_model.Comment))
                                c.Item().PaddingTop(10).Text($"Bemerkung: {_model.Comment}").Italic();
                        });

                        // Jobb oldal: Összesítő számok
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Row(r => { r.RelativeItem().Text("Netto:").AlignRight(); r.RelativeItem().Text($"{_model.TotalNet.ToString("N2", _culture)} €").AlignRight(); });
                            c.Item().Row(r => { r.RelativeItem().Text("USt. (MwSt.):").AlignRight(); r.RelativeItem().Text($"{_model.TotalVat.ToString("N2", _culture)} €").AlignRight(); });

                            c.Item().PaddingVertical(5).LineHorizontal(1);

                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Gesamtbetrag:").Bold().FontSize(12).AlignRight();
                                r.RelativeItem().Text($"{_model.TotalGross.ToString("N2", _culture)} €").Bold().FontSize(14).AlignRight();
                            });
                        });
                    });
                });

                // Oldalszám az aljára
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Seite ");
                    x.CurrentPageNumber();
                    x.Span(" von ");
                    x.TotalPages();
                });
            });
        }

        // Stílus segédfüggvények
        static IContainer HeaderStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Darken1).PaddingVertical(5).PaddingHorizontal(2).DefaultTextStyle(x => x.SemiBold());
        static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(2);
    }
}