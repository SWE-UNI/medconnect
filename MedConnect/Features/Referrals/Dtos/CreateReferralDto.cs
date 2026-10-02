namespace MedConnect.Features.Referrals.Dtos;

public record CreateReferralDto(int PatientId, int FromFacilityId, int ToFacilityId);
