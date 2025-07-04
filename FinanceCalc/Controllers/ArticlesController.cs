using Microsoft.AspNetCore.Mvc;

namespace FinanceCalc.Controllers
{
    [Route("Articles")]
    public class ArticlesController : Controller
    {
        // GET: /Articles
        [HttpGet("")]
        public IActionResult Index()
        {
            // Return to home page or create an Articles index page
            return RedirectToAction("Index", "Home");
        }

        // GET: /Articles/{slug}
        [HttpGet("{slug}")]
        public IActionResult Article(string slug)
        {
            // Convert slug to lowercase for case-insensitive matching
            var normalizedSlug = slug?.ToLowerInvariant();

            return normalizedSlug switch
            {
                "article1" or "5-easy-ways-to-start-saving-today" => View("Article1"),
                "article2" or "how-to-build-an-emergency-fund" => View("Article2"),
                "article3" or "master-your-money-beginners-guide" => View("Article3"),
                _ => NotFound()
            };
        }

        // Optional: Add individual action methods for direct access
        [HttpGet("Article1")]
        public IActionResult Article1() => View();

        [HttpGet("Article2")]
        public IActionResult Article2() => View();

        [HttpGet("Article3")]
        public IActionResult Article3() => View();
    }
}
