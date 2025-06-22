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

            // Base queries
            var expensesQuery = _context.Expenses.Where(e => e.UserId == userId);
            var incomeQuery = _context.Income.Where(i => i.UserId == userId);
            var savingsQuery = _context.Savings.Where(s => s.UserId == userId);

            // Optional filters
            if (startDate.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.AddDays(1);
                expensesQuery = expensesQuery.Where(e => e.Date < inclusiveEnd);
            }

            // Execute queries
            var expenses = await expensesQuery.ToListAsync();
            var income = await incomeQuery.ToListAsync();
            var savings = await savingsQuery.ToListAsync();

            // Expense chart
            var expenseData = expenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                }).ToList();

            ViewBag.Labels = expenseData.Select(e => e.Category).ToList();
            ViewBag.Data = expenseData.Select(e => e.Total).ToList();

            decimal totalIncome = income.Sum(i => i.Amount);
            decimal totalExpenses = expenses.Sum(e => e.Amount);

            ViewBag.MonthlyIncome = totalIncome;
            ViewBag.IncomeVsExpenseLabels = new[] { "Expenses", "Remaining Income" };
            ViewBag.IncomeVsExpenseData = new[] { totalExpenses, Math.Max(0, totalIncome - totalExpenses) };

            ViewBag.Savings = savings.Sum(s => s.CurrentAmount);
            var userSavings = savings.FirstOrDefault(); // assuming 1 savings record per user
            ViewBag.CurrentSavings = userSavings?.CurrentAmount ?? 0;
            ViewBag.SavingsGoal = userSavings?.TargetAmount ?? 0;
            await _inboxService.AddCongratsMessageIfEligible(userId, ViewBag.Savings);

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var viewModel = new DashboardViewModel
            {
                Expenses = expenses,
                Income = income,
                Savings = savings,
     //           AllTransactions = expenses.Concat<Expenses>(income).Concat(savings).ToList() // if needed
            };

            return View(viewModel);
        }

    }
}
