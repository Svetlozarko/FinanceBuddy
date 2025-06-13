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
        public DbSet<FinanceCalc.Models.Transaction> Transaction { get; set; } = default!;
        public DbSet<SavingGoal> SavingGoals { get; set; }
    }
}
