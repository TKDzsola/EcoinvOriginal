using Ecoinv.BL;
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
            ECSYS seller)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var model = new InvoicePdfModel
            {
                // --- SZÁMLA ADATOK ---
                InvoiceNumber = header.INVOICE_NUMBER,
                IssueDate = header.ISSUE_DATE ?? DateTime.Now,
                DueDate = header.DUE_DATE ?? DateTime.Now,

                // --- KIBOCSÁTÓ ADATOK (ECSYS) ---
                SellerName = seller?.SZKNEV ?? "Unbekannt",
                SellerAddress = seller?.SZKCIM ?? string.Empty,

                // Adószámok szétválogatása
                SellerTaxNumber = seller?.SZKTAX ?? string.Empty,       // Steuernummer
                SellerEuTaxNumber = seller?.SZKCOMTAX ?? string.Empty,  // UID-Nummer (Ezt használjuk EU-s adószámnak)
                SellerBankAccount = seller?.SZKBANKACCOUNT ?? string.Empty,

                // --- VEVŐ ADATOK ---
                ClientName = client?.NAME ?? string.Empty,
                ClientTaxNumber = client?.TAX_NUMBER ?? string.Empty,

                ClientAddress = clientAddress != null
                    ? $"{clientAddress.POSTALCODE} {clientAddress.CITY}, {clientAddress.ADDRESS}"
                    : string.Empty,

                // FIZETÉSI MÓD FORDÍTÁSA (Magyar -> Német)
                PaymentMethod = TranslatePaymentMethod(header.PAYMENT_METHOD),

                Comment = string.Empty
            };

            // --- TÉTELEK ---
            foreach (var d in details)
            {
                model.Items.Add(new InvoicePdfItem
                {
                    Description = d.SERVICE_NAME, // A szolgáltatás neve marad, ahogy beírták (vagy ezt is fordítani kell?)
                    Quantity = d.QTY,
                    NetUnitPrice = d.NET_UNIT_PRICE,
                    NetTotal = d.LINE_TOTAL_NET,
                    VatPercent = d.VAT_PERCENT,
                    VatAmount = d.VAT_AMOUNT,
                    GrossTotal = d.LINE_TOTAL_GROSS
                });
            }

            // --- ÖSSZESÍTÉS ---
            model.TotalNet = model.Items.Sum(x => x.NetTotal);
            model.TotalVat = model.Items.Sum(x => x.VatAmount);
            model.TotalGross = model.Items.Sum(x => x.GrossTotal);

            return model;
        }

        // Segédfüggvény a fizetési módok fordítására
        private string TranslatePaymentMethod(string hungarianMethod)
        {
            if (string.IsNullOrWhiteSpace(hungarianMethod)) return string.Empty;

            var lower = hungarianMethod.ToLower().Trim();

            if (lower.Contains("átutalás") || lower.Contains("bank")) return "Überweisung";
            if (lower.Contains("készpénz") || lower.Contains("kp")) return "Barzahlung";
            if (lower.Contains("kártya")) return "Kartenzahlung";
            if (lower.Contains("utánvét")) return "Nachnahme";

            // Ha nem ismerjük fel, visszaadjuk az eredetit
            return hungarianMethod;
        }
    }
}