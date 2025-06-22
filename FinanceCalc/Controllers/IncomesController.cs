using FinanceCalc.Data;
using Microsoft.AspNetCore.Mvc;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalc.Controllers
{
    public class IncomesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomesController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddIncome([FromBody] Income income)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Assign userId to income object
            income.UserId = userId;

            // Try to find an existing income record for this user
            var existingIncome = await _context.Income
                .FirstOrDefaultAsync(i => i.UserId == userId);

            if (existingIncome != null)
            {
                // Update the existing income amount
                existingIncome.Amount = income.Amount;

                // Mark the entity as modified (optional if tracked)
                _context.Income.Update(existingIncome);
            }
            else
            {
                // No existing record, add new
                _context.Income.Add(income);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                incomeId = existingIncome?.Id ?? income.Id,
                amount = existingIncome?.Amount ?? income.Amount,
            });
        }


    }
}
