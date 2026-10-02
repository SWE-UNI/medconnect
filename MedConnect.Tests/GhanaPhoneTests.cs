using MedConnect.Common;
using Xunit;

namespace MedConnect.Tests;

public class GhanaPhoneTests
{
    [Theory]
    [InlineData("0241234567", "+233241234567")]
    [InlineData("0200000000", "+233200000000")]
    [InlineData("0555555555", "+233555555555")]
    [InlineData("233241234567", "+233241234567")]
    [InlineData("233 24 123 4567", "+233241234567")]
    [InlineData("024-123-4567", "+233241234567")]
    public void ValidNumbersNormalizeToE164(string input, string expected)
    {
        Assert.True(GhanaPhone.TryNormalize(input, out var normalized));
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("notaphone")]
    [InlineData("12345")]
    [InlineData("+1001234567890123")]
    [InlineData("0301234567")]
    [InlineData("0123456789")]
    [InlineData("2332412345678")]
    public void InvalidNumbersAreRejected(string input)
    {
        Assert.False(GhanaPhone.TryNormalize(input, out var normalized));
        Assert.Null(normalized);
    }
}