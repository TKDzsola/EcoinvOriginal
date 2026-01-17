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
            // Osztrák formátum (Dátum: dd.MM.yyyy, Pénznem: 1.234,56 €)
            _culture = new CultureInfo("de-AT");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // Oldal beállítások
                page.Size(PageSizes.A4);
                page.Margin(35, Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // =========================================================
                    // 1. FEJLÉC (HEADER)
                    // =========================================================
                    col.Item().Row(row =>
                    {
                        // --- BAL OLDAL: LOGÓ + CÉGADATOK ---
                        row.RelativeItem().Column(c =>
                        {
                            // A) LOGÓ
                            // Ellenőrizd, hogy ez az útvonal létezik-e a gépeden!
                            string logoPath = @"c:\Users\prozs\source\repos\Ecoinv\Images\econtologo.jpg";

                            if (File.Exists(logoPath))
                            {
                                c.Item().Height(50).Image(logoPath);
                            }

                            // B) ELADÓ ADATAI (Osztrák formátum)

                            // Név (Kiemelve)
                            c.Item().Text(_model.SellerName).FontSize(14).Bold();

                            // Opcionális: Foglalkozás (ha kell, vedd ki a kommentet)
                            // c.Item().Text("Selbstständige Bilanzbuchalterin").FontSize(9); 

                            // Cím formázása: "Irsz Város, Utca Házszám" -> Szétvágjuk a vesszőnél
                            if (!string.IsNullOrEmpty(_model.SellerAddress))
                            {
                                var parts = _model.SellerAddress.Split(',');

                                if (parts.Length > 1)
                                {
                                    // Ha van vessző, cseréljük a sorrendet (Utca felül, Város alul)

                                    // 1. sor: Utca (a vessző utáni rész) -> Pl: "Europastraße 1"
                                    c.Item().Text(parts[1].Trim());

                                    // 2. sor: Irsz + Város (a vessző előtti rész) -> Pl: "A-7540 Güssing"
                                    c.Item().Text(parts[0].Trim());
                                }
                                else
                                {
                                    // Ha nincs vessző, kiírjuk egyben
                                    c.Item().Text(_model.SellerAddress);
                                }
                            }

                            // Ország
                            c.Item().Text("Österreich");

                            // Kis térköz
                            c.Item().PaddingTop(5);

                            // Adószámok (Szürke, kisebb betű)
                            if (!string.IsNullOrEmpty(_model.SellerEuTaxNumber))
                            {
                                c.Item().Text($"UID-Nummer: {_model.SellerEuTaxNumber}")
                                       .FontSize(9).FontColor(Colors.Grey.Darken2);
                            }
                            else if (!string.IsNullOrEmpty(_model.SellerTaxNumber))
                            {
                                c.Item().Text($"Steuernummer: {_model.SellerTaxNumber}")
                                       .FontSize(9).FontColor(Colors.Grey.Darken2);
                            }
                        });

                        // --- JOBB OLDAL: SZÁMLA FELIRAT + SORSZÁM ---
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("RECHNUNG").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Nr.: {_model.InvoiceNumber}").FontSize(12);
                        });
                    });

                    // Vonal a fejléc alatt
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Dátumok (jobbra igazítva)
                    col.Item().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Ausstellungsdatum: {_model.IssueDate.ToString("d", _culture)}");
                        c.Item().Text($"Fälligkeitsdatum: {_model.DueDate.ToString("d", _culture)}");
                    });

                    col.Spacing(20);

                    // =========================================================
                    // 2. VEVŐ ADATAI (Keretben)
                    // =========================================================
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Item().Text("Empfänger:").FontSize(8).FontColor(Colors.Grey.Darken2);
                        c.Item().Text(_model.ClientName).Bold();
                        c.Item().Text(_model.ClientAddress);

                        if (!string.IsNullOrEmpty(_model.ClientTaxNumber))
                        {
                            c.Item().Text($"Steuernummer/UID: {_model.ClientTaxNumber}");
                        }
                    });

                    col.Spacing(20);

                    // =========================================================
                    // 3. TÉTELEK TÁBLÁZAT
                    // =========================================================
                    col.Item().Table(table =>
                    {
                        // Oszlopok definíciója
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4); // Megnevezés
                            columns.RelativeColumn();  // Mennyiség
                            columns.RelativeColumn();  // Egységár
                            columns.RelativeColumn();  // Nettó
                        });

                        // Fejléc
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Beschreibung");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Menge");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Einzelpreis");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Netto");
                        });

                        // Sorok
                        foreach (var item in _model.Items)
                        {
                            table.Cell().Element(CellStyle).Text(item.Description);
                            table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("0.##", _culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(item.NetUnitPrice.ToString("N2", _culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(item.NetTotal.ToString("N2", _culture));
                        }
                    });

                    // =========================================================
                    // 4. LÁBLÉC (Összesítés + Bank)
                    // =========================================================
                    col.Spacing(10);
                    col.Item().Row(row =>
                    {
                        // Bal oldal: Banki adatok
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Zahlungsinformationen:").Bold();
                            c.Item().Text($"Zahlungsart: {_model.PaymentMethod}");
                            c.Item().Text($"Bankverbindung (IBAN): {_model.SellerBankAccount}");

                            if (!string.IsNullOrEmpty(_model.SellerTaxNumber))
                                c.Item().Text($"Steuernummer: {_model.SellerTaxNumber}");
                        });

                        // Jobb oldal: Végösszesen
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            // Nettó
                            c.Item().Row(r => {
                                r.RelativeItem().Text("Netto:").AlignRight();
                                r.RelativeItem().Text($"{_model.TotalNet.ToString("N2", _culture)} €").AlignRight();
                            });

                            // ÁFA
                            c.Item().Row(r => {
                                r.RelativeItem().Text("USt. (MwSt.):").AlignRight();
                                r.RelativeItem().Text($"{_model.TotalVat.ToString("N2", _culture)} €").AlignRight();
                            });

                            c.Item().PaddingVertical(5).LineHorizontal(1);

                            // Bruttó
                            c.Item().Row(r => {
                                r.RelativeItem().Text("Gesamtbetrag:").Bold().FontSize(12).AlignRight();
                                r.RelativeItem().Text($"{_model.TotalGross.ToString("N2", _culture)} €").Bold().FontSize(12).AlignRight();
                            });
                        });
                    });

                    // Oldalszámozás
                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Seite ");
                        x.CurrentPageNumber();
                        x.Span(" von ");
                        x.TotalPages();
                    });
                });
            });
        }

        // --- STÍLUS FÜGGVÉNYEK ---

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(5)
                .PaddingHorizontal(2)
                .DefaultTextStyle(x => x.SemiBold());
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(2);
        }
    }
}