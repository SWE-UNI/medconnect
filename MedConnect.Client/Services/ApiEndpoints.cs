namespace MedConnect.Client.Services;

/// <summary>
/// API origin used by REST calls and SignalR hub connections. Populated once at
/// startup from the "ApiBaseUrl" configuration value (appsettings.json), falling
/// back to the SPA's own origin when empty — so local dev points at the Web API
/// while a same-origin deployment needs no configuration at all.
/// </summary>
public static class ApiEndpoints
{
    public static string BaseUrl { get; private set; } = string.Empty;

    public static void Initialize(string? configuredBaseUrl, string fallbackOrigin)
    {
        var value = string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? fallbackOrigin
            : configuredBaseUrl;

        // Prevent Mixed Content: If the SPA is hosted over HTTPS, browsers strictly block
        // direct requests to insecure HTTP endpoints (e.g. http://medconnect.runasp.net).
        // Fall back to the SPA's own HTTPS origin so requests are securely proxied.
        if (fallbackOrigin.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
            value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            value = fallbackOrigin;
        }

        BaseUrl = value.TrimEnd('/');
    }

    public static string Hub(string path) => $"{BaseUrl}{path}";
}