using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Shelly_UI.Services;

public class ThemeService
{
    public void ApplyCustomAccent(Color accent)
    {
        var fluentTheme = Application.Current?.Styles.OfType<FluentTheme>().FirstOrDefault();
        if (fluentTheme != null)
        {
            if (fluentTheme.Palettes.TryGetValue(ThemeVariant.Dark, out var currentDark) &&
                currentDark is { } darkPalette)
            {
                darkPalette.Accent = accent;
                fluentTheme.Palettes[ThemeVariant.Dark] = darkPalette;
            }

            if (fluentTheme.Palettes.TryGetValue(ThemeVariant.Light, out var currentLight) &&
                currentLight is { } lightPalette)
            {
                lightPalette.Accent = accent;
                fluentTheme.Palettes[ThemeVariant.Light] = lightPalette;
            }
        }
    }

    public void SetTheme(bool isDark)
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }
}