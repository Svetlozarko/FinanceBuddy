    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;  // added for UserId claim access
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using FinanceCalc.Data;
    using FinanceCalc.Models;
    using FinanceCalc.Enums;    
using Microsoft.AspNetCore.Authorization;
using System.Text;
using NuGet.Packaging;
using iTextSharp.text;
using iTextSharp.text.pdf;






namespace FinanceCalc.Controllers
    {
        public class TransactionsController : Controller
        {
            private readonly ApplicationDbContext _context;

            public TransactionsController(ApplicationDbContext context)
            {
                _context = context;
            }

            // GET: Transactions
            public async Task<IActionResult> Index()
            {
                // Get current user id
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If user not logged in, show empty list
                if (userId == null)
                {
                    return View(new List<Transaction>());
                }

                // Fetch transactions only for this user
                var transactions = await _context.Transaction
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                // Group only Expense type transactions
                var expenseData = transactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .GroupBy(t => t.ExpenseCategory)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Sum(t => t.Amount)
                    })
                    .ToList();

                ViewBag.Labels = expenseData.Select(d => d.Category).ToList();
                ViewBag.Data = expenseData.Select(d => d.Total).ToList();

                return View(transactions);
            }


            // GET: Transactions/Details/5
            public async Task<IActionResult> Details(Guid? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Transaction
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }

            // GET: Transactions/Create
            public IActionResult Create()
            {
                PopulateDropdowns();
                return View();
            }

            private void PopulateDropdowns()
            {
                ViewBag.TransactionTypes = Enum.GetValues(typeof(TransactionType))
                    .Cast<TransactionType>()
                    .Select(tt => new SelectListItem
                    {
                        Value = tt.ToString(),
                        Text = tt.ToString()
                    }).ToList();

                ViewBag.ExpenseCategories = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Food", Text = "Food" },
                    new SelectListItem { Value = "Trips", Text = "Trips" },
                    new SelectListItem { Value = "Going Out", Text = "Going Out" },
                    new SelectListItem { Value = "Shopping", Text = "Shopping" },
                    new SelectListItem { Value = "Bills", Text = "Bills" }
                };
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create([Bind("Id,Date,Amount,ExpenseCategory,Type")] Transaction transaction)
            {
                if (ModelState.IsValid)
                {
                    // Assign new Guid and current userId here
                    transaction.Id = Guid.NewGuid();
                    transaction.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _context.Add(transaction);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }

                // Re-populate dropdowns if model invalid
                PopulateDropdowns();

                return View(transaction);
            }

            // GET: Transactions/Edit/5
            public async Task<IActionResult> Edit(Guid? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Transaction
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null)
                {
                    return NotFound();
                }

                PopulateDropdowns(); // 👈 Important
                return View(transaction);
            }

            // POST: Transactions/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(Guid id, [Bind("Id,Date,Amount,ExpenseCategory,Type")] Transaction transaction)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (id != transaction.Id)
                {
                    return NotFound();
                }

                // Make sure the transaction belongs to the user
                var transactionFromDb = await _context.Transaction
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transactionFromDb == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Preserve UserId on update
                        transaction.UserId = userId;

                        _context.Update(transaction);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransactionExists(transaction.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                PopulateDropdowns(); // 👈 Re-populate dropdowns if model invalid
                return View(transaction);
            }


            // GET: Transactions/Delete/5
            public async Task<IActionResult> Delete(Guid? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Transaction
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }

            // POST: Transactions/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(Guid id)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Transaction
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction != null)
                {
                    _context.Transaction.Remove(transaction);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            private bool TransactionExists(Guid id)
            {
                // Check transaction existence without user filter (optional)
                return _context.Transaction.Any(e => e.Id == id);
            }


        [Authorize]
        public IActionResult ExportToPdf()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = _context.Transaction
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
                    AddCell(table, t.Type.ToString());
                    AddCell(table, t.ExpenseCategory ?? "N/A");
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
        var transactions = _context.Transaction
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToList();

        // ✅ Set license context here
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Transactions");

        worksheet.Cells["A1"].Value = "Дата";
        worksheet.Cells["B1"].Value = "Сума";
        worksheet.Cells["C1"].Value = "Тип";
        worksheet.Cells["D1"].Value = "Категория";

        int row = 2;
        foreach (var t in transactions)
        {
            worksheet.Cells[row, 1].Value = t.Date.ToString("dd.MM.yyyy");
            worksheet.Cells[row, 2].Value = t.Amount;
            worksheet.Cells[row, 3].Value = t.Type.ToString();
            worksheet.Cells[row, 4].Value = t.ExpenseCategory ?? "-";
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
            var transactions = _context.Transaction
                .Where(t => t.UserId == userId)
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Date,Amount,Type,Category");

            foreach (var t in transactions)
            {
                builder.AppendLine($"{t.Date:yyyy-MM-dd},{t.Amount},{t.Type},{t.ExpenseCategory ?? "N/A"}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", "transactions.csv");
        }


    }
}
