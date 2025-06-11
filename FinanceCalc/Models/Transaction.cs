using FinanceCalc.Enums;

namespace FinanceCalc.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }

        public Guid? ExpenseCategoryId { get; set; }
        public string? ExpenseCategory { get; set; }

        public TransactionType Type { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
