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

        public InvoicePdfDocument(InvoicePdfModel model) { _model = model; }

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
                    bool logoFound = false;
                    try
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("econtologo.jpg", StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(resourceName))
                        {
                            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                            {
                                if (stream != null) { column.Item().Width(150).Image(stream); logoFound = true; }
                            }
                        }
                    }
                    catch { }
                    if (!logoFound) column.Item().Text("ECONT").Style(titleStyle);
                    column.Item().Height(10);
                    column.Item().Text(_model.SellerName).Bold();
                    column.Item().Text(_model.SellerAddress);
                    column.Item().Text($"Steuernummer: {_model.SellerTaxNumber}");
                });
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("RECHNUNG").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().AlignRight().Text($"Nr.: {_model.InvoiceNumber}").FontSize(14).Bold();
                    column.Item().Height(20);
                    column.Item().AlignRight().Text(_model.ClientName).Bold();
                    column.Item().AlignRight().Text(_model.ClientAddress);
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
                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().Column(c => { c.Item().Text($"Zahlung: {_model.PaymentMethod}"); });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("Netto:"); r.RelativeItem().AlignRight().Text($"{_model.TotalNet:N2} €"); });
                        c.Item().Row(r => { r.RelativeItem().AlignRight().Text("MwSt:"); r.RelativeItem().AlignRight().Text($"{_model.TotalVat:N2} €"); });
                        c.Item().PaddingTop(5).Row(r => { r.RelativeItem().AlignRight().Text("Gesamt:").FontSize(14).Bold(); r.RelativeItem().AlignRight().Text($"{_model.TotalGross:N2} €").FontSize(14).Bold(); });
                    });
                });
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns => { columns.ConstantColumn(25); columns.RelativeColumn(3); columns.RelativeColumn(1); columns.RelativeColumn(1.2f); columns.RelativeColumn(1.2f); columns.RelativeColumn(1.2f); columns.RelativeColumn(1.2f); });
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
                    table.Cell().Element(CellStyle).Text(item.Item.Name).Bold();
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.Quantity:N0}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Item.GrossTotal:N2} €").Bold();
                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void ComposeFooter(IContainer container) { container.AlignCenter().Text(text => { text.CurrentPageNumber(); text.Span(" / "); text.TotalPages(); }); }
    }
}