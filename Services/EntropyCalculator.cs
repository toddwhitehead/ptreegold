using PwdGen.Models;

namespace PwdGen.Services;

public class EntropyCalculator
{
    private const string AmbiguousChars = "0Ol1I";
    private const string DefaultUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DefaultLowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string DefaultDigits = "0123456789";
    private const string DefaultSymbols = "!@#$%^&*-_+=";

    public record EntropyResult(double Bits, string Label, string Color);

    public EntropyResult Calculate(string password, PasswordPattern pattern)
    {
        int poolSize = ComputePoolSize(pattern);
        double bits = poolSize > 1
            ? password.Length * Math.Log2(poolSize)
            : 0;

        return new EntropyResult(bits, GetLabel(bits), GetColor(bits));
    }

    public int ComputePoolSize(PasswordPattern pattern)
    {
        var pool = new HashSet<char>();

        AddToPool(pool, DefaultUppercase, pattern.UseUppercase, pattern.ExcludeAmbiguous);
        AddToPool(pool, DefaultLowercase, pattern.UseLowercase, pattern.ExcludeAmbiguous);
        AddToPool(pool, DefaultDigits, pattern.UseDigits, pattern.ExcludeAmbiguous);

        if (pattern.UseSymbols)
        {
            string symbols = string.IsNullOrWhiteSpace(pattern.SymbolSet)
                ? DefaultSymbols
                : pattern.SymbolSet;
            AddToPool(pool, symbols, true, pattern.ExcludeAmbiguous);
        }

        return pool.Count;
    }

    private static void AddToPool(HashSet<char> pool, string chars, bool enabled, bool excludeAmbiguous)
    {
        if (!enabled) return;
        foreach (char c in chars)
            if (!excludeAmbiguous || !AmbiguousChars.Contains(c))
                pool.Add(c);
    }

    private static string GetLabel(double bits) => bits switch
    {
        < 40 => "Weak",
        < 60 => "Fair",
        < 80 => "Strong",
        < 100 => "Very Strong",
        _ => "Excellent"
    };

    private static string GetColor(double bits) => bits switch
    {
        < 40 => "red",
        < 60 => "yellow",
        < 80 => "blue",
        < 100 => "green",
        _ => "lime"
    };
}
