using Terminal.Gui;
using PTreeGold.Models;
using PTreeGold.Services;

namespace PTreeGold.UI;

// ── GenerateDialog ────────────────────────────────────────────────────────────
// Asks how many passwords to generate, then runs the generator.

public class GenerateDialog : Dialog
{
    public bool Cancelled { get; private set; } = true;
    public List<string> Passwords { get; private set; } = [];

    public GenerateDialog(
        string patternName,
        PasswordPattern pattern,
        PasswordGenerator generator,
        EntropyCalculator entropyCalc)
    {
        Title        = $" Generate — {patternName} ";
        Width        = 52;
        Height       = 14;
        ColorScheme  = AppColorScheme.Dialog;

        // Description row
        Add(new Label
        {
            X    = 1, Y = 1,
            Text = $"{pattern.Description}",
            ColorScheme = AppColorScheme.Info,
        });

        // Length / charset summary
        Add(new Label
        {
            X    = 1, Y = 2,
            Text = $"Length {pattern.MinLength}-{pattern.MaxLength}" +
                   (pattern.ExcludeAmbiguous ? "  no-ambiguous" : ""),
        });

        // Count prompt
        Add(new Label { X = 1, Y = 4, Text = "Number of passwords (1-50):" });
        var countField = new TextField
        {
            X = 30, Y = 4, Width = 8,
            Text = "5",
            ColorScheme = AppColorScheme.Dialog,
        };
        Add(countField);

        // Buttons
        var btnGenerate = new Button { Text = "_Generate", X = 2,  Y = 7, IsDefault = true };
        var btnCancel   = new Button { Text = "_Cancel",   X = 16, Y = 7 };

        btnGenerate.Accepting += (_, _) =>
        {
            if (!int.TryParse(countField.Text.ToString(), out int count)
                || count < 1 || count > 50)
            {
                MessageBox.ErrorQuery("Invalid Input", "Enter a number between 1 and 50.", "OK");
                return;
            }

            Passwords = Enumerable.Range(0, count)
                .Select(_ => generator.Generate(pattern))
                .ToList();

            Cancelled = false;
            Application.RequestStop();
        };

        btnCancel.Accepting += (_, _) => Application.RequestStop();

        Add(btnGenerate, btnCancel);

        countField.SetFocus();
    }
}

// ── CustomPatternDialog ───────────────────────────────────────────────────────
// Lets the user build a PasswordPattern from scratch.

public class CustomPatternDialog : Dialog
{
    public bool Cancelled { get; private set; } = true;

    private readonly TextField  _minLen;
    private readonly TextField  _maxLen;
    private readonly CheckBox   _chkUpper;
    private readonly CheckBox   _chkLower;
    private readonly CheckBox   _chkDigits;
    private readonly CheckBox   _chkSymbols;
    private readonly TextField  _symbolSet;
    private readonly CheckBox   _chkAmbiguous;

    public CustomPatternDialog()
    {
        Title       = " Custom Pattern ";
        Width       = 56;
        Height      = 20;
        ColorScheme = AppColorScheme.Dialog;

        // Length
        Add(new Label { X = 1, Y = 1, Text = "Min length:" });
        _minLen = new TextField { X = 20, Y = 1, Width = 6, Text = "16", ColorScheme = AppColorScheme.Dialog };

        Add(new Label { X = 1, Y = 2, Text = "Max length:" });
        _maxLen = new TextField { X = 20, Y = 2, Width = 6, Text = "24", ColorScheme = AppColorScheme.Dialog };

        Add(_minLen, _maxLen);

        // Character sets
        Add(new Label { X = 1, Y = 4, Text = "Character sets:" });

        _chkUpper    = new CheckBox { Text = "_Uppercase (A-Z)",             X = 3, Y = 5,  CheckedState = CheckState.Checked };
        _chkLower    = new CheckBox { Text = "_Lowercase (a-z)",             X = 3, Y = 6,  CheckedState = CheckState.Checked };
        _chkDigits   = new CheckBox { Text = "_Digits (0-9)",                X = 3, Y = 7,  CheckedState = CheckState.Checked };
        _chkSymbols  = new CheckBox { Text = "_Symbols",                     X = 3, Y = 8,  CheckedState = CheckState.Checked };
        Add(_chkUpper, _chkLower, _chkDigits, _chkSymbols);

        Add(new Label { X = 5, Y = 9, Text = "Symbol set:" });
        _symbolSet = new TextField { X = 18, Y = 9, Width = 18, Text = "!@#$%^&*-_+=", ColorScheme = AppColorScheme.Dialog };
        Add(_symbolSet);

        // Ambiguous
        _chkAmbiguous = new CheckBox { Text = "E_xclude ambiguous chars (0Ol1I)", X = 3, Y = 11, CheckedState = CheckState.Checked };
        Add(_chkAmbiguous);

        // Buttons
        var btnOk     = new Button { Text = "_OK",     X = 3,  Y = 14, IsDefault = true };
        var btnCancel = new Button { Text = "_Cancel", X = 14, Y = 14 };

        btnOk.Accepting     += (_, _) => { Cancelled = false; Application.RequestStop(); };
        btnCancel.Accepting += (_, _) => Application.RequestStop();

        Add(btnOk, btnCancel);
        _minLen.SetFocus();
    }

    public PasswordPattern BuildPattern() => new()
    {
        Description     = "Custom",
        MinLength       = int.TryParse(_minLen.Text.ToString(),  out int min) ? min : 16,
        MaxLength       = int.TryParse(_maxLen.Text.ToString(),  out int max) ? max : 24,
        UseUppercase    = _chkUpper.CheckedState    == CheckState.Checked,
        UseLowercase    = _chkLower.CheckedState    == CheckState.Checked,
        UseDigits       = _chkDigits.CheckedState   == CheckState.Checked,
        UseSymbols      = _chkSymbols.CheckedState  == CheckState.Checked,
        SymbolSet       = _symbolSet.Text?.ToString() ?? "!@#$%^&*-_+=",
        ExcludeAmbiguous = _chkAmbiguous.CheckedState == CheckState.Checked,
    };
}

// ── ExportDialog ──────────────────────────────────────────────────────────────
// Simple dialog with a path field; async export fires when confirmed.

public class ExportDialog : Dialog
{
    public ExportDialog(List<string> passwords, ExportService exporter, Label infoLabel)
    {
        Title       = " Export Passwords ";
        Width       = 62;
        Height      = 11;
        ColorScheme = AppColorScheme.Dialog;

        var defaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $"passwords_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        Add(new Label
        {
            X    = 1, Y = 1,
            Text = $"Exporting {passwords.Count} password(s) to:",
        });

        var pathField = new TextField
        {
            X = 1, Y = 3, Width = Dim.Fill()! - 2,
            Text = defaultPath,
            ColorScheme = AppColorScheme.Dialog,
        };
        Add(pathField);

        var btnExport = new Button { Text = "_Export", X = 3,  Y = 6, IsDefault = true };
        var btnCancel = new Button { Text = "_Cancel", X = 16, Y = 6 };

        btnExport.Accepting += async (_, _) =>
        {
            var path = pathField.Text?.ToString() ?? defaultPath;
            try
            {
                await exporter.ExportAsync(passwords, path);
                Application.RequestStop();
                infoLabel.Text = $" \u2713 Exported {passwords.Count} password(s) to {path}";
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Export Failed", ex.Message, "OK");
            }
        };

        btnCancel.Accepting += (_, _) => Application.RequestStop();

        Add(btnExport, btnCancel);
        pathField.SetFocus();
    }
}
