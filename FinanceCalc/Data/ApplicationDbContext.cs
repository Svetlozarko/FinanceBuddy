using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinanceCalc.Models;

namespace FinanceCalc.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<FinanceCalc.Models.Expenses> Expenses { get; set; } = default!;
        public DbSet<FinanceCalc.Models.Savings> Savings { get; set; }
        public DbSet<InboxMessage> InboxMessage { get; set; } = default!;
        public DbSet<FinanceCalc.Models.Income> Income { get; set; } = default!;
    }
}
