using System;
using System.Collections.Generic;

namespace Ecoinv.Pdf.Models
{
    public class InvoicePdfModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }

        public bool IsStorno { get; set; }
        public string OriginalInvoiceNumber { get; set; }

        // --- Eladó ---
        public string SellerName { get; set; }
        public string SellerAddress { get; set; }
        public string SellerTaxNumber { get; set; }
        public string SellerEuTaxNumber { get; set; }
        public string SellerBankAccount { get; set; } // Régi mező, megtarthatjuk

        // --- ÚJ MEZŐK ---
        public string SellerIBAN { get; set; }
        public string SellerBIC { get; set; }

        // --- Vevő ---
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientTaxNumber { get; set; }

        public List<InvoicePdfItem> Items { get; set; } = new List<InvoicePdfItem>();

        public decimal TotalNet { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TotalGross { get; set; }

        public string PaymentMethod { get; set; }
        public string Comment { get; set; }
    }

    public class InvoicePdfItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal NetUnitPrice { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }
    }
}