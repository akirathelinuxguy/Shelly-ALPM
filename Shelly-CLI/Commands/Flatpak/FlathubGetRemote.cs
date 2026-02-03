using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlathubGetRemote : Command<DefaultSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] DefaultSettings settings)
    {
        var result = new FlatpakManager().GetAvailableAppsFromAppstreamJson("flathub");

        using var stdout = Console.OpenStandardOutput();
        using var writer = new System.IO.StreamWriter(stdout, System.Text.Encoding.UTF8);
        writer.WriteLine(result);
        writer.Flush();
        return 0;
    }
}