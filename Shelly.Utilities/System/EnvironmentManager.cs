using System.Diagnostics;
using Shelly.Utilities.Extensions;
using Shelly.Utilities.System.Enums;

namespace Shelly.Utilities.System;

public static class EnvironmentManager
{
    private const string DesktopEnvironmentVariable = "XDG_CURRENT_DESKTOP";

    public static string CreateWindowManagerVars()
    {
        return GetDesktopEnvironment() switch
        {
            SupportedDesktopEnvironments.KDE or SupportedDesktopEnvironments.GNOME or SupportedDesktopEnvironments.XFCE
                or SupportedDesktopEnvironments.Cinnamon or SupportedDesktopEnvironments.MATE
                or SupportedDesktopEnvironments.LXQt or SupportedDesktopEnvironments.LXDE
                or SupportedDesktopEnvironments.Budgie or SupportedDesktopEnvironments.Pantheon
                or SupportedDesktopEnvironments.COSMIC => "",
            SupportedDesktopEnvironments.Hyprland or SupportedDesktopEnvironments.Sway
                or SupportedDesktopEnvironments.Niri or SupportedDesktopEnvironments.i3
                or SupportedDesktopEnvironments.Unknown => CreateWMLaunchVars(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static string UserPath
    {
        get
        {
            // If running via pkexec, get original user's home
            var pkexecUid = Environment.GetEnvironmentVariable("PKEXEC_UID");
            if (pkexecUid != null)
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "getent",
                    Arguments = $"passwd {pkexecUid}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });
                process?.WaitForExit();
                var output = process?.StandardOutput.ReadLine();
                var home = output?.Split(':')[5];
                if (!string.IsNullOrEmpty(home)) return home;
            }
        
            return Environment.GetEnvironmentVariable("HOME") 
                   ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }

    private static string CreateWMLaunchVars()
    {
        List<string> convertedVars = [];
        var envVars = EnumExtensions.ToNameList<WindowManagerEnvVariables>();
        convertedVars.AddRange(from envVar in envVars
            let value = Environment.GetEnvironmentVariable(envVar)
            where !string.IsNullOrEmpty(value)
            select $"{envVar}={value}");

        return convertedVars.Count > 0 ? $" {string.Join(" ", convertedVars)} " : "";
    }

    public static SupportedDesktopEnvironments GetDesktopEnvironment() =>
        Enum.TryParse<SupportedDesktopEnvironments>(Environment.GetEnvironmentVariable(DesktopEnvironmentVariable),
            true, out var result)
            ? result
            : SupportedDesktopEnvironments
                .Unknown;
}