using MedConnect.Common.Constants;
using MedConnect.Features.Divisions.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Divisions;

[ApiController]
[Route("api/v1/divisions")]
public class DivisionsController(DivisionService service) : ControllerBase
{
    // Public: the registration form needs to populate a division dropdown
    // for Doctors before the user has a token, same rationale as Facilities.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateDivisionDto dto)
    {
        var (succeeded, error, division) = await service.CreateAsync(dto);
        return succeeded ? Ok(division) : BadRequest(new { error });
    }
}
