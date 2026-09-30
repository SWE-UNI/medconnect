using QRCoder;

namespace MedConnect.Client.Features.Auth;

/// <summary>
/// Renders an otpauth:// URI as a base64 PNG data URL using QRCoder. Runs
/// entirely in the browser (WASM) — no server round-trip, no System.Drawing.
/// </summary>
public static class QrImage
{
    public static string? ToDataUrl(string payload, int pixelsPerModule = 6)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            using var qr = new PngByteQRCode(data);
            byte[] png = qr.GetGraphic(pixelsPerModule);
            return "data:image/png;base64," + Convert.ToBase64String(png);
        }
        catch
        {
            return null;
        }
    }
}