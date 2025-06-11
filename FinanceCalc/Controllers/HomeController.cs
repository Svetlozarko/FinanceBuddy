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


        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Return empty list if user not logged in
                return View(new List<Transaction>());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var transactions = await _context.Transaction
                .Where(t => t.UserId == userId)
                .ToListAsync();

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



    }
}
