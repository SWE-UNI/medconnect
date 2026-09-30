using QRCoder;
using Xunit;

namespace MedConnect.Tests;

public class QrImageTests
{
    // Mirrors MedConnect.Client.Features.Auth.QrImage.ToDataUrl so the exact
    // WASM runtime path is exercised on the same .NET runtime.
    private static string? ToDataUrl(string payload, int pixelsPerModule = 6)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qr = new PngByteQRCode(data);
        byte[] png = qr.GetGraphic(pixelsPerModule);
        return "data:image/png;base64," + Convert.ToBase64String(png);
    }

    [Fact]
    public void OtpauthUri_ProducesPngDataUrl()
    {
        const string uri = "otpauth://totp/MedConnect%20GH:admin%40medconnect.gh" +
                           "?secret=GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ&issuer=MedConnect%20GH&period=30&digits=6";

        var dataUrl = ToDataUrl(uri);

        Assert.NotNull(dataUrl);
        Assert.StartsWith("data:image/png;base64,", dataUrl);

        var png = Convert.FromBase64String(dataUrl.Split(',')[1]);
        // PNG magic number 89 50 4E 47 0D 0A 1A 0A
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png.Take(8).ToArray());
        Assert.True(png.Length > 500, $"PNG unexpectedly small: {png.Length} bytes");
    }
}