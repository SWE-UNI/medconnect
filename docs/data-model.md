# Data Model

Implemented in `MedConnect/Domain/`, migrated via EF Core
(`MedConnect/Data/Migrations/`). All foreign keys use
`DeleteBehavior.Restrict` — several entities (Visit, Referral, Appointment)
reference both `Patient` and `Facility`, and `Referral` references `Facility`
twice (`FromFacility`/`ToFacility`); cascading any of these would create the
multiple-cascade-paths conflict SQL Server rejects at migration time, so
deletion of referenced Patients/Facilities is handled explicitly at the
application level instead.

| Entity | Key fields | Relationships |
|---|---|---|
| `Patient` | `PatientId`, `NHISNumber` (unique index), `FullName`, `DateOfBirth`, `ContactInfo` | 1–many `Visit`, `Referral`, `Appointment`, `FieldVisitLog` |
| `ApplicationUser` (extends `IdentityUser`) | `Id`, `FullName`, `FacilityId` | many–1 `Facility`; 1–many `FieldVisitLog`, `Appointment` (as creator). Role comes from ASP.NET Core Identity roles, not a field on the entity |
| `Facility` | `FacilityId`, `Name`, `Type` (CHPS/Clinic/Hospital), `Location` | 1–many `ApplicationUser`, `Visit` |
| `Visit` | `VisitId`, `PatientId`, `FacilityId`, `Date`, `Diagnosis`, `PrescriptionRef` | many–1 `Patient`, `Facility` |
| `Referral` | `ReferralId`, `PatientId`, `FromFacilityId`, `ToFacilityId`, `Status`, `Timestamp` | many–1 `Patient`; two many–1 relationships to `Facility`. Drives `ReferralHub` alerts |
| `FieldVisitLog` | `LogId`, `CHWUserId`, `PatientId` (optional), `HouseholdData`, `Latitude`/`Longitude`, `VisitDate` | many–1 `ApplicationUser` (CHW), many–1 `Patient` (optional) |
| `Appointment` | `AppointmentId`, `PatientId`, `FacilityId`, `ScheduledTime`, `Status`, `CreatedByUserId` | many–1 `Patient`, `Facility`, `ApplicationUser` (creator) |

## Enums (`MedConnect/Domain/Enums/`)

- `UserRole`: Patient, CHW, Clinician, Admin
- `FacilityType`: CHPS, Clinic, Hospital
- `ReferralStatus`: Pending, Accepted, InTransit, Completed, Cancelled
- `AppointmentStatus`: Scheduled, Completed, Cancelled, NoShow
