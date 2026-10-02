using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Referrals;

public class ReferralsApiClient(HttpClient http)
{
    public Task<List<ReferralDto>> GetByPatientAsync(int patientId) =>
        SafeApi.GetListAsync<ReferralDto>(http, $"api/v1/referrals/patient/{patientId}");

    public Task<List<ReferralDto>> GetIncomingAsync(int facilityId) =>
        SafeApi.GetListAsync<ReferralDto>(http, $"api/v1/referrals/facility/{facilityId}/incoming");

    public Task<ApiResult<ReferralDto>> CreateAsync(CreateReferralDto dto) =>
        SafeApi.PostAsync<CreateReferralDto, ReferralDto>(http, "api/v1/referrals", dto);

    public Task<ApiResult<ReferralDto>> UpdateStatusAsync(int id, int status) =>
        SafeApi.PutAsync<object, ReferralDto>(http, $"api/v1/referrals/{id}/status", new { Status = status });
}