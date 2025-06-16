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
    public class InboxMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InboxMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InboxMessages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.InboxMessage.Include(i => i.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: InboxMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inboxMessage = await _context.InboxMessage
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inboxMessage == null)
            {
                return NotFound();
            }

            return View(inboxMessage);
        }

        // GET: InboxMessages/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: InboxMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Message,IsRead,CreatedAt")] InboxMessage inboxMessage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inboxMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", inboxMessage.UserId);
            return View(inboxMessage);
        }

        // GET: InboxMessages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inboxMessage = await _context.InboxMessage.FindAsync(id);
            if (inboxMessage == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", inboxMessage.UserId);
            return View(inboxMessage);
        }

        // POST: InboxMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Message,IsRead,CreatedAt")] InboxMessage inboxMessage)
        {
            if (id != inboxMessage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inboxMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InboxMessageExists(inboxMessage.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", inboxMessage.UserId);
            return View(inboxMessage);
        }

        // GET: InboxMessages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inboxMessage = await _context.InboxMessage
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inboxMessage == null)
            {
                return NotFound();
            }

            return View(inboxMessage);
        }

        // POST: InboxMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inboxMessage = await _context.InboxMessage.FindAsync(id);
            if (inboxMessage != null)
            {
                _context.InboxMessage.Remove(inboxMessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InboxMessageExists(int id)
        {
            return _context.InboxMessage.Any(e => e.Id == id);
        }
    }
}
