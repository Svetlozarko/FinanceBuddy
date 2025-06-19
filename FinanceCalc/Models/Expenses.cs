using FinanceCalc.Data;
using FinanceCalc.Enums;
using System.Text.Json.Serialization;

namespace FinanceCalc.Models
{
    public class Expenses
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string? Category { get; set; }

        [JsonIgnore] 
        public string UserId { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
    