using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Pdf.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecoinv.Pdf.Services
{
    public class InvoicePdfService
    {
        public InvoicePdfModel BuildInvoicePdfModel(
            INVOICE_HEADERS header,
            IEnumerable<INVOICE_DETAILS> details,
            CLIENTS client,
            ADRESSES clientAddress,
            ECSYS seller,
            IEnumerable<SERVICES> allServices,
            string manualFooterNote)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var model = new InvoicePdfModel
            {
                InvoiceNumber = header.INVOICE_NUMBER,
                IssueDate = header.ISSUE_DATE ?? DateTime.Now,
                DueDate = header.DUE_DATE ?? DateTime.Now,

                SellerName = seller?.SZKNEV ?? "Unbekannt",
                SellerAddress = seller?.SZKCIM ?? string.Empty,
                SellerTaxNumber = seller?.SZKTAX ?? string.Empty,
                SellerEuTaxNumber = seller?.SZKCOMTAX ?? string.Empty,
                SellerBankAccount = seller?.SZKBANKACCOUNT ?? string.Empty,
                SellerIBAN = seller?.IBAN ?? string.Empty,
                SellerBIC = seller?.BIC ?? string.Empty,

                ClientName = client?.NAME ?? string.Empty,
                ClientTaxNumber = client?.TAX_NUMBER ?? string.Empty,
                ClientEuTaxNumber = client?.COMTAX_NUMBER ?? string.Empty,

                ClientAddress = clientAddress != null
                    ? $"{clientAddress.POSTALCODE} {clientAddress.CITY}, {clientAddress.ADDRESS}"
                    : string.Empty,

                PaymentMethod = TranslatePaymentMethod(header.PAYMENT_METHOD),
                Comment = manualFooterNote ?? string.Empty
            };

            foreach (var d in details)
            {
                var service = allServices?.FirstOrDefault(s => s.ID == d.SERVICES_ID);

                string finalName = d.SERVICE_NAME;
                string finalDescription = service != null ? service.DESCRIPTION : "";

                if (!string.IsNullOrEmpty(d.SERVICE_NAME) && d.SERVICE_NAME.Contains("/"))
                {
                    var parts = d.SERVICE_NAME.Split('/');
                    finalName = parts[0].Trim();
                    if (parts.Length > 1)
                    {
                        finalDescription = string.Join("/", parts.Skip(1)).Trim();
                    }
                }

                model.Items.Add(new InvoicePdfItem
                {
                    Name = finalName,
                    Description = finalDescription,
                    Quantity = d.QTY,
                    NetUnitPrice = Math.Round(d.NET_UNIT_PRICE, 2),
                    NetTotal = Math.Round(d.LINE_TOTAL_NET, 2),
                    VatPercent = Math.Round(d.VAT_PERCENT, 2),
                    VatAmount = Math.Round(d.VAT_AMOUNT, 2),
                    GrossTotal = Math.Round(d.LINE_TOTAL_GROSS, 2)
                });
            }

            // TECH LEAD JAVÍTÁS: Kerekített összesítés az Euro bizonylathoz
            model.TotalNet = Math.Round(model.Items.Sum(x => x.NetTotal), 2);
            model.TotalVat = Math.Round(model.Items.Sum(x => x.VatAmount), 2);
            model.TotalGross = Math.Round(model.Items.Sum(x => x.GrossTotal), 2);

            return model;
        }

        private string TranslatePaymentMethod(string hungarianMethod)
        {
            if (string.IsNullOrWhiteSpace(hungarianMethod)) return "Barzahlung";
            var lower = hungarianMethod.ToLower().Trim();

            if (lower.Contains("kártya") || lower.Contains("card")) return "Kartenzahlung";
            if (lower.Contains("átutalás") || lower.Contains("bank")) return "Überweisung";
            if (lower.Contains("készpénz") || lower.Contains("kp")) return "Barzahlung";

            return hungarianMethod;
        }
    }
}