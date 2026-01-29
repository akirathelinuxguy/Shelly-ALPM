using System.Diagnostics.CodeAnalysis;
using PackageManager.Flatpak;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Flatpak;

public class FlatpakListCommand : Command
{
    public override int Execute([NotNull] CommandContext context)
    {
        var manager = new FlatpakManager();

        var packages = manager.SearchInstalled();

        var table = new Table();
        table.AddColumn("Name");
        table.AddColumn("Id");
        table.AddColumn("Version");
        table.AddColumn("Arch");
        table.AddColumn("Branch");
        table.AddColumn("Summary");

        foreach (var pkg in packages.OrderBy(p => p.Id))
        {
            table.AddRow(
                pkg.Name,
                pkg.Id,
                pkg.Version,
                pkg.Arch,
                pkg.Version,
                pkg.Summary.EscapeMarkup().Truncate(50)
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[blue]Total: packages[/]");
        return 0;
    }
}
