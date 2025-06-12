namespace FinanceCalc.Models
{
    public class DashboardViewModel
    {
        public List<Transaction> Expenses { get; set; }
        public List<Transaction> Income { get; set; }
        public List<Transaction> Savings { get; set; }
        public List<Transaction> AllTransactions { get; set; }
    }
}
