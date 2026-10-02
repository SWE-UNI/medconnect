using System.Security.Claims;
using MedConnect.Common.Constants;
using MedConnect.Features.FieldVisits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.FieldVisits;

[ApiController]
[Route("api/v1/field-visits")]
[Authorize(Roles = Roles.CHW)]
public class FieldVisitsController(FieldVisitLogService service) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await service.GetByCHWUserIdAsync(userId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFieldVisitLogDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var log = await service.CreateAsync(dto, userId);
        return Ok(log);
    }
}
