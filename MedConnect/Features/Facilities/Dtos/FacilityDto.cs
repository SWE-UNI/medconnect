using MedConnect.Domain.Enums;

namespace MedConnect.Features.Facilities.Dtos;

public record FacilityDto(int FacilityId, string Name, FacilityType Type, string Location);
