using MedConnect.Domain;

namespace MedConnect.Features.Referrals;

public interface IReferralRepository
{
    Task<List<Referral>> GetByPatientIdAsync(int patientId);
    Task<List<Referral>> GetIncomingByFacilityIdAsync(int facilityId);
    Task<Referral?> GetByIdAsync(int id);
    Task<Referral> AddAsync(Referral referral);
    Task UpdateAsync(Referral referral);
}
