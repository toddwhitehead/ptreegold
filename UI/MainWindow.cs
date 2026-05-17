using System.Collections.ObjectModel;
using Terminal.Gui;
using PTreeGold.Models;
using PTreeGold.Services;

namespace PTreeGold.UI;

public class MainWindow : Window
{
    // ── Services ──────────────────────────────────────────────────
    private readonly Dictionary<string, PasswordPattern> _patterns;
    private readonly PasswordGenerator _generator;
    private readonly EntropyCalculator _entropyCalc;
    private readonly ExportService _exporter;

    // ── State ─────────────────────────────────────────────────────
    private readonly List<string> _patternKeys;
    private List<string> _passwords = [];
    private PasswordPattern? _activePattern;

    // ── Views ─────────────────────────────────────────────────────
    private readonly ListView _patternList;
    private readonly ListView _passwordList;
    private readonly TextField _countField;
    private readonly Label _settingsLabel;
    private readonly Label _statusMsgLabel;

    // Layout constants
    private const int LeftWidth  = 26;
    private const int SettingsH  = 13;   // Generator Settings frame height (incl. borders)

    public MainWindow(
        Dictionary<string, PasswordPattern> patterns,
        PasswordGenerator generator,
        EntropyCalculator entropyCalc,
        ExportService exporter)
    {
        _patterns    = patterns;
        _generator   = generator;
        _entropyCalc = entropyCalc;
        _exporter    = exporter;

        Title        = "PTREE Gold  ~~  Passwords for the cloud age with added retro happiness";
        ColorScheme  = AppColorScheme.Base;

        // Build ordered list: all config keys then sentinel for custom
        _patternKeys = [.. patterns.Keys, "custom"];

        // ── Menu bar ──────────────────────────────────────────────
        var menuBar = new MenuBar
        {
            ColorScheme = AppColorScheme.Panel,
            Menus = new MenuBarItem[]
            {
                new MenuBarItem("_Actions", new MenuItem[]
                {
                    new MenuItem("_Generate...",    "F2",  () => OpenGenerateDialog(),       null!, null!, Key.F2),
                    new MenuItem("_Copy Password",  "F3",  () => CopySelected(),             null!, null!, Key.F3),
                    new MenuItem("_Export to File", "F5",  () => OpenExportDialog(),         null!, null!, Key.F5),
                    null!,
                    new MenuItem("_Quit",           "F10", () => Application.RequestStop(), null!, null!, Key.F10),
                }, null!),
            },
        };

        // ── Left panel – Named Patterns ───────────────────────────
        var patternDisplayNames = _patternKeys
            .Select(k => k == "custom" ? "── Custom ──" : $" {k}")
            .ToList();

        _patternList = new ListView
        {
            X = 0, Y = 0,
            Width  = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = AppColorScheme.List,
        };
        _patternList.SetSource(new ObservableCollection<string>(patternDisplayNames));
        _patternList.SelectedItemChanged += (_, _) => UpdateInfo();
        _patternList.KeyDown += (_, e) =>
        {
            if (e.KeyCode == KeyCode.Enter)
            {
                OpenGenerateDialog();
                e.Handled = true;
            }
        };

        var leftFrame = new FrameView
        {
            Title  = $"Named Patterns ({patterns.Count})",
            X = 0, Y = 1,
            Width  = LeftWidth,
            Height = Dim.Fill()! - 1,
            ColorScheme = AppColorScheme.Panel,
        };
        leftFrame.Add(_patternList);

        // ── Right panel – Generated Passwords ────────────────────
        _passwordList = new ListView
        {
            X = 0, Y = 0,
            Width  = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = AppColorScheme.List,
        };

        var rightFrame = new FrameView
        {
            Title  = "Generated Passwords",
            X = LeftWidth, Y = 1,
            Width  = Dim.Fill(),
            Height = Dim.Fill()! - SettingsH - 2,   // leaves room for settingsFrame + statusBar
            ColorScheme = AppColorScheme.Panel,
        };
        rightFrame.Add(_passwordList);

        // ── Generator Settings frame ──────────────────────────────
        var countLabel = new Label
        {
            X = 1, Y = 0,
            Text = "Passwords to generate:",
        };
        _countField = new TextField
        {
            X = 23, Y = 0, Width = 6,
            Text = "5",
            ColorScheme = AppColorScheme.Dialog,
        };
        _settingsLabel = new Label
        {
            X = 1, Y = 2,
            Width  = Dim.Fill()! - 2,
            Height = 8,
            Text   = "",
        };
        _statusMsgLabel = new Label
        {
            X = 1, Y = 10,
            Width  = Dim.Fill()! - 2,
            Height = 1,
            Text   = "",
            ColorScheme = AppColorScheme.Info,
        };

        var settingsFrame = new FrameView
        {
            Title  = "Generator Settings",
            X = LeftWidth,
            Y = Pos.AnchorEnd(SettingsH + 1),  // just above the status bar
            Width  = Dim.Fill(),
            Height = SettingsH,
            ColorScheme = AppColorScheme.Panel,
        };
        settingsFrame.Add(countLabel, _countField, _settingsLabel, _statusMsgLabel);

        // ── Panel Tab navigation + focus highlight ────────────────
        _patternList.KeyDown += (_, e) =>
        {
            if (e.KeyCode == KeyCode.Tab)
            {
                _passwordList.SetFocus();
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.Tab | KeyCode.ShiftMask))
            {
                _countField.SetFocus();
                e.Handled = true;
            }
        };
        _patternList.HasFocusChanged += (_, _) =>
        {
            leftFrame.ColorScheme = _patternList.HasFocus ? AppColorScheme.PanelFocused : AppColorScheme.Panel;
            leftFrame.SetNeedsDraw();
        };

