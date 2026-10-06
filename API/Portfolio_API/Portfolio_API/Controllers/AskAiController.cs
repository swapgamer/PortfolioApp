using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio_API.Dtos;
using Portfolio_API.Services;

namespace Portfolio_API.Controllers
{
    [ApiController]
    [Route("api/ask-ai")]
    [EnableRateLimiting("ask-ai")]
    public class AskAiController : ControllerBase
    {
        private readonly IRagService _ragService;

        public AskAiController(IRagService ragService)
        {
            _ragService = ragService;
        }

        [HttpPost]
        public async Task<ActionResult<AskAiResponse>> Ask([FromBody] AskAiRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length > 500)
                return BadRequest("Query must be 1-500 characters.");

            try
            {
                var result = await _ragService.GetAnswerAsync(request.Query, ct);
                return Ok(result);
            }
            catch (LlmUnavailableException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
            }
        }
    }
}
