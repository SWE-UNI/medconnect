using MedConnect.Features.Identity;
using Xunit;

namespace MedConnect.Tests;

public class TotpServiceTests
{
    // RFC 6238 Appendix B known vectors (SHA-1), secret = ASCII
    // "12345678901234567890" -> base32 GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ.
    [Theory]
    [InlineData(1, "287082")]
    [InlineData(37037036, "081804")]
    [InlineData(37037037, "050471")]
    [InlineData(41152263, "005924")]
    public void Rfc6238Sha1Vectors(long counter, string expected)
    {
        const string secret = "GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ";
        Assert.Equal(expected, TotpService.GenerateCode(secret, counter));
    }

    [Fact]
    public void ValidateCode_RejectsGarbage()
    {
        var service = new TotpService();
        Assert.False(service.ValidateCode("", "123456"));
        Assert.False(service.ValidateCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", ""));
        Assert.False(service.ValidateCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", "ABC123"));
        Assert.False(service.ValidateCode("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", "12345"));
    }

    [Fact]
    public void GenerateSecret_Is32Base32CharsAndRoundTrips()
    {
        var service = new TotpService();
        var secret = service.GenerateSecret();
        Assert.Equal(32, secret.Length);
        Assert.All(secret.ToCharArray(), c => Assert.True("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".Contains(c)));
        // The secret decodes to 20 bytes; a code must still generate.
        _ = TotpService.GenerateCode(secret, 0);
    }

    [Fact]
    public void GetOtpauthUri_ContainsSecretIssuerAndPeriod()
    {
        var uri = new TotpService().GetOtpauthUri("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567", "admin@medconnect.gh");
        Assert.StartsWith("otpauth://totp/", uri);
        Assert.Contains("secret=ABCDEFGHIJKLMNOPQRSTUVWXYZ234567", uri);
        Assert.Contains("issuer=MedConnect%20GH", uri);
        Assert.Contains("period=30", uri);
    }

    [Fact]
    public void ValidateCode_AcceptsCurrentCode()
    {
        var service = new TotpService();
        var secret = service.GenerateSecret();
        var counter = (long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds / 30);
        var code = TotpService.GenerateCode(secret, counter);
        Assert.True(service.ValidateCode(secret, code));
    }
}