        _passwordList.KeyDown += (_, e) =>
        {
            if (e.KeyCode == KeyCode.Tab)
            {
                _countField.SetFocus();
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.Tab | KeyCode.ShiftMask))
            {
                _patternList.SetFocus();
                e.Handled = true;
            }
        };
        _passwordList.HasFocusChanged += (_, _) =>
        {
            rightFrame.ColorScheme = _passwordList.HasFocus ? AppColorScheme.PanelFocused : AppColorScheme.Panel;
            rightFrame.SetNeedsDraw();
        };

        _countField.KeyDown += (_, e) =>
        {
            if (e.KeyCode == KeyCode.Tab)
            {
                _patternList.SetFocus();
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.Tab | KeyCode.ShiftMask))
            {
                _passwordList.SetFocus();
                e.Handled = true;
            }
        };
        _countField.HasFocusChanged += (_, _) =>
        {
            settingsFrame.ColorScheme = _countField.HasFocus ? AppColorScheme.PanelFocused : AppColorScheme.Panel;
            settingsFrame.SetNeedsDraw();
        };

        // ── Status bar ────────────────────────────────────────────
        var statusBar = new StatusBar();
        statusBar.Add(new Shortcut(Key.F2,  "~F2~ Generate", () => OpenGenerateDialog(),       ""));
        statusBar.Add(new Shortcut(Key.F3,  "~F3~ Copy",     () => CopySelected(),             ""));
        statusBar.Add(new Shortcut(Key.F5,  "~F5~ Export",   () => OpenExportDialog(),         ""));
        statusBar.Add(new Shortcut(Key.F10, "~F10~ Quit",     () => Application.RequestStop(), ""));

        Add(menuBar, leftFrame, rightFrame, settingsFrame, statusBar);

        _patternList.SetFocus();
        UpdateInfo();
    }

    // ── Settings panel ────────────────────────────────────────────

    private void UpdateInfo()
    {
        var idx = _patternList.SelectedItem;
        if (idx < 0 || idx >= _patternKeys.Count) return;

        var key = _patternKeys[idx];

        if (key == "custom")
        {
            _settingsLabel.Text = " Press F2 to configure a custom pattern.";
            return;
        }

        if (!_patterns.TryGetValue(key, out var p)) return;

        var symbolLine = p.UseSymbols
            ? $"Yes  {(string.IsNullOrWhiteSpace(p.SymbolSet) ? "!@#$%^&*-_+=" : p.SymbolSet)}"
            : "No";

        const string descPrefix = " Description:      ";
        int labelW = _settingsLabel.Frame.Width > 0 ? _settingsLabel.Frame.Width : 60;
        string descLine = WrapValue(descPrefix, p.Description, labelW);

        _settingsLabel.Text =
            $"{descLine}\n" +
            $" Min length:       {p.MinLength}\n" +
            $" Max length:       {p.MaxLength}\n" +
            $" Uppercase (A-Z):  {(p.UseUppercase     ? "Yes" : "No")}\n" +
            $" Lowercase (a-z):  {(p.UseLowercase     ? "Yes" : "No")}\n" +
            $" Digits (0-9):     {(p.UseDigits        ? "Yes" : "No")}\n" +
            $" Symbols:          {symbolLine}\n" +
            $" Excl. ambiguous:  {(p.ExcludeAmbiguous ? "Yes" : "No")}";

        if (_passwords.Count > 0 && _activePattern != null)
        {
            var avg = _passwords.Average(pw => _entropyCalc.Calculate(pw, _activePattern).Bits);
            _statusMsgLabel.Text = $" Last batch: {_passwords.Count} pwd  Avg entropy: {avg:F0} bits";
        }
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static string WrapValue(string fieldLabel, string value, int labelWidth)
    {
        int indent = fieldLabel.Length;
        int firstLineChars = labelWidth - indent;
        if (firstLineChars <= 0 || value.Length <= firstLineChars)
            return fieldLabel + value;

        var sb = new System.Text.StringBuilder();
        sb.Append(fieldLabel);
        string pad = new string(' ', indent);
        int pos = 0;
        bool first = true;

        while (pos < value.Length)
        {
            int lineChars = first ? firstLineChars : (labelWidth - indent);
            first = false;

            if (pos > 0)
                sb.Append('\n').Append(pad);

            int end = Math.Min(pos + lineChars, value.Length);
            if (end < value.Length)
            {
                int lastSpace = value.LastIndexOf(' ', end - 1, end - pos);
                if (lastSpace > pos) end = lastSpace;
            }

            sb.Append(value, pos, end - pos);
            pos = end;
            while (pos < value.Length && value[pos] == ' ') pos++;
        }

        return sb.ToString();
    }

    // ── Generate ──────────────────────────────────────────────────

    private void OpenGenerateDialog()
    {
        var idx = _patternList.SelectedItem;
        if (idx < 0 || idx >= _patternKeys.Count) return;

        var key = _patternKeys[idx];
        PasswordPattern pattern;

        if (key == "custom")
        {
            var customDlg = new CustomPatternDialog();
            Application.Run(customDlg);
            if (customDlg.Cancelled) return;
            pattern = customDlg.BuildPattern();
        }
        else
        {
            if (!_patterns.TryGetValue(key, out pattern!)) return;
        }

        if (!int.TryParse(_countField.Text?.ToString(), out int count) || count < 1 || count > 50)
        {
            MessageBox.ErrorQuery("Invalid Count", "Passwords to generate must be 1–50.", "OK");
            return;
        }

        _activePattern = pattern;
        _passwords = Enumerable.Range(0, count)
            .Select(_ => _generator.Generate(pattern))
            .ToList();

        _statusMsgLabel.Text = "";
        RefreshPasswordList();
    }

    private void RefreshPasswordList()
    {
        if (_passwords.Count == 0) return;

        var display = _passwords
            .Select((pw, i) =>
            {
                var r = _activePattern != null
                    ? _entropyCalc.Calculate(pw, _activePattern)
                    : null;
                var entropy = r != null ? $"  {r.Bits:F0}b ({r.Label})" : "";
                return $" {i + 1,2}.  {pw}   {pw.Length}ch{entropy}";
            })
            .ToList();

        _passwordList.SetSource(new ObservableCollection<string>(display));
        _passwordList.SetFocus();
        UpdateInfo();
    }

    // ── Copy ──────────────────────────────────────────────────────

    private async void CopySelected()
    {
        if (_passwords.Count == 0)
        {
            MessageBox.Query("Nothing to Copy", "Generate passwords first (F2).", "OK");
            return;
        }

        var idx = _passwordList.SelectedItem;
        if (idx < 0 || idx >= _passwords.Count) idx = 0;

        await TextCopy.ClipboardService.SetTextAsync(_passwords[idx]);
        _statusMsgLabel.Text = $" ✓ Password #{idx + 1} copied to clipboard.";
    }

    // ── Export ────────────────────────────────────────────────────

    private void OpenExportDialog()
    {
        if (_passwords.Count == 0)
        {
            MessageBox.Query("Nothing to Export", "Generate passwords first (F2).", "OK");
            return;
        }

        var exportDlg = new ExportDialog(_passwords, _exporter, _statusMsgLabel);
        Application.Run(exportDlg);
    }
}
