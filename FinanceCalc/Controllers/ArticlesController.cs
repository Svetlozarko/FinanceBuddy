using Microsoft.AspNetCore.Mvc;

namespace FinanceCalc.Controllers
{
    [Route("Articles")]
    public class ArticlesController : Controller
    {
        [HttpGet("{slug}")]
        public IActionResult Article(string slug)
        {
            return slug switch
            {
                "article1" => View("Article1"),
                "article2" => View("Article2"),
                "article3" => View("Article3"),
                _ => NotFound()
            };
        }
    }
}
