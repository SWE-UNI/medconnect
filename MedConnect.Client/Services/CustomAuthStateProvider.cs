using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace MedConnect.Client.Services;

public class CustomAuthStateProvider(TokenStorage tokenStorage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorage.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonymous);
        }

        return new AuthenticationState(BuildPrincipal(token));
    }

    public void NotifyUserAuthenticated(string token) =>
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(BuildPrincipal(token))));

    public void NotifyUserLoggedOut() =>
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));

    private static ClaimsPrincipal BuildPrincipal(string jwt)
    {
        var claims = ParseClaimsFromJwt(jwt);
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var json = Encoding.UTF8.GetString(ParseBase64WithoutPadding(payload));
        var pairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

        foreach (var (key, value) in pairs)
        {
            var claimType = key switch
            {
                "sub" => ClaimTypes.NameIdentifier,
                "email" => ClaimTypes.Email,
                _ => key
            };

            yield return new Claim(claimType, value.ToString());
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
