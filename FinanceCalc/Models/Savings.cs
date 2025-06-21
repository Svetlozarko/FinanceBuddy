using FinanceCalc.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceCalc.Models
{
    public class Savings
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; } 

    }

}
