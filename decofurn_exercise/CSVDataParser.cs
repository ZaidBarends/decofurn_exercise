using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace decofurn_exercise
{
    internal static class CSVDataParser
    {
        public static void ImportCsvToDatabase(string csvFilePath, string connectionString)
        {
            try
            {
                Console.WriteLine($"Starting import of CSV file: {csvFilePath}");

                var options = CreateDbContextOptions(connectionString);
                using var context = new InvoiceContext(options);

                var invoiceHeaders = MapInvoiceHeaderData(csvFilePath);
                Console.WriteLine($"InvoiceHeader data mapped successfully. Rows read: {invoiceHeaders.Count}");
                BulkInsertInvoiceHeaders(invoiceHeaders, context);

                var invoiceLines = MapInvoiceLinesData(csvFilePath);
                Console.WriteLine($"InvoiceLines data mapped successfully. Rows read: {invoiceLines.Count}");
                BulkInsertInvoiceLines(invoiceLines, context);

                PrintInvoiceQuantities(invoiceLines);

                Console.WriteLine("=== CSV Import Process Completed Successfully ===");

                CheckInvoiceTotalsBalance(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during import: {ex.Message}");
            }
        }

        private static DbContextOptions<InvoiceContext> CreateDbContextOptions(string connectionString)
        {
            return new DbContextOptionsBuilder<InvoiceContext>()
                .UseSqlServer(connectionString)
                .Options;
        }

        private static List<InvoiceHeader> MapInvoiceHeaderData(string csvFilePath)
        {
            var uniqueInvoiceNumbers = new HashSet<string>();
            var invoiceHeaders = new List<InvoiceHeader>();

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var invoiceNumber = csv.GetField("Invoice Number");
                if (!uniqueInvoiceNumbers.Contains(invoiceNumber))
                {
                    uniqueInvoiceNumbers.Add(invoiceNumber);
                    invoiceHeaders.Add(CreateInvoiceHeader(csv, invoiceNumber));
                }
            }

            return invoiceHeaders;
        }

        private static InvoiceHeader CreateInvoiceHeader(CsvReader csv, string invoiceNumber)
        {
            return new InvoiceHeader
            {
                InvoiceNumber = invoiceNumber,
                InvoiceDate = ParseDate(csv.GetField("Invoice Date")),
                Address = csv.GetField("Address"),
                InvoiceTotal = ParseDouble(csv.GetField("Invoice Total Ex VAT"))
            };
        }

        private static DateTime ParseDate(string dateStr)
        {
            return DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : default;
        }

        private static double ParseDouble(string valueStr)
        {
            return double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0.0;
        }

        private static List<InvoiceLine> MapInvoiceLinesData(string csvFilePath)
        {
            var invoiceLines = new List<InvoiceLine>();

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                invoiceLines.Add(CreateInvoiceLine(csv));
            }

            return invoiceLines;
        }

        private static InvoiceLine CreateInvoiceLine(CsvReader csv)
        {
            return new InvoiceLine
            {
                InvoiceNumber = csv.GetField("Invoice Number"),
                Description = csv.GetField("Line description"),
                Quantity = ParseInt(csv.GetField("Invoice Quantity")),
                UnitSellingPriceExVAT = ParseDouble(csv.GetField("Unit selling price ex VAT"))
            };
        }

        private static int ParseInt(string valueStr)
        {
            return int.TryParse(valueStr, out var value) ? value : 0;
        }

        private static void BulkInsertInvoiceHeaders(List<InvoiceHeader> invoiceHeaders, InvoiceContext context)
        {
            context.InvoiceHeaders.RemoveRange(context.InvoiceHeaders);
            context.InvoiceHeaders.AddRange(invoiceHeaders);
            context.SaveChanges();
            Console.WriteLine("InvoiceHeader data successfully inserted.");
        }

        private static void BulkInsertInvoiceLines(List<InvoiceLine> invoiceLines, InvoiceContext context)
        {
            context.InvoiceLines.RemoveRange(context.InvoiceLines);
            context.InvoiceLines.AddRange(invoiceLines);
            context.SaveChanges();
            Console.WriteLine("InvoiceLines data successfully inserted.");
        }

        private static void PrintInvoiceQuantities(List<InvoiceLine> invoiceLines)
        {
            var invoiceTotals = invoiceLines
                .GroupBy(line => line.InvoiceNumber)
                .Select(group => new
                {
                    InvoiceNumber = group.Key,
                    TotalQuantity = group.Sum(line => line.Quantity)
                }).ToList();

            invoiceTotals.ForEach(total => Console.WriteLine($"Invoice Number: {total.InvoiceNumber}, Total Quantity: {total.TotalQuantity}"));
        }

        private static void CheckInvoiceTotalsBalance(InvoiceContext context)
        {
            try
            {
                double invoiceHeaderTotalSum = CalculateInvoiceHeaderTotalSum(context);
                double invoiceLineTotalSum = CalculateInvoiceLineTotalSum(context);

                Console.WriteLine($"Sum of all InvoiceHeader totals: {invoiceHeaderTotalSum:F2}");
                Console.WriteLine($"Sum of all InvoiceLine totals: {invoiceLineTotalSum:F2}");

                if (Math.Abs(invoiceHeaderTotalSum - invoiceLineTotalSum) > 0.01)
                {
                    Console.WriteLine("Mismatch! The totals do not balance.");
                    Console.WriteLine($"Difference: {Math.Abs(invoiceHeaderTotalSum - invoiceLineTotalSum):F2}");
                }
                else
                {
                    Console.WriteLine("The totals balance.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during invoice totals check: {ex.Message}");
            }
        }

        private static double CalculateInvoiceHeaderTotalSum(InvoiceContext context)
        {
            return context.InvoiceHeaders.Sum(header => header.InvoiceTotal);
        }

        private static double CalculateInvoiceLineTotalSum(InvoiceContext context)
        {
            return context.InvoiceLines
                .AsEnumerable()
                .GroupBy(line => line.InvoiceNumber)
                .Sum(group => group.Sum(line => line.Quantity * line.UnitSellingPriceExVAT));
        }
    }
}
