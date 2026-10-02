namespace MedConnect.Common;

/// Ghana mobile number parsing and normalisation to the +233 international form.
public static class GhanaPhone
{
    private static readonly string[] NetworkCodes =
        ["20", "23", "24", "26", "27", "50", "53", "54", "55", "56", "57", "59"];

    /// Accepts 0241234567 or 233241234567 (with spaces/dashes) and returns the
    /// E.164 form +233241234567.
    public static bool TryNormalize(string? input, out string? normalized)
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.Length == 12 && digits.StartsWith("233"))
        {
            var code = digits.Substring(3, 2);
            if (NetworkCodes.Contains(code))
            {
                normalized = "+" + digits;
            }

            return normalized is not null;
        }

        if (digits.Length == 10 && digits.StartsWith("0"))
        {
            var code = digits[1..3];
            if (NetworkCodes.Contains(code))
            {
                normalized = "+233" + digits[1..];
            }

            return normalized is not null;
        }

        return false;
    }
}