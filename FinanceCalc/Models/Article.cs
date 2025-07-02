namespace FinanceCalc.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string HtmlContent { get; set; } // The saved HTML body
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
