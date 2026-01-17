using System;
using System.Collections.Generic;

namespace Ecoinv.Pdf.Models
{
    public class InvoicePdfModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        // --- Eladó (Ausztria) ---
        public string SellerName { get; set; }
        public string SellerAddress { get; set; }

        // Ausztriában kétféle adószámot szokás feltüntetni:
        public string SellerTaxNumber { get; set; }   // Belföldi adószám (Steuernummer)
        public string SellerEuTaxNumber { get; set; } // Közösségi adószám (UID-Nummer) - FONTOS!

        public string SellerBankAccount { get; set; } // IBAN / Bankszámla

        // --- Vevő ---
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientTaxNumber { get; set; }   // Steuernummer vagy UID

        public List<InvoicePdfItem> Items { get; set; } = new();

        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }

        public string PaymentMethod { get; set; }
        public string Comment { get; set; }
    }

    public class InvoicePdfItem
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal NetUnitPrice { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }
    }
}