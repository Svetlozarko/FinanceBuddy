using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceCalc.Data;
using FinanceCalc.Models;

namespace FinanceCalc
{
    public class SavingGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SavingGoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SavingGoals
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SavingGoals.Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SavingGoals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savingGoal = await _context.SavingGoals
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (savingGoal == null)
            {
                return NotFound();
            }

            return View(savingGoal);
        }

        // GET: SavingGoals/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: SavingGoals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,TargetAmount,CurrentAmount,Purpose,IsCompleted,CreatedAt")] SavingGoal savingGoal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(savingGoal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", savingGoal.UserId);
            return View(savingGoal);
        }

        // GET: SavingGoals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savingGoal = await _context.SavingGoals.FindAsync(id);
            if (savingGoal == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", savingGoal.UserId);
            return View(savingGoal);
        }

        // POST: SavingGoals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TargetAmount,CurrentAmount,Purpose,IsCompleted,CreatedAt")] SavingGoal savingGoal)
        {
            if (id != savingGoal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(savingGoal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SavingGoalExists(savingGoal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", savingGoal.UserId);
            return View(savingGoal);
        }

        // GET: SavingGoals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var savingGoal = await _context.SavingGoals
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (savingGoal == null)
            {
                return NotFound();
            }

            return View(savingGoal);
        }

        // POST: SavingGoals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var savingGoal = await _context.SavingGoals.FindAsync(id);
            if (savingGoal != null)
            {
                _context.SavingGoals.Remove(savingGoal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SavingGoalExists(int id)
        {
            return _context.SavingGoals.Any(e => e.Id == id);
        }
    }
}
