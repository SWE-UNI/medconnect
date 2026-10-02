namespace MedConnect.Client.Features.FieldVisits;

public record CreateFieldVisitLogDto(
    int? PatientId,
    string HouseholdData,
    double? Latitude,
    double? Longitude,
    DateTime VisitDate,
    int? PregnancyStatus = null,
    int? GestationalAgeWeeks = null,
    int? AntenatalVisits = null,
    int? ChildAgeMonths = null,
    decimal? ChildWeightKg = null,
    bool? ImmunizationsUpToDate = null,
    string? DangerSigns = null);