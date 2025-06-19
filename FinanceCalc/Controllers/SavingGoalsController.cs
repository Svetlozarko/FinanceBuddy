using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceCalc.Data;
using FinanceCalc.Models;

namespace FinanceCalc.Controllers
{
    public class SavingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SavingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SavingGoals
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Savings.Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
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
    }
}
