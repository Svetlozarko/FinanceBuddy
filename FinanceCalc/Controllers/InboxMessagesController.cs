using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceCalc.Data;
using FinanceCalc.Models;

namespace FinanceCalc.Controllers
{
    [Route("Inbox")]
    public class InboxMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InboxMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Inbox
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var messages = _context.InboxMessage.Include(i => i.User);
            return View(await messages.ToListAsync());
        }

        // GET: /Inbox/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inboxMessage = await _context.InboxMessage
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inboxMessage == null) return NotFound();

            return View(inboxMessage);
        }

        
        }
    }

