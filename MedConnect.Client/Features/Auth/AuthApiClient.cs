using System.Net.Http.Headers;
using System.Net.Http.Json;
using MedConnect.Client.Features.Auth.Dtos;
using MedConnect.Client.Services;

namespace MedConnect.Client.Features.Auth;

public class AuthApiClient(HttpClient http)
{
    public Task<ApiResult<AuthResponseDto>> LoginAsync(LoginDto dto) =>
        SafeApi.PostAsync<LoginDto, AuthResponseDto>(http, "api/v1/auth/login", dto);

    public Task<ApiResult<AuthResponseDto>> RegisterAsync(RegisterDto dto) =>
        SafeApi.PostAsync<RegisterDto, AuthResponseDto>(http, "api/v1/auth/register", dto);

    public Task<ApiResult<AuthResponseDto>> RegisterStaffAsync(RegisterStaffDto dto) =>
        SafeApi.PostAsync<RegisterStaffDto, AuthResponseDto>(http, "api/v1/auth/register-staff", dto);

    public async Task<ApiResult<bool>> StatusTwoFactorAsync()
    {
        var body = await SafeApi.GetNullableAsync<EnabledResponse>(http, "api/v1/auth/2fa/status");
        return body is null
            ? ApiResult<bool>.Fail(SafeApi.UnreachableMessage)
            : ApiResult<bool>.Ok(body.Enabled);
    }

    public async Task<ApiResult<TwoFactorSetupDto>> SetupTwoFactorAsync()
    {
        var result = await SafeApi.SendAsync<TwoFactorSetupDto>(http, () => http.PostAsync("api/v1/auth/2fa/setup", null));
        return result.Value is not null ? ApiResult<TwoFactorSetupDto>.Ok(result.Value) : result;
    }

    public Task<ApiResult<bool>> EnableTwoFactorAsync(string code) =>
        SafeApi.PostStatusAsync(http, "api/v1/auth/2fa/enable", new TwoFactorCodeDto(code));

    public Task<ApiResult<bool>> DisableTwoFactorAsync(string code) =>
        SafeApi.PostStatusAsync(http, "api/v1/auth/2fa/disable", new TwoFactorCodeDto(code));

    /// Completes the second factor using the short-lived ticket issued at login.
    /// The ticket is sent as its own Authorization header because it is not (and
    /// must not be) the stored session token.
    public Task<ApiResult<AuthResponseDto>> VerifyTwoFactorAsync(string code, string pendingToken)
    {
        HttpRequestMessage BuildRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/2fa/verify")
            {
                Content = JsonContent.Create(new TwoFactorCodeDto(code))
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", pendingToken);
            return request;
        }

        return SafeApi.SendAsync<AuthResponseDto>(http, async () =>
        {
            using var request = BuildRequest();
            return await http.SendAsync(request);
        });
    }

    public Task<ApiResult<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordDto dto) =>
        SafeApi.PostAsync<ForgotPasswordDto, ForgotPasswordResponseDto>(http, "api/v1/auth/forgot-password", dto);

    public Task<ApiResult<bool>> ResetPasswordAsync(ResetPasswordDto dto) =>
        SafeApi.PostStatusAsync(http, "api/v1/auth/reset-password", dto);

    private sealed class EnabledResponse(bool enabled)
    {
        public bool Enabled { get; } = enabled;
    }
}