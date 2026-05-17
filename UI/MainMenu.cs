// Superseded by UI/MainWindow.cs.
// This file is intentionally empty.
namespace PTreeGold.UI;

internal static class MainMenu_Obsolete
{
#if false
    private readonly GenerateFlow _generateFlow;
    private readonly Dictionary<string, PasswordPattern> _patterns;

    public MainMenu(GenerateFlow generateFlow, Dictionary<string, PasswordPattern> patterns)
    {
        _generateFlow = generateFlow;
        _patterns = patterns;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            DisplayHelpers.ShowBanner();

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold gold1]Choose thy path:[/]")
                    .PageSize(6)
                    .HighlightStyle(Style.Parse("bold gold1"))
                    .AddChoices(
                        "(G)enerate Password",
                        "(L)ist Named Patterns",
                        "(Q)uit"));

            switch (choice)
            {
                case "(G)enerate Password":
                    await _generateFlow.RunAsync();
                    Pause();
                    break;

                case "(L)ist Named Patterns":
                    DisplayHelpers.ShowBanner();
                    DisplayHelpers.ShowPatternsTable(_patterns);
                    Pause();
                    break;

                case "(Q)uit":
                    AnsiConsole.MarkupLine("[gold1]Fare thee well, brave soul.[/]");
                    return;
            }
        }
    }

    private static void Pause()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(intercept: true);
    }
#endif
}
