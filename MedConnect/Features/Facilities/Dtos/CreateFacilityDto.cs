using MedConnect.Domain.Enums;

namespace MedConnect.Features.Facilities.Dtos;

public record CreateFacilityDto(string Name, FacilityType Type, string Location);
