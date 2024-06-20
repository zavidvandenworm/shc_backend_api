using ApplicationEF.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SquadHealthCheck.Controllers;

[Route("complete")]
[ApiController]
public class AiController : ControllerBase
{
    private readonly AiService _aiService;

    public AiController(AiService aiService)
    {
        _aiService = aiService;
    }


    public class AiDto
    {
        public string Comment { get; set; } = null!;
        public string QuestionTitle { get; set; } = null!;
    }
    
    [HttpPost("comment")]
    public async Task<IActionResult> Comment([FromBody]AiDto aiDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var completion = await _aiService.GetResponse(AiService.RewriteCommentSystemPrompt, JsonConvert.SerializeObject(aiDto));

        return Ok(completion);
    }
}