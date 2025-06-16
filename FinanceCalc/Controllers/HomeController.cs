using FinanceCalc.Data;
using FinanceCalc.Enums;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinanceCalc.Services;

namespace FinanceCalc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InboxMessageService _inboxService;

        public HomeController(ApplicationDbContext context, InboxMessageService inboxService)
        {
            _context = context;
            _inboxService = inboxService;
        }



        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(new DashboardViewModel());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Transaction.Where(t => t.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date < endDate.Value.AddDays(1)); // Inclusive of entire day

            var transactions = await query.ToListAsync();

            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .ToList();

            var income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .ToList();

            var savings = transactions
                .Where(t => t.Type == TransactionType.Saving)
                .ToList();

            // Expense chart (by category)
            var expenseData = expenses
                .GroupBy(t => t.ExpenseCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToList();

            ViewBag.Labels = expenseData.Select(e => e.Category).ToList();
            ViewBag.Data = expenseData.Select(e => e.Total).ToList();

            // Monthly income
            decimal totalIncome = income.Sum(t => t.Amount);
            ViewBag.MonthlyIncome = totalIncome;

            // Total expenses
            decimal totalExpenses = expenses.Sum(t => t.Amount);

            // Income vs Expenses donut chart
            ViewBag.IncomeVsExpenseLabels = new[] { "Expenses", "Remaining Income" };
            ViewBag.IncomeVsExpenseData = new[] { totalExpenses, Math.Max(0, totalIncome - totalExpenses) };

            // Savings donut chart
            ViewBag.Savings = savings.Sum(t => t.Amount);
            var totalSaved = savings.Sum(t => t.Amount);
            await _inboxService.AddCongratsMessageIfEligible(userId, totalSaved);


            // Preserve filters in ViewBag
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var viewModel = new DashboardViewModel
            {
                Expenses = expenses ?? new List<Transaction>(),
                Income = income ?? new List<Transaction>(),
                Savings = savings ?? new List<Transaction>(),
                AllTransactions = transactions ?? new List<Transaction>()
            };

            return View(viewModel);
        }
    }
}
