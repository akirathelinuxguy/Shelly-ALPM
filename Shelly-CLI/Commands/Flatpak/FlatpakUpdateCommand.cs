using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakUpdateCommand : Command<FlatpakPackageSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] FlatpakPackageSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]Updating flatpak app...[/]");
        var manager = new FlatpakManager();
        var result = manager.UpdateApp(settings.Packages);

        AnsiConsole.MarkupLine("[yellow]" + result.EscapeMarkup() + "[/]");

        return 0;
    }
}
