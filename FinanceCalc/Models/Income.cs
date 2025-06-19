using FinanceCalc.Data;

namespace FinanceCalc.Models
{
    public class Income
    {
        public int Id { get; set; }
        public string?  UserId { get; set; }

        public decimal Amount { get; set; }

    }
}
