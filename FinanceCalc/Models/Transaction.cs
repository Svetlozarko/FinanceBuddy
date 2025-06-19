using FinanceCalc.Enums;

namespace FinanceCalc.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }

        public Guid? CategoryId { get; set; }
        public string? Category { get; set; }

        public TransactionType? Type { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
