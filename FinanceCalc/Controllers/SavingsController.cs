using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceCalc.Data;
using FinanceCalc.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FinanceCalc.Controllers
{
    public class SavingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SavingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var userSavings = await _context.Savings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            ViewBag.CurrentSavings = userSavings?.CurrentAmount ?? 0;
            ViewBag.SavingsGoal = userSavings?.TargetAmount ?? 0;

            return View(userSavings); // Or pass a view model instead
        }



        // GET: SavingGoals/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }



        private bool SavingGoalExists(int id)
        {
            return _context.Savings.Any(e => e.Id == id);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveOrUpdateSavings([FromBody] Savings savings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Find this user's savings entry
            var existingSavings = await _context.Savings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSavings != null)
            {
                // Update the user's existing row
                existingSavings.CurrentAmount = savings.CurrentAmount;
                existingSavings.TargetAmount = savings.TargetAmount;

                _context.Update(existingSavings);
            }
            else
            {
                // First time this user is saving
                savings.UserId = userId;
                _context.Add(savings);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                savings = new
                {
                    savings.CurrentAmount,
                    savings.TargetAmount
                }
            });
        }

    }
}
