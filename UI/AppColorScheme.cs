using Terminal.Gui;
using TAttr = Terminal.Gui.Attribute;

namespace PTreeGold.UI;

/// <summary>
/// XTree Gold-inspired color palette: white/cyan on dark blue, yellow highlights.
/// </summary>
internal static class AppColorScheme
{
    /// <summary>Main window background (blue canvas).</summary>
    public static ColorScheme Base { get; } = new()
    {
        Normal    = new TAttr(Color.White,        Color.Blue),
        Focus     = new TAttr(Color.BrightYellow, Color.Blue),
        HotNormal = new TAttr(Color.BrightYellow, Color.Blue),
        HotFocus  = new TAttr(Color.BrightYellow, Color.Blue),
        Disabled  = new TAttr(Color.Gray,         Color.Blue),
    };

    /// <summary>FrameView borders and titles.</summary>
    public static ColorScheme Panel { get; } = new()
    {
        Normal    = new TAttr(Color.BrightCyan,   Color.Blue),
        Focus     = new TAttr(Color.BrightYellow, Color.Blue),
        HotNormal = new TAttr(Color.BrightYellow, Color.Blue),
        HotFocus  = new TAttr(Color.BrightYellow, Color.Blue),
        Disabled  = new TAttr(Color.Gray,         Color.Blue),
    };

    /// <summary>ListView items (white on blue, yellow-on-blue selected).</summary>
    public static ColorScheme List { get; } = new()
    {
        Normal    = new TAttr(Color.White,        Color.Blue),
        Focus     = new TAttr(Color.Black,        Color.BrightYellow),
        HotNormal = new TAttr(Color.BrightCyan,   Color.Blue),
        HotFocus  = new TAttr(Color.Black,        Color.BrightYellow),
        Disabled  = new TAttr(Color.Gray,         Color.Blue),
    };

    /// <summary>Info strip below the password list.</summary>
    public static ColorScheme Info { get; } = new()
    {
        Normal    = new TAttr(Color.BrightCyan,   Color.Blue),
        Focus     = new TAttr(Color.BrightCyan,   Color.Blue),
        HotNormal = new TAttr(Color.BrightCyan,   Color.Blue),
        HotFocus  = new TAttr(Color.BrightCyan,   Color.Blue),
        Disabled  = new TAttr(Color.Gray,         Color.Blue),
    };

    /// <summary>FrameView border/title when its panel has keyboard focus.</summary>
    public static ColorScheme PanelFocused { get; } = new()
    {
        Normal    = new TAttr(Color.White, Color.Blue),
        Focus     = new TAttr(Color.White, Color.Blue),
        HotNormal = new TAttr(Color.White, Color.Blue),
        HotFocus  = new TAttr(Color.White, Color.Blue),
        Disabled  = new TAttr(Color.Gray,  Color.Blue),
    };

    /// <summary>Dialog windows.</summary>
    public static ColorScheme Dialog { get; } = new()
    {
        Normal    = new TAttr(Color.White,        Color.DarkGray),
        Focus     = new TAttr(Color.Black,        Color.BrightYellow),
        HotNormal = new TAttr(Color.BrightYellow, Color.DarkGray),
        HotFocus  = new TAttr(Color.Black,        Color.BrightYellow),
        Disabled  = new TAttr(Color.Gray,         Color.DarkGray),
    };
}

