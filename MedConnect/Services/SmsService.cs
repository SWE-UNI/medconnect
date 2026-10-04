using System.Text;
using System.Text.Json;

namespace MedConnect.Services;

/// Thin wrapper around the Arkesel SMS API (https://sms.arkesel.com). When
/// Sms:ApiKey is not configured every send becomes a logged no-op, so the whole
/// feature is safe to run in development with no Arkesel account.
public class SmsService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<SmsService> logger)
{
    private const string ArkeselApiUrl = "https://sms.arkesel.com/api/v2/sms/send";

    public async Task<bool> SendAsync(string recipient, string message)
    {
        var apiKey = configuration["Sms:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogInformation("SMS disabled (no Sms:ApiKey configured). Would send to {To}: {Message}", recipient, message);
            return true;
        }

        try
        {
            var client = httpClientFactory.CreateClient("SmsApi");
            var payload = new
            {
                sender = configuration["Sms:SenderId"] ?? "MEDCONNECT",
                message,
                recipients = new[] { recipient }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, ArkeselApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                logger.LogWarning("Arkesel SMS failed ({Status}): {Body}", response.StatusCode, body);
                return false;
            }

            logger.LogInformation("SMS sent to {To}", recipient);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SMS send failed for {To}", recipient);
            return false;
        }
    }
}