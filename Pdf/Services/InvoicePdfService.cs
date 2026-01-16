using Ecoinv.BL;
using Ecoinv.BL.Enums;
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
            ADRESSES clientAddress)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var model = new InvoicePdfModel
            {
                // Nullable DateTime kezelése
                InvoiceNumber = header.INVOICE_NUMBER,
                IssueDate = header.ISSUE_DATE ?? DateTime.Now,
                DueDate = header.DUE_DATE ?? DateTime.Now,

                // Fix adatok (később konfigból jöhet)
                SellerName = "ZsolaSoft Kft.",
                SellerAddress = "3100 Salgótarján, Fő út 1.",
                SellerTaxNumber = "12345678-1-12",
                SellerBankAccount = "11700000-00000000",

                // Vevő adatok
                ClientName = client?.NAME ?? string.Empty,
                ClientTaxNumber = client?.TAX_NUMBER ?? string.Empty,

                // Cím összerakása (Ellenőrizd, hogy az ADRESSES táblában ADDRESS vagy STREET a mező neve!)
                ClientAddress = clientAddress != null
                    ? $"{clientAddress.POSTALCODE} {clientAddress.CITY}, {clientAddress.ADDRESS}"
                    : string.Empty,

                PaymentMethod = header.PAYMENT_METHOD,
                Comment = string.Empty
            };

            // Tételek feldolgozása
            foreach (var d in details)
            {
                model.Items.Add(new InvoicePdfItem
                {
                    Description = d.SERVICE_NAME,
                    Quantity = d.QTY,
                    NetUnitPrice = d.NET_UNIT_PRICE,
                    NetTotal = d.LINE_TOTAL_NET,
                    VatPercent = d.VAT_PERCENT,
                    VatAmount = d.VAT_AMOUNT,
                    GrossTotal = d.LINE_TOTAL_GROSS
                });
            }

            // Összesítés
            model.TotalNet = model.Items.Sum(x => x.NetTotal);
            model.TotalVat = model.Items.Sum(x => x.VatAmount);
            model.TotalGross = model.Items.Sum(x => x.GrossTotal);

            return model;
        }
    }
}