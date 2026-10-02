using MedConnect.Domain;
using MedConnect.Features.Facilities.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Features.Facilities;

public class FacilityService(IFacilityRepository repository)
{
    public async Task<List<FacilityDto>> GetAllAsync()
    {
        var facilities = await repository.GetAllAsync();
        return facilities.Select(ToDto).ToList();
    }

    public async Task<FacilityDto?> GetByIdAsync(int id)
    {
        var facility = await repository.GetByIdAsync(id);
        return facility is null ? null : ToDto(facility);
    }

    public async Task<FacilityDto> CreateAsync(CreateFacilityDto dto)
    {
        var facility = await repository.AddAsync(new Facility
        {
            Name = dto.Name,
            Type = dto.Type,
            Location = dto.Location
        });
        return ToDto(facility);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteAsync(int id)
    {
        try
        {
            var deleted = await repository.DeleteAsync(id);
            return deleted ? (true, null) : (false, "not-found");
        }
        catch (DbUpdateException)
        {
            return (false, "This facility can't be deleted while it still has staff, patients, visits, referrals, or appointments linked to it.");
        }
    }

    private static FacilityDto ToDto(Facility f) => new(f.FacilityId, f.Name, f.Type, f.Location);
}
