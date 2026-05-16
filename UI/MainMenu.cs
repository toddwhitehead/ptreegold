using Spectre.Console;
using PwdGen.Models;
using PwdGen.Services;

namespace PwdGen.UI;

public class MainMenu
{
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
                    .Title("What would you like to do?")
                    .PageSize(6)
                    .HighlightStyle(Style.Parse("cyan"))
                    .AddChoices(
                        "Generate Password",
                        "List Named Patterns",
                        "Quit"));

            switch (choice)
            {
                case "Generate Password":
                    await _generateFlow.RunAsync();
                    Pause();
                    break;

                case "List Named Patterns":
                    DisplayHelpers.ShowBanner();
                    DisplayHelpers.ShowPatternsTable(_patterns);
                    Pause();
                    break;

                case "Quit":
                    AnsiConsole.MarkupLine("[grey]Goodbye.[/]");
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
}
