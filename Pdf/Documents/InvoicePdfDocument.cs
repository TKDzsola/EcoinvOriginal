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
        private byte[] _logoBytes;

        public InvoicePdfDocument(InvoicePdfModel model)
        {
            _model = model;
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
                                break; // sikerült, nem kell tovább keresni
                            }
                        }
                    }
                }

                // Ha embedded resource nem található, próbáljuk fájlból betölteni
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
                row.RelativeItem().Column(column =>
                {
                    // JAVÍTVA: byte[]-ből töltjük a logót, így a rendereléskor is elérhető
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
                        column.Item().Text($"UID-Nr.: {_model.SellerEuTaxNumber}");
                });
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("RECHNUNG").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().AlignRight().Text($"Nr.: {_model.InvoiceNumber}").FontSize(14).Bold();
                    column.Item().Height(20);
                    column.Item().AlignRight().Text(_model.ClientName).Bold();
                    column.Item().AlignRight().Text(_model.ClientAddress);

                    if (!string.IsNullOrEmpty(_model.ClientTaxNumber))
                        column.Item().AlignRight().Text($"Steuernummer: {_model.ClientTaxNumber}");

                    if (!string.IsNullOrEmpty(_model.ClientEuTaxNumber))
                        column.Item().AlignRight().Text($"UID-Nr.: {_model.ClientEuTaxNumber}");

                    column.Item().Height(15);
                    column.Item().AlignRight().Text($"Datum: {_model.IssueDate:yyyy.MM.dd}");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Item().Element(ComposeTable);

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        foreach (var group in _model.Items.GroupBy(x => x.VatPercent))
                        {
                            c.Item().Text($"1 {group.Key:N0} %");
                            c.Item().Text($"{group.Sum(x => x.VatAmount):N1} €");
                        }
                    });
                    row.RelativeItem().PaddingVertical(5);
                });

                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().Column(c => { c.Item().Text($"Zahlung: {_model.PaymentMethod}"); });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("Netto:"); r.RelativeItem().AlignRight().Text($"{_model.TotalNet:N1} €"); });
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("MwSt:"); r.RelativeItem().AlignRight().Text($"{_model.TotalVat:N1} €"); });
                        c.Item().PaddingTop(5).Row(r => { r.RelativeItem().AlignRight().Text("Gesamt:").FontSize(14).Bold(); r.RelativeItem().AlignRight().Text($"{_model.TotalGross:N1} €").FontSize(14).Bold(); });
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
                });

                table.Header(header => {
                    header.Cell().Element(CellStyle).Text("#").Bold();
                    header.Cell().Element(CellStyle).Text("Bezeichnung").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Menge").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Brutto (€)").Bold();
                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten1);
                });

                foreach (var item in _model.Items.Select((x, i) => new { Item = x, Index = i + 1 }))
                {
                    table.Cell().Element(CellStyle).Text(item.Index.ToString());

                    table.Cell().Element(CellStyle).Column(c => {
                        c.Item().Text(item.Item.Name).Bold();
                        if (!string.IsNullOrEmpty(item.Item.Description))
                        {
                            c.Item().Text(item.Item.Description).FontSize(9).Italic();
                        }
                    });

                    // JAVÍTVA: N2 formátum a tizedes mennyiséghez (pl. 1,5 óra)
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.Quantity:N2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.GrossTotal:N2} €").Bold();
                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void ComposeFooter(IContainer container) { container.AlignCenter().Text(text => { text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); }); }
    }
}