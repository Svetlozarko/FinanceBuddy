using FinanceCalc.Data;

namespace FinanceCalc.Models
{
    public class InboxMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ApplicationUser User { get; set; }
        public string? ImageUrl { get; set; }
    }

}
