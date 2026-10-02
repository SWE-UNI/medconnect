using MedConnect.Domain;
using MedConnect.Features.Divisions.Dtos;

namespace MedConnect.Features.Divisions;

public class DivisionService(IDivisionRepository repository)
{
    public async Task<List<DivisionDto>> GetAllAsync()
    {
        var divisions = await repository.GetAllAsync();
        return divisions.Select(ToDto).ToList();
    }

    public async Task<(bool Succeeded, string? Error, DivisionDto? Division)> CreateAsync(CreateDivisionDto dto)
    {
        if (await repository.ExistsByNameAsync(dto.Name))
        {
            return (false, "A division with this name already exists.", null);
        }

        var division = await repository.AddAsync(new Division { Name = dto.Name });
        return (true, null, ToDto(division));
    }

    private static DivisionDto ToDto(Division d) => new(d.DivisionId, d.Name);
}
