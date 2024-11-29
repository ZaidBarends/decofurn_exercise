using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace decofurn_exercise
{
    internal class InvoiceLine
    {
        public required int LineId { get; set; } // Primary Key
        public required string InvoiceNumber { get; set; } // Foreign key referencing InvoiceHeader
        public string? Description { get; set; } // Line item description
        public double? Quantity { get; set; } // Quantity of the item
        public double? UnitSellingPriceExVAT { get; set; } // Price excluding VAT
    }
}
