using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace decofurn_exercise
{
    internal class InvoiceHeader
    {
        public required int InvoiceId { get; set; } // Primary Key
        public required string InvoiceNumber { get; set; } // Unique Invoice Number
        public DateTime? InvoiceDate { get; set; } // Date of the Invoice
        public string? Address { get; set; } // Address associated with the invoice
        public double? InvoiceTotal { get; set; } // Total amount for the invoice
    }
}
