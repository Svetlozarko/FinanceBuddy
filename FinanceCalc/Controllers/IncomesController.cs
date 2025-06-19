using FinanceCalc.Data;
using Microsoft.AspNetCore.Mvc;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

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

            income.UserId = userId;
            income.Amount = income.Amount;

            _context.Add(income);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                income.Id,
                income.Amount,
            });
        }

    }
}
