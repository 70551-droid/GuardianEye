using System.Runtime.InteropServices;

namespace GuardianEye.Shared;

public static class UIStyles
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMSBT_MAINWINDOW = 2;
    private const int DWMWCP_ROUND = 2;

    public static void EnableMica(IntPtr hwnd)
    {
        int backdrop = DWMSBT_MAINWINDOW;
        DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
    }

    public static void EnableAcrylic(IntPtr hwnd)
    {
        int backdrop = 3; // DWMSBT_ACRYLIC
        DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
    }

    public static void EnableRoundedCorners(IntPtr hwnd)
    {
        int corner = DWMWCP_ROUND;
        DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
    }

    public static void EnableDarkTitleBar(IntPtr hwnd)
    {
        int dark = 1;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
    }

    public static readonly Color GlassBg = Color.FromArgb(185, 12, 12, 28);
    public static readonly Color GlassCard = Color.FromArgb(200, 22, 22, 42);
    public static readonly Color GlassBorder = Color.FromArgb(35, 255, 255, 255);
    public static readonly Color AccentBlue = Color.FromArgb(79, 195, 247);
    public static readonly Color AccentPurple = Color.FromArgb(179, 136, 255);
    public static readonly Color AccentGreen = Color.FromArgb(105, 240, 174);
    public static readonly Color AccentRed = Color.FromArgb(255, 107, 107);
    public static readonly Color AccentAmber = Color.FromArgb(255, 213, 79);
    public static readonly Color TextPrimary = Color.FromArgb(230, 230, 235);
    public static readonly Color TextMuted = Color.FromArgb(140, 140, 155);
    public static readonly Color DeepBg = Color.FromArgb(8, 8, 20);
    public static readonly Color MidBg = Color.FromArgb(16, 16, 36);

    public static void StyleGunaButton(dynamic btn, Color accent)
    {
        btn.FillColor = accent;
        btn.FillColor2 = Color.FromArgb(accent.A - 30, accent.R, accent.G, accent.B);
        btn.HoverState.FillColor = Color.FromArgb(accent.A, Math.Min(255, accent.R + 30), Math.Min(255, accent.G + 30), Math.Min(255, accent.B + 30));
        btn.ForeColor = Color.White;
        btn.BorderRadius = 8;
        btn.BorderThickness = 0;
    }

    public static void StyleGunaInput(dynamic input)
    {
        input.FillColor = Color.FromArgb(40, 255, 255, 255);
        input.BorderColor = Color.FromArgb(50, 255, 255, 255);
        input.FocusedBorderColor = AccentBlue;
        input.ForeColor = TextPrimary;
        input.PlaceholderForeColor = TextMuted;
        input.BorderRadius = 8;
        input.BorderThickness = 1;
    }
}
