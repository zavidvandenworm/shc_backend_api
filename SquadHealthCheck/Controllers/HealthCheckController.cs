using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SquadHealthCheck.Controllers;

[ApiController]
[Route("healthcheck")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthcheckRepository _healthcheckRepository;
    private readonly UserRepository _userRepository;

    public HealthCheckController(HealthcheckRepository healthcheckRepository, UserRepository userRepository)
    {
        _healthcheckRepository = healthcheckRepository;
        _userRepository = userRepository;
    }

    [EnableCors("allowDevApi")]
    [HttpGet("get/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var healthCheck = await _healthcheckRepository.GetFromLink(id);
        return Ok(healthCheck);
    }

    [EnableCors("allowDevApi")]
    [HttpGet("getall")]
    public async Task<IActionResult> GetAll(int userId)
    {
        var healthCheck = await _healthcheckRepository.GetUserHealthChecks(userId);
        return Ok(healthCheck);
    }

    public class IdToLinkDto
    {
        public int HealthCheckId { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }
    }

    [EnableCors("allowDevApi")]
    [HttpGet("id_to_link")]
    public async Task<IActionResult> IdToLink([FromQuery] IdToLinkDto dto)
    {
        var uniqueLink = await _healthcheckRepository.GetLinkFronIdAndVersionAndUser(dto.HealthCheckId, dto.Version, dto.UserId);
        return Ok(
            new
            {
                UniqueLink = uniqueLink,
                HealthCheckId = dto.HealthCheckId
            });
    }

    public class SubmitDto
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public int UserId { get; set; }
    }
    
    [EnableCors("allowDevApi")]
    [HttpPost("submit/{id:int}")]
    public async Task<IActionResult> Submit([FromBody] SubmitDto dto)
    {
        var uniqueLink = await _healthcheckRepository.GetLinkFronIdAndVersionAndUser(dto.Id, dto.Version, dto.UserId);
        var healthCheck = await _healthcheckRepository.GetFromLink(uniqueLink);

        if (healthCheck.Questions.Any(question => question.Answer?.AnswerColor == null))
        {
            return BadRequest("One or more questions were not answered.");
        }

        await _healthcheckRepository.Submit(dto.Id, dto.Version);
        return Ok();
    }

    [EnableCors("allowDevApi")]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserInfo([FromQuery] string link)
    {
        var user = await _userRepository.GetFromUniqueLink(link);

        return Ok(new
        {
            UserId = user.Id,
            Name = user.Name
        });
    }
}