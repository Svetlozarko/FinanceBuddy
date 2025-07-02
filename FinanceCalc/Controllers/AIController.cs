using Microsoft.AspNetCore.Mvc;
using FinanceCalc.Services;

namespace FinanceCalc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly OpenAIService _openAI;

        public AIController(OpenAIService openAI)
        {
            _openAI = openAI;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string prompt)
        {
            var response = await _openAI.AskChatGPT(prompt);
            return Ok(new { answer = response });
        }
    }

}
