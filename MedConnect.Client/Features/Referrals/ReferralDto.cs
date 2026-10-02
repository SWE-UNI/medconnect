namespace MedConnect.Client.Features.Referrals;

public record ReferralDto(
    int ReferralId, int PatientId,
    int FromFacilityId, string FromFacilityName,
    int ToFacilityId, string ToFacilityName,
    int Status, DateTime Timestamp);
