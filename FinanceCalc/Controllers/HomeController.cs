using FinanceCalc.Data;
using FinanceCalc.Enums;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinanceCalc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
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
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Include entire endDate day by adding 1 day and comparing less than
                var nextDay = endDate.Value.AddDays(1);
                query = query.Where(t => t.Date < nextDay);
            }

            var transactions = await query.ToListAsync();

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

            ViewBag.MonthlyIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            ViewBag.Savings = transactions
                .Where(t => t.Type == TransactionType.Saving)
                .Sum(t => t.Amount);

            // Pass the filter dates back to the ViewBag so UI can keep the values
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var viewModel = new DashboardViewModel
            {
                Expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList(),
                Income = transactions.Where(t => t.Type == TransactionType.Income).ToList(),
                Savings = transactions.Where(t => t.Type == TransactionType.Saving).ToList(),
                AllTransactions = transactions
            };

            return View(viewModel);
        }






    }
}
