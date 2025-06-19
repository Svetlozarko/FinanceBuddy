namespace FinanceCalc.Models
{
    public class DashboardViewModel
    {
        public List<Expenses> Expenses { get; set; }
        public List<Income> Income { get; set; }
        public List<Savings> Savings { get; set; }
        public List<Expenses> AllTransactions { get; set; }
    }
}
