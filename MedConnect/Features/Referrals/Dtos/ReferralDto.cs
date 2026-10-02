using MedConnect.Domain.Enums;

namespace MedConnect.Features.Referrals.Dtos;

public record ReferralDto(
    int ReferralId,
    int PatientId,
    int FromFacilityId,
    string FromFacilityName,
    int ToFacilityId,
    string ToFacilityName,
    ReferralStatus Status,
    DateTime Timestamp);
