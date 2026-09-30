namespace MedConnect.Common.Constants;

public static class Roles
{
    public const string Patient = "Patient";
    public const string CHW = "CHW";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";
    public const string Receptionist = "Receptionist";
    public const string FacilityAdmin = "FacilityAdmin";
    public const string Admin = "Admin";

    public static readonly string[] All =
        [Patient, CHW, Doctor, Nurse, Receptionist, FacilityAdmin, Admin];

    /// Roles that may only be assigned by an Admin (everything except Patient,
    /// which is the only role that self-registers).
    public static readonly string[] Staff =
        [CHW, Doctor, Nurse, Receptionist, FacilityAdmin, Admin];

    /// Roles that need a FacilityId at registration (everyone except Patient).
    public static readonly string[] FacilityScoped = [CHW, Doctor, Nurse, Receptionist, FacilityAdmin];

    // Full clinical-record access: patients, visits, referrals, vitals.
    public const string ClinicalStaff = $"{CHW},{Doctor},{Nurse},{FacilityAdmin},{Admin}";

    // Clinical staff plus front-desk: appointment scheduling only.
    public const string AppointmentStaff = $"{ClinicalStaff},{Receptionist}";
}
