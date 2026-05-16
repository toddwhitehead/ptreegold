using Spectre.Console;
using PwdGen.Models;
using PwdGen.Services;

namespace PwdGen.UI;

public static class DisplayHelpers
{
    public static void ShowBanner()
    {
        if (AnsiConsole.Profile.Capabilities.Ansi)
            AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("PwdGen").Color(Color.Cyan1));
        AnsiConsole.Write(new Rule("[grey]Enterprise Secure Password Generator[/]").RuleStyle("grey").LeftJustified());
        AnsiConsole.WriteLine();
    }

    public static void ShowResultsTable(
        IReadOnlyList<string> passwords,
        PasswordPattern pattern,
        EntropyCalculator entropy)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[bold cyan]Generated Passwords[/]")
            .AddColumn(new TableColumn("[grey]#[/]").RightAligned().Width(4))
            .AddColumn(new TableColumn("[bold]Password[/]"))
            .AddColumn(new TableColumn("[grey]Length[/]").Centered().Width(8))
            .AddColumn(new TableColumn("[grey]Entropy (bits)[/]").Centered().Width(15))
            .AddColumn(new TableColumn("[grey]Strength[/]").Centered().Width(12));

        for (int i = 0; i < passwords.Count; i++)
        {
            string pwd = passwords[i];
            var result = entropy.Calculate(pwd, pattern);
            table.AddRow(
                $"[grey]{i + 1}[/]",
                $"[bold white]{Markup.Escape(pwd)}[/]",
                $"[grey]{pwd.Length}[/]",
                $"[grey]{result.Bits:F1}[/]",
                $"[{result.Color}]{result.Label}[/]"
            );
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void ShowPatternsTable(Dictionary<string, PasswordPattern> patterns)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[bold cyan]Named Patterns[/]")
            .AddColumn(new TableColumn("[bold]Name[/]").Width(22))
            .AddColumn(new TableColumn("[grey]Description[/]"))
            .AddColumn(new TableColumn("[grey]Length[/]").Centered().Width(10))
            .AddColumn(new TableColumn("[grey]Char Sets[/]").Centered().Width(12))
            .AddColumn(new TableColumn("[grey]Minimums[/]").Centered().Width(16));

        foreach (var (key, p) in patterns)
        {
            string sets = string.Join("+", new[]
            {
                p.UseUppercase ? "A-Z" : null,
                p.UseLowercase ? "a-z" : null,
                p.UseDigits ? "0-9" : null,
                p.UseSymbols ? "sym" : null
            }.Where(s => s != null));

            string mins = $"U:{p.MinUppercase} L:{p.MinLowercase} D:{p.MinDigits} S:{p.MinSymbols}";
            string ambig = p.ExcludeAmbiguous ? "[grey](no ambig)[/]" : string.Empty;

            table.AddRow(
                $"[cyan]{Markup.Escape(key)}[/]",
                $"[grey]{Markup.Escape(p.Description)}[/]",
                $"[grey]{p.MinLength}–{p.MaxLength}[/]",
                $"[grey]{sets}[/] {ambig}",
                $"[grey]{mins}[/]"
            );
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void ShowSuccess(string message) =>
        AnsiConsole.MarkupLine($"[green]✔[/] {message}");

    public static void ShowError(string message) =>
        AnsiConsole.MarkupLine($"[red]✘[/] {Markup.Escape(message)}");

    public static void ShowSectionRule(string title) =>
        AnsiConsole.Write(new Rule($"[grey]{title}[/]").RuleStyle("grey").LeftJustified());
}
