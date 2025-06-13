using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceCalc.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal GoalAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
