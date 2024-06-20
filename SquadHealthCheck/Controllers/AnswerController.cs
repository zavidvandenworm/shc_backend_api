using Common.Dto;
using Common.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SquadHealthCheck.Controllers;


[ApiController]
[Route("answer")]
public class AnswerController : ControllerBase
{
    private readonly QuestionRepository _questionRepository;

    public AnswerController(QuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }
    
    [EnableCors("allowDevApi")]
    [HttpPost("set")]
    public async Task<IActionResult> Send([FromBody]SendAnswerDto sendAnswerDto)
    {
        await _questionRepository.UpdateOrInsertAnswer(sendAnswerDto.QuestionId, sendAnswerDto.UserId, sendAnswerDto.Answer);
        return Ok();
    }
}