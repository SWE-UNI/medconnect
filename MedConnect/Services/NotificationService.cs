using MedConnect.Data;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Services;

/// Resolves a patient's phone number and triggers SMS notifications. All sends
/// run through <see cref="SmsService"/>, which is a no-op without Sms:ApiKey.
public class NotificationService(ApplicationDbContext db, SmsService sms)
{
    public async Task SendAppointmentReminderAsync(int patientId, string facilityName, DateTime scheduledTimeUtc)
    {
        var phone = await GetPhoneAsync(patientId);
        if (phone is null)
        {
            return;
        }

        var when = scheduledTimeUtc.ToUniversalTime().ToString("ddd d MMM yyyy HH:mm") + " GMT";
        await sms.SendAsync(phone,
            $"MedConnect: You have an appointment at {facilityName} on {when}. Please arrive 15 minutes early.");
    }

    public async Task SendReferralCreatedAsync(int patientId, string toFacility)
    {
        var phone = await GetPhoneAsync(patientId);
        if (phone is null)
        {
            return;
        }

        await sms.SendAsync(phone,
            $"MedConnect: A referral has been created for you to {toFacility}. You will be notified when it is accepted.");
    }

    public async Task SendReferralStatusChangedAsync(int patientId, string statusText)
    {
        var phone = await GetPhoneAsync(patientId);
        if (phone is null)
        {
            return;
        }

        await sms.SendAsync(phone, $"MedConnect: Your referral status is now: {statusText}.");
    }

    public async Task SendLabResultReadyAsync(int patientId, string testName)
    {
        var phone = await GetPhoneAsync(patientId);
        if (phone is null)
        {
            return;
        }

        await sms.SendAsync(phone, $"MedConnect: Your {testName} result is ready. Please visit the facility to collect it.");
    }

    private Task<string?> GetPhoneAsync(int patientId) =>
        db.Patients.AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .Select(p => p.ContactInfo)
            .FirstOrDefaultAsync();
}