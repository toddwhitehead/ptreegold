// Superseded by UI/GenerateDialog.cs.
// This file is intentionally empty.
namespace PTreeGold.UI;

internal static class GenerateFlow_Obsolete
{
#if false
    private readonly PasswordGenerator _generator;
    private readonly EntropyCalculator _entropy;
    private readonly ExportService _export;
    private readonly Dictionary<string, PasswordPattern> _patterns;

    public GenerateFlow(
        PasswordGenerator generator,
        EntropyCalculator entropy,
        ExportService export,
        Dictionary<string, PasswordPattern> patterns)
    {
        _generator = generator;
        _entropy = entropy;
        _export = export;
        _patterns = patterns;
    }

    public async Task RunAsync()
    {
        DisplayHelpers.ShowSectionRule("Generate Password");
        AnsiConsole.WriteLine();

        PasswordPattern pattern = ChoosePattern(out string patternName);

        int count = AnsiConsole.Prompt(
            new TextPrompt<int>("[grey]How many passwords?[/] [cyan](1–50)[/]")
                .DefaultValue(1)
                .Validate(n => n is >= 1 and <= 50
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Enter a number between 1 and 50.[/]")));

        List<string> passwords = [];

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start($"Generating {count} password(s) using [cyan]{patternName}[/]...", _ =>
            {
                for (int i = 0; i < count; i++)
                    passwords.Add(_generator.Generate(pattern));
            });

        DisplayHelpers.ShowResultsTable(passwords, pattern, _entropy);

        await PostGenerateMenuAsync(passwords);
    }

    private PasswordPattern ChoosePattern(out string patternName)
    {
        const string customOption = "Custom (define your own)";

        var choices = _patterns
            .Select(kv => $"{kv.Key}  [grey]{kv.Value.Description}[/]")
            .Prepend(customOption)
            .ToList();

        string selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [cyan]named pattern[/] or choose [yellow]Custom[/]:")
                .PageSize(14)
                .EnableSearch()
                .HighlightStyle(Style.Parse("cyan"))
                .AddChoices(choices));

        if (selection == customOption)
        {
            patternName = "Custom";
            return BuildCustomPattern();
        }

        // Extract the key (text before the first double-space)
        patternName = selection.Split("  ")[0].Trim();
        return _patterns[patternName];
    }

    private static PasswordPattern BuildCustomPattern()
    {
        AnsiConsole.WriteLine();
        DisplayHelpers.ShowSectionRule("Custom Pattern");
        AnsiConsole.WriteLine();

        var sets = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [cyan]character sets[/] to include:")
                .NotRequired()
                .PageSize(6)
                .HighlightStyle(Style.Parse("cyan"))
                .InstructionsText("[grey](Space to toggle, Enter to confirm)[/]")
                .AddChoices("Uppercase (A–Z)", "Lowercase (a–z)", "Digits (0–9)", "Symbols"));

        if (sets.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No sets selected — defaulting to all.[/]");
            sets = ["Uppercase (A–Z)", "Lowercase (a–z)", "Digits (0–9)", "Symbols"];
        }

        int minLen = AnsiConsole.Prompt(
            new TextPrompt<int>("Minimum length:")
                .DefaultValue(16)
                .Validate(n => n >= 4
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Minimum length must be at least 4.[/]")));

        int maxLen = AnsiConsole.Prompt(
            new TextPrompt<int>("Maximum length:")
                .DefaultValue(Math.Max(24, minLen))
                .Validate(n => n >= minLen
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]Max must be ≥ min ({minLen}).[/]")));

        bool excludeAmbig = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Exclude ambiguous characters [grey](0, O, l, 1, I)?[/]")
                .AddChoices("Yes", "No")) == "Yes";

        return new PasswordPattern
        {
            Description = "Custom",
            MinLength = minLen,
            MaxLength = maxLen,
            UseUppercase = sets.Contains("Uppercase (A–Z)"),
            UseLowercase = sets.Contains("Lowercase (a–z)"),
            UseDigits = sets.Contains("Digits (0–9)"),
            UseSymbols = sets.Contains("Symbols"),
            ExcludeAmbiguous = excludeAmbig,
            MinUppercase = sets.Contains("Uppercase (A–Z)") ? 1 : 0,
            MinLowercase = sets.Contains("Lowercase (a–z)") ? 1 : 0,
            MinDigits = sets.Contains("Digits (0–9)") ? 1 : 0,
            MinSymbols = sets.Contains("Symbols") ? 1 : 0
        };
    }

    private async Task PostGenerateMenuAsync(List<string> passwords)
    {
        while (true)
        {
            var actions = new List<string> { "Generate again", "Back to main menu" };

            if (passwords.Count == 1)
                actions.Insert(0, "Copy to clipboard");
            else
                actions.InsertRange(0, Enumerable.Range(1, passwords.Count)
                    .Select(i => $"Copy #{i} to clipboard"));

            actions.Insert(passwords.Count == 1 ? 1 : passwords.Count, "Export to file");

            string action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .PageSize(12)
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(actions));

            if (action.StartsWith("Copy"))
            {
                int index = passwords.Count == 1
                    ? 0
                    : int.Parse(action.Split('#')[1].Split(' ')[0]) - 1;

                await ClipboardService.SetTextAsync(passwords[index]);
                DisplayHelpers.ShowSuccess("Password copied to clipboard. [grey]Remember to clear your clipboard when done.[/]");
            }
            else if (action == "Export to file")
            {
                await ExportPasswordsAsync(passwords);
            }
            else if (action == "Generate again")
            {
                AnsiConsole.WriteLine();
                await RunAsync();
                return;
            }
            else
            {
                return; // Back to main menu
            }

            AnsiConsole.WriteLine();
        }
    }

    private async Task ExportPasswordsAsync(List<string> passwords)
    {
        string defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $"passwords_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        string filePath = AnsiConsole.Prompt(
            new TextPrompt<string>("Export file path:")
                .DefaultValue(defaultPath));

        try
        {
            string saved = await _export.ExportAsync(passwords, filePath);
            DisplayHelpers.ShowSuccess($"Exported {passwords.Count} password(s) to [grey]{Markup.Escape(saved)}[/]");
        }
        catch (Exception ex)
        {
            DisplayHelpers.ShowError(ex.Message);
        }
    }
#endif
}
