using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ecoinv.Pdf.Models;

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
                page.Size(PageSizes.A4);
                page.Margin(30, Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    // ======================
                    // FEJLÉC
                    // ======================
                    col.Item().Text($"Számla: {_model.InvoiceNumber}")
                        .FontSize(16)
                        .Bold();

                    col.Item().Text($"Kelt: {_model.IssueDate:yyyy.MM.dd}");
                    col.Item().Text($"Fizetési határidő: {_model.DueDate:yyyy.MM.dd}");

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // ======================
                    // ELADÓ / VEVŐ (Itt marad a RelativeItem, mert ez Row!)
                    // ======================
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Eladó").Bold();
                            c.Item().Text(_model.SellerName);
                            c.Item().Text(_model.SellerAddress);
                            c.Item().Text($"Adószám: {_model.SellerTaxNumber}");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Vevő").Bold();
                            c.Item().Text(_model.ClientName);
                            c.Item().Text(_model.ClientAddress);
                            c.Item().Text($"Adószám: {_model.ClientTaxNumber}");
                        });
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // ======================
                    // TÉTELEK (Itt visszaállítottam RelativeColumn-ra, mert ez Table!)
                    // ======================
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            // JAVÍTVA: Table esetén RelativeColumn kell!
                            columns.RelativeColumn(4);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Megnevezés").Bold();
                            header.Cell().AlignRight().Text("Menny.");
                            header.Cell().AlignRight().Text("Nettó");
                            header.Cell().AlignRight().Text("Bruttó");
                            header.Cell().Element(x => x.PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Black));
                        });

                        foreach (var item in _model.Items)
                        {
                            table.Cell().PaddingVertical(2).Text(item.Description);
                            table.Cell().PaddingVertical(2).AlignRight().Text(item.Quantity.ToString("0.##"));
                            table.Cell().PaddingVertical(2).AlignRight().Text(item.NetTotal.ToString("N0"));
                            table.Cell().PaddingVertical(2).AlignRight().Text(item.GrossTotal.ToString("N0"));
                        }
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // ======================
                    // ÖSSZESÍTÉS
                    // ======================
                    col.Item().AlignRight().Column(c =>
                    {
                        c.Item().Text($"Nettó összesen: {_model.TotalNet:N0} Ft");
                        c.Item().Text($"ÁFA összesen: {_model.TotalVat:N0} Ft");
                        c.Item().Text($"Bruttó összesen: {_model.TotalGross:N0} Ft")
                            .Bold()
                            .FontSize(12);
                    });
                });
            });
        }
    }
}