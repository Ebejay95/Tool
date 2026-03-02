using Microsoft.JSInterop;
using MudBlazor;

namespace App.Services;

/// <summary>
/// Theme-Service für Blazor WASM.
/// Liest/Schreibt das Theme-Cookie per JS (kein IHttpContextAccessor verfügbar).
/// </summary>
public sealed class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private const string CookieName = "theme";
    private const int CookieDays = 365;

    private bool _isDarkMode = false;
    public bool IsDarkMode => _isDarkMode;

    public event Action? OnThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        // In WASM gibt es keinen synchronen HTTP-Context.
        // Theme wird asynchron via InitializeAsync aus dem Cookie geladen.
    }

    /// <summary>
    /// Lädt das Theme aus dem Cookie via JSInterop.
    /// Muss nach dem ersten Render aufgerufen werden (OnAfterRenderAsync).
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var value = await _jsRuntime.InvokeAsync<string?>("cookieHelper.get", CookieName);
            var isDark = value == "dark";
            if (isDark == _isDarkMode) return; // nichts geändert
            _isDarkMode = isDark;
            OnThemeChanged?.Invoke();
        }
        catch
        {
            // JSInterop nicht verfügbar – ignorieren
        }
    }

    public MudTheme Theme { get; } = new()
    {
        Palette = new Palette
        {
            // Markenfarben
            Primary           = "#ffffff",
            PrimaryDarken     = "#EBEBEB",
            PrimaryLighten    = "#6d9bff",
            PrimaryContrastText = "#FFFFFF",
            Secondary         = "#313131",
            SecondaryContrastText = "#FFFFFF",

            // AppBar
            AppbarBackground  = "#6d9bff",
            AppbarText        = "#FFFFFF",

            // Hintergründe
            Background        = "#F4F4F4",
            BackgroundGrey    = "#EBEBEB",
            Surface           = "#FFFFFF",
            DrawerBackground  = "#FFFFFF",
            DrawerText        = "#313131",
            DrawerIcon        = "#6d9bff",

            // Text
            TextPrimary       = "#1A1A1A",
            TextSecondary     = "#616161",
            TextDisabled      = "#BDBDBD",

            // Aktionen & Icons
            ActionDefault     = "#616161",
            ActionDisabled    = "#BDBDBD",
            ActionDisabledBackground = "#F5F5F5",

            // Linien & Divider
            Divider           = "#E0E0E0",
            DividerLight      = "#F0F0F0",
            TableLines        = "#E0E0E0",

            // Status
            Success           = "#2E7D32",
            SuccessContrastText = "#FFFFFF",
            Warning           = "#E65100",
            WarningContrastText = "#FFFFFF",
            Error             = "#C62828",
            ErrorContrastText  = "#FFFFFF",
            Info              = "#6d9bff",
            InfoContrastText  = "#FFFFFF",

            // Schatten / Overlay
            OverlayDark       = "rgba(0,0,0,0.5)",
            OverlayLight      = "rgba(255,255,255,0.6)",

            // Hover
            HoverOpacity      = 0.06,

            // Lines
            LinesDefault      = "rgba(0,0,0,0.12)",
            LinesInputs       = "rgba(0,0,0,0.42)",
        },

        PaletteDark = new PaletteDark
        {
            // Markenfarben
            Primary           = "#6d9bff",
            PrimaryDarken     = "#4c85ff",
            PrimaryLighten    = "#6d9bff",
            PrimaryContrastText = "#FFFFFF",
            Secondary         = "#E3E3E3",
            SecondaryContrastText = "#1A1A1A",

            // AppBar
            AppbarBackground  = "#1C1C2E",
            AppbarText        = "#E3E3E3",

            // Hintergründe
            Background        = "#121212",
            BackgroundGrey    = "#1A1A1A",
            Surface           = "#1E1E2E",
            DrawerBackground  = "#1C1C2E",
            DrawerText        = "#E3E3E3",
            DrawerIcon        = "#5C62D6",

            // Text
            TextPrimary       = "#E3E3E3",
            TextSecondary     = "#9E9E9E",
            TextDisabled      = "#616161",

            // Aktionen & Icons
            ActionDefault     = "#9E9E9E",
            ActionDisabled    = "#616161",
            ActionDisabledBackground = "#1E1E1E",

            // Linien & Divider
            Divider           = "#2E2E2E",
            DividerLight      = "#242424",
            TableLines        = "#2E2E2E",

            // Status
            Success           = "#43A047",
            SuccessContrastText = "#FFFFFF",
            Warning           = "#FB8C00",
            WarningContrastText = "#1A1A1A",
            Error             = "#EF5350",
            ErrorContrastText  = "#FFFFFF",
            Info              = "#4c85ff",
            InfoContrastText  = "#1A1A1A",

            // Schatten / Overlay
            OverlayDark       = "rgba(0,0,0,0.7)",
            OverlayLight      = "rgba(255,255,255,0.05)",

            // Hover
            HoverOpacity      = 0.08,
        },

        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Roboto", "Helvetica", "Arial", "sans-serif"],
                FontSize   = "0.875rem",
                FontWeight = 400,
                LineHeight = 1.5,
                LetterSpacing = "0.01071em",
            },
            H1 = new H1 { FontSize = "2.5rem",  FontWeight = 300 },
            H2 = new H2 { FontSize = "2rem",    FontWeight = 300 },
            H3 = new H3 { FontSize = "1.75rem", FontWeight = 400 },
            H4 = new H4 { FontSize = "1.5rem",  FontWeight = 400 },
            H5 = new H5 { FontSize = "1.25rem", FontWeight = 400 },
            H6 = new H6 { FontSize = "1rem",    FontWeight = 500 },
            Body1 = new Body1 { FontSize = "1rem",    FontWeight = 400 },
            Body2 = new Body2 { FontSize = "0.875rem",FontWeight = 400 },
            Button = new Button { FontSize = "0.875rem", FontWeight = 500, LetterSpacing = "0.02857em", TextTransform = "uppercase" },
            Caption = new Caption { FontSize = "0.75rem", FontWeight = 400 },
            Overline = new Overline { FontSize = "0.625rem", FontWeight = 400, LetterSpacing = "0.08333em" },
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft     = "280px",
            DrawerWidthRight    = "280px",
            AppbarHeight        = "64px",
        },
    };

    public async Task ToggleAsync()
    {
        _isDarkMode = !_isDarkMode;
        try
        {
            await _jsRuntime.InvokeVoidAsync("cookieHelper.set", CookieName, _isDarkMode ? "dark" : "light", CookieDays);
        }
        catch
        {
            // ignore if JS not available
        }
        OnThemeChanged?.Invoke();
    }
}
