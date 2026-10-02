namespace MedConnect.Client.Features.Referrals;

public record CreateReferralDto(int PatientId, int FromFacilityId, int ToFacilityId);
