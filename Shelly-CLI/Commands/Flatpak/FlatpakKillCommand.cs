using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakKillCommand : Command<FlatpakPackageSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] FlatpakPackageSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]Killing selected flatpak app...[/]");
        var result = new FlatpakManager().KillApp(settings.Packages);

        AnsiConsole.MarkupLine("[red]" + result + "[/]");

        return 0;
    }
}
