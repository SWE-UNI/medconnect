using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Prescriptions;

public class PrescriptionsApiClient(HttpClient http)
{
    public Task<List<PrescriptionDto>> GetByPatientAsync(int patientId) =>
        SafeApi.GetListAsync<PrescriptionDto>(http, $"api/v1/prescriptions/patient/{patientId}");

    public Task<ApiResult<PrescriptionDto>> CreateAsync(CreatePrescriptionDto dto) =>
        SafeApi.PostAsync<CreatePrescriptionDto, PrescriptionDto>(http, "api/v1/prescriptions", dto);
}