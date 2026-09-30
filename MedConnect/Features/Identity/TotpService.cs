using System.Security.Cryptography;
using System.Text;

namespace MedConnect.Features.Identity;

/// RFC 6238 time-based one-time passwords (SHA-1, 30 s step, 6 digits, ±1 window)
/// without any external dependency.
public class TotpService
{
    private const int StepSeconds = 30;
    private const int CodeDigits = 6;
    private const int Window = 1;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string GenerateSecret()
    {
        return ToBase32(RandomNumberGenerator.GetBytes(20));
    }

    public string GetOtpauthUri(string secret, string account, string issuer = "MedConnect GH")
    {
        var label = Uri.EscapeDataString($"{issuer}:{account}");
        var parameters =
            $"secret={Uri.EscapeDataString(secret)}&issuer={Uri.EscapeDataString(issuer)}" +
            $"&algorithm=SHA1&digits={CodeDigits}&period={StepSeconds}";
        return $"otpauth://totp/{label}?{parameters}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var trimmed = code.Trim();
        if (trimmed.Length != CodeDigits || !trimmed.All(char.IsDigit))
        {
            return false;
        }

        var counter = (long)(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds / StepSeconds);
        for (var offset = -Window; offset <= Window; offset++)
        {
            if (FixedTimeEquals(GenerateCode(secret, counter + offset), trimmed))
            {
                return true;
            }
        }

        return false;
    }

    public static string GenerateCode(string secret, long counter)
    {
        var secretBytes = FromBase32(secret);
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24) |
                     ((hash[offset + 1] & 0xFF) << 16) |
                     ((hash[offset + 2] & 0xFF) << 8) |
                     (hash[offset + 3] & 0xFF);
        return (binary % 1_000_000).ToString("D6");
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private static string ToBase32(byte[] data)
    {
        var sb = new StringBuilder();
        var value = 0;
        var bits = 0;
        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(Base32Alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
        {
            sb.Append(Base32Alphabet[(value << (5 - bits)) & 31]);
        }

        return sb.ToString();
    }

    private static byte[] FromBase32(string base32)
    {
        var cleaned = base32.ToUpperInvariant().Replace(" ", "").TrimEnd('=');
        var buffer = 0L;
        var bitsLeft = 0;
        var bytes = new List<byte>();
        foreach (var c in cleaned)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | index;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return bytes.ToArray();
    }
}