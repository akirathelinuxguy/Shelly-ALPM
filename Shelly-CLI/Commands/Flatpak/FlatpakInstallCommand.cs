using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakInstallCommand : Command<FlatpakPackageSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] FlatpakPackageSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]Installing flatpak app...[/]");
        var manager = new FlatpakManager();
        var result = manager.InstallApp(settings.Packages);

        AnsiConsole.MarkupLine("[yellow]Installed: " + result.EscapeMarkup() + "[/]");

        return 0;
    }
}
