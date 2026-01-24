using System;
using System.Diagnostics;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shelly_UI.ViewModels;

namespace Shelly_UI.Views;

public partial class SettingWindow : ReactiveUserControl<SettingViewModel>
{
    public SettingWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OpenUrlCrossPlatform(object? sender, RoutedEventArgs routedEventArgs)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = "https://buymeacoffee.com/zoeyerinba3",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }
}