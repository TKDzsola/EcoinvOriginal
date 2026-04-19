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
<<<<<<< Updated upstream
            // Német formátum (Ausztria), hogy az euró és a számok helyesek legyenek
            _culture = new CultureInfo("de-AT");
=======
            LoadLogo();
        }

        /// <summary>
        /// A logót előre betöltjük byte[]-be, mert a QuestPDF rendereléskor
        /// a using blokk már lezárná a stream-et — és ezért tűnt el a kép.
        /// Több assembly-ből is próbálkozunk (executing + entry), mert WPF-ben
        /// az embedded resource gyakran a fő projekt assembly-ben van.
        /// </summary>
        private void LoadLogo()
        {
            try
            {
                var assemblies = new[]
                {
                    Assembly.GetExecutingAssembly(),
                    Assembly.GetEntryAssembly()
                };

                foreach (var assembly in assemblies)
                {
                    if (assembly == null) continue;

                    var resourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(x => x.EndsWith("econtologo.jpg", StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(resourceName))
                    {
                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    _logoBytes = ms.ToArray();
                                }
                                break;
                            }
                        }
                    }
                }

                if (_logoBytes == null || _logoBytes.Length == 0)
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var logoPath = Path.Combine(baseDir, "econtologo.jpg");
                    if (File.Exists(logoPath))
                    {
                        _logoBytes = File.ReadAllBytes(logoPath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logo betöltési hiba: {ex.Message}");
            }
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
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
=======
                    if (_logoBytes != null && _logoBytes.Length > 0)
                    {
                        column.Item().Width(150).Image(_logoBytes);
                    }
                    else
                    {
                        column.Item().Text("ECONT").Style(titleStyle);
                    }

                    column.Item().Height(10);
                    column.Item().Text(_model.SellerName).Bold();
                    column.Item().Text(_model.SellerAddress);

                    if (!string.IsNullOrEmpty(_model.SellerTaxNumber))
                        column.Item().Text($"Steuernummer: {_model.SellerTaxNumber}");

                    if (!string.IsNullOrEmpty(_model.SellerEuTaxNumber))
                        column.Item().Text($"UID-Nr: {_model.SellerEuTaxNumber}");
                });
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("RECHNUNG").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().AlignRight().Text($"Nr.: {_model.InvoiceNumber}").FontSize(14).Bold();
                    column.Item().Height(20);
                    column.Item().AlignRight().Text("Kunde (Vevő):").FontSize(9).FontColor(Colors.Grey.Medium);
                    column.Item().AlignRight().Text(_model.ClientName).Bold();
                    column.Item().AlignRight().Text(_model.ClientAddress);

                    if (!string.IsNullOrEmpty(_model.ClientTaxNumber))
                        column.Item().AlignRight().Text($"Steuernummer: {_model.ClientTaxNumber}");

                    if (!string.IsNullOrEmpty(_model.ClientEuTaxNumber))
                        column.Item().AlignRight().Text($"UID-Nr.: {_model.ClientEuTaxNumber}");

                    column.Item().Height(10);
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

                        // Bankverbindung megjelenítése
                        bool hasBank = !string.IsNullOrEmpty(_model.SellerIBAN) || !string.IsNullOrEmpty(_model.SellerBIC) || !string.IsNullOrEmpty(_model.SellerBankAccount);
                        if (hasBank)
                        {
                            c.Item().PaddingTop(5).Text("Bankverbindung:").Bold();

                            if (!string.IsNullOrEmpty(_model.SellerIBAN))
                                c.Item().Text($"IBAN: {_model.SellerIBAN}");

                            if (!string.IsNullOrEmpty(_model.SellerBIC))
                                c.Item().Text($"BIC: {_model.SellerBIC}");

                            if (!string.IsNullOrEmpty(_model.SellerBankAccount) && string.IsNullOrEmpty(_model.SellerIBAN))
                                c.Item().Text($"Konto: {_model.SellerBankAccount}");
                        }
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("Netto:"); r.RelativeItem().AlignRight().Text($"{_model.TotalNet:N2} €"); });
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("USt. (MwSt.):"); r.RelativeItem().AlignRight().Text($"{_model.TotalVat:N2} €"); });
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
                    columns.ConstantColumn(25);     // #
                    columns.RelativeColumn(3);      // Bezeichnung
                    columns.RelativeColumn(1);      // Menge
                    columns.RelativeColumn(1.2f);   // Einzel
                    columns.RelativeColumn(1.2f);   // Netto
                    columns.RelativeColumn(1.2f);   // MwSt
                    columns.RelativeColumn(1.2f);   // Brutto
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

                    static IContainer CellStyle(IContainer container) =>
                        container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                });

                foreach (var item in _model.Items.Select((x, i) => new { Item = x, Index = i + 1 }))
                {
                    table.Cell().Element(CellStyle).Text(item.Index.ToString());

                    table.Cell().Element(CellStyle).Column(c =>
                    {
                        c.Item().Text(item.Item.Name).Bold();
                        if (!string.IsNullOrEmpty(item.Item.Description))
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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
=======
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.Quantity:N0}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.NetUnitPrice:N2} €");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.NetTotal:N2} €");
                    table.Cell().Element(CellStyle).AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text($"{item.Item.VatAmount:N2} €");
                        c.Item().AlignRight().Text($"({item.Item.VatPercent:N0}%)").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.GrossTotal:N2} €").Bold();

                    static IContainer CellStyle(IContainer container) =>
                        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        }
>>>>>>> Stashed changes
    }
}