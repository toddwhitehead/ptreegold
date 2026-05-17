using System.Security.Cryptography;
using PTreeGold.Models;

namespace PTreeGold.Services;

public class PasswordGenerator
{
    private const string AmbiguousChars = "0Ol1I";
    private const string DefaultUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DefaultLowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string DefaultDigits = "0123456789";
    private const string DefaultSymbols = "!@#$%^&*-_+=";

    public string Generate(PasswordPattern pattern)
    {
        char[] upper = BuildPool(DefaultUppercase, pattern.ExcludeAmbiguous);
        char[] lower = BuildPool(DefaultLowercase, pattern.ExcludeAmbiguous);
        char[] digits = BuildPool(DefaultDigits, pattern.ExcludeAmbiguous);
        char[] symbols = BuildPool(
            string.IsNullOrWhiteSpace(pattern.SymbolSet) ? DefaultSymbols : pattern.SymbolSet,
            pattern.ExcludeAmbiguous);

        // Build full pool from enabled character sets
        var fullPool = new List<char>();
        if (pattern.UseUppercase && upper.Length > 0) fullPool.AddRange(upper);
        if (pattern.UseLowercase && lower.Length > 0) fullPool.AddRange(lower);
        if (pattern.UseDigits && digits.Length > 0) fullPool.AddRange(digits);
        if (pattern.UseSymbols && symbols.Length > 0) fullPool.AddRange(symbols);

        if (fullPool.Count == 0)
            throw new InvalidOperationException("Pattern produces an empty character pool.");

        char[] pool = fullPool.ToArray();

        // Pick required minimums from each enabled set
        var required = new List<char>();
        if (pattern.UseUppercase && upper.Length > 0 && pattern.MinUppercase > 0)
            required.AddRange(RandomNumberGenerator.GetItems<char>(upper, pattern.MinUppercase));
        if (pattern.UseLowercase && lower.Length > 0 && pattern.MinLowercase > 0)
            required.AddRange(RandomNumberGenerator.GetItems<char>(lower, pattern.MinLowercase));
        if (pattern.UseDigits && digits.Length > 0 && pattern.MinDigits > 0)
            required.AddRange(RandomNumberGenerator.GetItems<char>(digits, pattern.MinDigits));
        if (pattern.UseSymbols && symbols.Length > 0 && pattern.MinSymbols > 0)
            required.AddRange(RandomNumberGenerator.GetItems<char>(symbols, pattern.MinSymbols));

        // Choose final length
        int minLen = Math.Max(pattern.MinLength, required.Count);
        int maxLen = Math.Max(pattern.MaxLength, minLen);
        int length = RandomNumberGenerator.GetInt32(minLen, maxLen + 1);

        // Fill remaining slots from full pool
        int remaining = length - required.Count;
        char[] fill = remaining > 0
            ? RandomNumberGenerator.GetItems<char>(pool, remaining)
            : Array.Empty<char>();

        // Combine and shuffle
        char[] result = [.. required, .. fill];
        RandomNumberGenerator.Shuffle<char>(result.AsSpan());

        return new string(result);
    }

    private static char[] BuildPool(string source, bool excludeAmbiguous)
    {
        if (!excludeAmbiguous) return source.ToCharArray();
        return source.Where(c => !AmbiguousChars.Contains(c)).ToArray();
    }
}
