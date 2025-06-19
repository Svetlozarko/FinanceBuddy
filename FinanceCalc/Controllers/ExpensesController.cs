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
        public class ExpensesController : Controller
        {
            private readonly ApplicationDbContext _context;

            public ExpensesController(ApplicationDbContext context)
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
                return View(new List<Expenses>());
            }

            // Fetch expenses only for this user
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .ToListAsync();

            // Group expenses by category
            var expenseData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .ToList();

            ViewBag.Labels = expenseData.Select(d => d.Category).ToList();
            ViewBag.Data = expenseData.Select(d => d.Total).ToList();

            return View(expenses);
        }



        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(Guid? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Expenses
                    .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }

        // GET: Transactions/Create




        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddExpense([FromBody] Expenses transaction)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = "Invalid model", details = ModelState });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            transaction.UserId = userId;

            try
            {
                _context.Expenses.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Category = transaction.Category ?? "Uncategorized",
                    Date = transaction.Date.ToString("yyyy-MM-dd"),
                    Description = transaction.Description ?? ""
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Something went wrong",
                    details = ex.Message
                });
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddIncome([FromBody] Income income)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var newIncome = new Income
            {
                UserId = userId,
                Amount = income.Amount,
            };

            _context.Income.Add(newIncome);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                newIncome.Id,
                newIncome.Amount,
            });
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var transaction = await _context.Expenses
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

                var transaction = await _context.Expenses
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction != null)
                {
                    _context.Expenses.Remove(transaction);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            private bool TransactionExists(Guid id)
            {
                // Check transaction existence without user filter (optional)
                return _context.Expenses.Any(e => e.Id == id);
            }


        


    }
}
