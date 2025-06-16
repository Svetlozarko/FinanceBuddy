using FinanceCalc.Data;
using FinanceCalc.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceCalc.Services
{
    public class InboxMessageService
    {
        private readonly ApplicationDbContext _context;

        public InboxMessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCongratsMessageIfEligible(string userId, decimal totalSaved)
        {
            if (totalSaved >= 5000)
            {
                bool alreadySent = await _context.InboxMessage
                    .AnyAsync(m => m.UserId == userId && m.Message.Contains("Честито"));

                if (!alreadySent)
                {
                    var message = new InboxMessage
                    {
                        UserId = userId,
                        Message = "Честито, спестихте достатъчно за Голф 5!",
                        ImageUrl = "/images/golf5.jpg",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.InboxMessage.Add(message);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}