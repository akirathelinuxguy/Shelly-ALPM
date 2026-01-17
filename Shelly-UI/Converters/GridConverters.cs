using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Shelly_UI.Services;

namespace Shelly_UI.Converters;

public class BottomPanelHeightConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool and true)
        {
            return new ConfigService().LoadConfig().ConsoleEnabled
                ? new Avalonia.Controls.GridLength(10)
                : new Avalonia.Controls.GridLength(0);
        }

        return new Avalonia.Controls.GridLength(150, Avalonia.Controls.GridUnitType.Pixel);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}