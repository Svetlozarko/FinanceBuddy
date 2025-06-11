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
        }
    }
