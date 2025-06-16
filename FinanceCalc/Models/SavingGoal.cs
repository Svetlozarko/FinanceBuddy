using FinanceCalc.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceCalc.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; } // Optional, or use savings sum

        [Required]
        public string Purpose { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser User { get; set; }
    }

}
