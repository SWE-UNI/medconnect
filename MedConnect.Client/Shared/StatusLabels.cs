namespace MedConnect.Client.Shared;

public static class StatusLabels
{
    private static readonly string[] ReferralStatuses = ["Pending", "Accepted", "InTransit", "Completed", "Cancelled"];
    private static readonly string[] AppointmentStatuses = ["Scheduled", "Completed", "Cancelled", "NoShow"];

    public static string Referral(int status) => ReferralStatuses.ElementAtOrDefault(status) ?? "Unknown";
    public static string Appointment(int status) => AppointmentStatuses.ElementAtOrDefault(status) ?? "Unknown";
}
