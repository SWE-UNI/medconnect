using MedConnect.Common.Constants;
using MedConnect.Features.Facilities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedConnect.Features.Facilities;

[ApiController]
[Route("api/v1/facilities")]
public class FacilitiesController(FacilityService service) : ControllerBase
{
    // Public: the registration form needs to populate a facility dropdown
    // before the user has a token, and facility name/location isn't sensitive.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var facility = await service.GetByIdAsync(id);
        return facility is null ? NotFound() : Ok(facility);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateFacilityDto dto)
    {
        var facility = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = facility.FacilityId }, facility);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var (succeeded, error) = await service.DeleteAsync(id);
        if (succeeded)
        {
            return NoContent();
        }

        return error == "not-found" ? NotFound() : BadRequest(new { error });
    }
}
