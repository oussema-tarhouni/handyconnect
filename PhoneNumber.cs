namespace HandyConnect.Api.Common;

public static class PhoneNumber
{
    /// <summary>
    /// Normalises a Tunisian number to "+216XXXXXXXX". Returns null when it cannot be one.
    /// Accepts "20 123 456", "+216 20 123 456", "00216 20123456".
    /// </summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = new string(raw.Where(char.IsAsciiDigit).ToArray());
        if (digits.StartsWith("00")) digits = digits[2..];

        if (digits.Length == 8) return "+216" + digits;
        if (digits.Length == 11 && digits.StartsWith("216")) return "+" + digits;
        return null;
    }
}
