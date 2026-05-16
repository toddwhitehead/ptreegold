namespace PwdGen.Models;

public class PasswordPattern
{
    public string Description { get; set; } = string.Empty;
    public int MinLength { get; set; } = 16;
    public int MaxLength { get; set; } = 24;
    public bool UseUppercase { get; set; } = true;
    public bool UseLowercase { get; set; } = true;
    public bool UseDigits { get; set; } = true;
    public bool UseSymbols { get; set; } = true;
    public string SymbolSet { get; set; } = "!@#$%^&*-_+=";
    public bool ExcludeAmbiguous { get; set; } = true;
    public int MinUppercase { get; set; } = 1;
    public int MinLowercase { get; set; } = 1;
    public int MinDigits { get; set; } = 1;
    public int MinSymbols { get; set; } = 1;
}
