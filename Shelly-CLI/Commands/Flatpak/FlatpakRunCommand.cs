using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakRunCommand : Command<FlatpakPackageSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] FlatpakPackageSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]Running selected flatpak app...[/]");
        var result = new FlatpakManager().LaunchApp(settings.Packages);

        if (result)
        {
            AnsiConsole.MarkupLine("[green]App launched successfully[/]");
            return 0;
       }

        AnsiConsole.MarkupLine("[red]Failed to launch app[/]");
        return 1;
    }
}
