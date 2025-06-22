using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using FinanceCalc.Data;

namespace FinanceCalc.Controllers
{
    public class ImportExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportExportController(ApplicationDbContext context)
        {
            _context = context;
        }
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

            // ✅ Set license context here
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
    }
}
