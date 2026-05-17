// Superseded by UI/AppColorScheme.cs and Terminal.Gui views.
// This file is intentionally empty.
namespace PTreeGold.UI;

#pragma warning disable CS0219 // unused
internal static class DisplayHelpers_Obsolete
{
#if false
    public static void ShowBanner()
    {
        if (AnsiConsole.Profile.Capabilities.Ansi)
            AnsiConsole.Clear();

        AnsiConsole.WriteLine();

        // ── Title ───────────────────────────────────────────────────────────
        var figlet = new FigletText("PwdGen")
            .Centered()
            .Color(Color.Gold1);

        // ── Ankh (symbol of the Avatar / the passphrase bearer) ─────────────
        var ankh = new Markup(
            "\n" +
            "[gold1]                                ___[/]\n" +
            "[gold1]                               /   \\[/]\n" +
            "[gold1]                              (     )[/]\n" +
            "[gold1]                               \\   /[/]\n" +
            "[gold1]                                \\_/[/]\n" +
            "[gold1]                                 |[/]\n" +
            "[gold1]                               --+--[/]\n" +
            "[gold1]                                 |[/]\n");

        // ── Tagline & footer ────────────────────────────────────────────────
        var subtitle = new Markup(
            "\n" +
            "[bold yellow]                   ~~ Quest of the Passphrase ~~[/]\n" +
            "[grey]                      Enterprise Password Forge[/]\n" +
            "\n" +
            "[grey dim]                   (C) 2026  Origin Passwords, Inc.[/]\n");

        var panel = new Panel(new Rows(new IRenderable[] { figlet, ankh, subtitle }))
            .Border(BoxBorder.Double)
            .BorderColor(Color.Gold1)
            .Padding(1, 0)
            .Expand();

        AnsiConsole.Write(panel);
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
#endif
}
