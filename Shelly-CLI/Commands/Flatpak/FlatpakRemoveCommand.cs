using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakRemoveCommand : Command<FlatpakPackageSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] FlatpakPackageSettings settings)
    {
        var manager = new FlatpakManager();
        var result = manager.UninstallApp(settings.Packages);

        AnsiConsole.MarkupLine("[yellow]" + result.EscapeMarkup() + "[/]");
        return 0;
    }
}
