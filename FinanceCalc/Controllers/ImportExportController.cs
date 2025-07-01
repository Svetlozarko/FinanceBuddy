using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using FinanceCalc.Data;
using System.Globalization;

namespace FinanceCalc.Controllers
{
    public class ImportExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Your existing export methods remain the same...
        [Authorize]
        public IActionResult ExportToPdf()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = _context.Expenses
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToList();

            using (var stream = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(doc, stream);
                writer.CloseStream = false;
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                doc.Add(new Paragraph("Transaction History", titleFont));
                doc.Add(new Paragraph("\n"));

                // Table
                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f });

                // Headers
                AddCell(table, "Date", true);
                AddCell(table, "Amount", true);
                AddCell(table, "Type", true);
                AddCell(table, "Category", true);

                // Rows
                foreach (var t in transactions)
                {
                    AddCell(table, t.Date.ToString("yyyy-MM-dd"));
                    AddCell(table, t.Amount.ToString("C"));
                    AddCell(table, t.Category ?? "N/A");
                }

                doc.Add(table);
                doc.Close();

                stream.Position = 0;
                return File(stream.ToArray(), "application/pdf", "Transactions.pdf");
            }
        }

        private void AddCell(PdfPTable table, string text, bool isHeader = false)
        {
            var font = isHeader
                ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)
                : FontFactory.GetFont(FontFactory.HELVETICA, 12);

            var cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = isHeader ? new BaseColor(220, 220, 220) : BaseColor.WHITE
            };
            table.AddCell(cell);
        }

        public IActionResult ExportToExcel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expenses = _context.Expenses
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToList();

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Expenses");

            worksheet.Cells["A1"].Value = "Дата";
            worksheet.Cells["B1"].Value = "Сума";
            worksheet.Cells["C1"].Value = "Тип";
            worksheet.Cells["D1"].Value = "Категория";

            int row = 2;
            foreach (var t in expenses)
            {
                worksheet.Cells[row, 1].Value = t.Date.ToString("dd.MM.yyyy");
                worksheet.Cells[row, 2].Value = t.Amount;
                worksheet.Cells[row, 4].Value = t.Category ?? "-";
                row++;
            }

            worksheet.Cells[1, 1, row - 1, 4].AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "transactions.xlsx");
        }

        public IActionResult ExportToCsv()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = _context.Expenses
                .Where(t => t.UserId == userId)
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Date,Amount,Type,Category");

            foreach (var t in transactions)
            {
                builder.AppendLine($"{t.Date:yyyy-MM-dd},{t.Amount},{t.Category ?? "N/A"}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", "transactions.csv");
        }

        // NEW IMPORT METHOD FOR AJAX
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ImportFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select a file to import." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = new List<dynamic>();
            var errors = new List<string>();

            try
            {
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();

                switch (extension)
                {
                    case ".csv":
                        transactions = await ImportFromCsv(file, userId, errors);
                        break;
                    case ".xlsx":
                    case ".xls":
                        transactions = await ImportFromExcel(file, userId, errors);
                        break;
                    case ".pdf":
                        transactions = await ImportFromPdf(file, userId, errors);
                        break;
                    default:
                        return Json(new { success = false, message = "Unsupported file format. Please use CSV, Excel, or PDF files." });
                }

                if (transactions.Any())
                {
                    // Remove duplicates based on date, amount, and category
                    var existingTransactions = await _context.Expenses
                        .Where(e => e.UserId == userId)
                        .Select(e => new { e.Date, e.Amount, e.Category })
                        .ToListAsync();

                    var newTransactions = transactions
                        .Where(t => !existingTransactions.Any(e =>
                            e.Date.Date == ((DateTime)t.Date).Date &&
                            e.Amount == (decimal)t.Amount &&
                            e.Category == (string)t.Category))
                        .Select(t => new FinanceCalc.Models.Expenses
                        {
                            UserId = userId,
                            Date = (DateTime)t.Date,
                            Amount = (decimal)t.Amount,
                            Category = (string)t.Category,
                        })
                        .ToList();

                    if (newTransactions.Any())
                    {
                        _context.Expenses.AddRange(newTransactions);
                        await _context.SaveChangesAsync();

                        var message = $"Successfully imported {newTransactions.Count} transactions.";
                        if (transactions.Count - newTransactions.Count > 0)
                        {
                            message += $" {transactions.Count - newTransactions.Count} duplicates were skipped.";
                        }

                        return Json(new
                        {
                            success = true,
                            message = message,
                            imported = newTransactions.Count,
                            duplicates = transactions.Count - newTransactions.Count,
                            errors = errors
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No new transactions found. All transactions already exist in the database.",
                            errors = errors
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "No valid transactions found in the file.",
                        errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error importing file: {ex.Message}" });
            }
        }

        private async Task<List<dynamic>> ImportFromCsv(IFormFile file, string userId, List<string> errors)
        {
            var transactions = new List<dynamic>();

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length >= 3)
                    {
                        var transaction = new
                        {
                            Date = ParseDate(values[0]),
                            Amount = ParseAmount(values[1]),
                            Category = values.Length > 3 ? values[3]?.Trim() : values[2]?.Trim()
                        };

                        transactions.Add(transaction);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {i + 1}: {ex.Message}");
                }
            }

            return transactions;
        }

        private async Task<List<dynamic>> ImportFromExcel(IFormFile file, string userId, List<string> errors)
        {
            var transactions = new List<dynamic>();
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new OfficeOpenXml.ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            // Skip header row
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var dateValue = worksheet.Cells[row, 1].Value;
                    var amountValue = worksheet.Cells[row, 2].Value;
                    var categoryValue = worksheet.Cells[row, 4].Value ?? worksheet.Cells[row, 3].Value;

                    if (dateValue != null && amountValue != null)
                    {
                        var transaction = new
                        {
                            Date = ParseExcelDate(dateValue),
                            Amount = ParseExcelAmount(amountValue),
                            Category = categoryValue?.ToString()?.Trim()
                        };

                        transactions.Add(transaction);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            return transactions;
        }

        private async Task<List<dynamic>> ImportFromPdf(IFormFile file, string userId, List<string> errors)
        {
            var transactions = new List<dynamic>();

            using var reader = new PdfReader(file.OpenReadStream());
            var text = new StringBuilder();

            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                text.Append(PdfTextExtractor.GetTextFromPage(reader, page));
            }

            var content = text.ToString();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Pattern to match transaction lines (adjust based on your PDF format)
            var transactionPattern = @"(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{2,4})\s+([+-]?\$?\d+[,.]?\d*)\s+(.+)";
            var regex = new Regex(transactionPattern, RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                try
                {
                    var match = regex.Match(line.Trim());
                    if (match.Success)
                    {
                        var transaction = new
                        {
                            Date = ParseDate(match.Groups[1].Value),
                            Amount = Math.Abs(ParseAmount(match.Groups[2].Value)), // Take absolute value
                            Category = match.Groups[3].Value.Trim()
                        };

                        transactions.Add(transaction);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"PDF parsing error: {ex.Message}");
                }
            }

            return transactions;
        }

        // Helper methods for parsing data
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentField = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString().Trim());
            return result.ToArray();
        }

        private DateTime ParseDate(string dateString)
        {
            dateString = dateString.Trim().Replace("\"", "");

            var formats = new[]
            {
                "yyyy-MM-dd", "dd.MM.yyyy", "MM/dd/yyyy", "dd/MM/yyyy",
                "yyyy/MM/dd", "dd-MM-yyyy", "MM-dd-yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            if (DateTime.TryParse(dateString, out DateTime parsed))
            {
                return parsed;
            }

            throw new FormatException($"Unable to parse date: {dateString}");
        }

        private DateTime ParseExcelDate(object dateValue)
        {
            if (dateValue is DateTime dt)
                return dt;

            if (dateValue is double d)
                return DateTime.FromOADate(d);

            return ParseDate(dateValue.ToString());
        }

        private decimal ParseAmount(string amountString)
        {
            amountString = amountString.Trim().Replace("\"", "").Replace("$", "").Replace("€", "").Replace("лв", "");

            if (decimal.TryParse(amountString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return Math.Abs(result); // Always store as positive amount
            }

            throw new FormatException($"Unable to parse amount: {amountString}");
        }

        private decimal ParseExcelAmount(object amountValue)
        {
            if (amountValue is decimal d)
                return Math.Abs(d);

            if (amountValue is double db)
                return Math.Abs((decimal)db);

            if (amountValue is int i)
                return Math.Abs(i);

            return ParseAmount(amountValue.ToString());
        }
    }
}
