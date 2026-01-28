using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using PackageManager.Aur;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI;

public class Aur
{
    public sealed class AurSearchSettings : CommandSettings
    {
        [CommandArgument(0, "<query>")]
        [Description("Search term for AUR packages")]
        public string Query { get; init; } = string.Empty;
    }

    public class AurPackageSettings : CommandSettings
    {
        [CommandArgument(0, "<packages>")]
        [Description("Package name(s) to operate on (space-separated)")]
        public string[] Packages { get; set; } = [];

        [CommandOption("--no-confirm")]
        [Description("Skip confirmation prompts")]
        public bool NoConfirm { get; set; }
    }

    public class AurInstallVersionSettings : CommandSettings
    {
        [CommandArgument(0, "<package>")]
        [Description("Package name to install")]
        public string Package { get; set; } = string.Empty;

        [CommandArgument(1, "<commit>")]
        [Description("Git commit hash for the specific version")]
        public string Commit { get; set; } = string.Empty;
    }

    public class AurSearchCommand : Command<AurSearchSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurSearchSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Query))
            {
                AnsiConsole.MarkupLine("[red]Query cannot be empty.[/]");
                return 1;
            }

            try
            {
                var manager = new AurPackageManager();
                manager.Initialize().GetAwaiter().GetResult();

                var results = manager.SearchPackages(settings.Query).GetAwaiter().GetResult();

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Name");
                table.AddColumn("Version");
                table.AddColumn("Description");

                foreach (var pkg in results.Take(25))
                {
                    table.AddRow(
                        pkg.Name.EscapeMarkup(),
                        pkg.Version.EscapeMarkup(),
                        (pkg.Description ?? "").EscapeMarkup().Truncate(60)
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[blue]Total results:[/] {results.Count}");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Search failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurListInstalledCommand : Command
    {
        public override int Execute([NotNull] CommandContext context)
        {
            try
            {
                var manager = new AurPackageManager();
                manager.Initialize().GetAwaiter().GetResult();

                var packages = manager.GetInstalledPackages().GetAwaiter().GetResult();

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Name");
                table.AddColumn("Version");
                table.AddColumn("Description");

                foreach (var pkg in packages.OrderBy(p => p.Name))
                {
                    table.AddRow(
                        pkg.Name.EscapeMarkup(),
                        pkg.Version.EscapeMarkup(),
                        (pkg.Description ?? "").EscapeMarkup().Truncate(60)
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[blue]Total:[/] {packages.Count} AUR packages installed");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to list packages:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurListUpdatesCommand : Command
    {
        public override int Execute([NotNull] CommandContext context)
        {
            try
            {
                var manager = new AurPackageManager();
                manager.Initialize().GetAwaiter().GetResult();

                var updates = manager.GetPackagesNeedingUpdate().GetAwaiter().GetResult();

                if (updates.Count == 0)
                {
                    AnsiConsole.MarkupLine("[green]All AUR packages are up to date.[/]");
                    return 0;
                }

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Name");
                table.AddColumn("Installed");
                table.AddColumn("Available");
                table.AddColumn("Description");

                foreach (var pkg in updates.OrderBy(p => p.Name))
                {
                    table.AddRow(
                        pkg.Name.EscapeMarkup(),
                        pkg.Version.EscapeMarkup(),
                        pkg.NewVersion.EscapeMarkup(),
                        pkg.Description.EscapeMarkup().Truncate(50)
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[blue]Total:[/] {updates.Count} packages need updates");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Failed to check updates:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurInstallCommand : Command<AurPackageSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurPackageSettings settings)
        {
            if (settings.Packages.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No packages specified.[/]");
                return 1;
            }

            try
            {
                var manager = new AurPackageManager();
                manager.Initialize(root: true).GetAwaiter().GetResult();

                manager.PackageProgress += (sender, args) =>
                {
                    var statusColor = args.Status switch
                    {
                        PackageProgressStatus.Downloading => "yellow",
                        PackageProgressStatus.Building => "blue",
                        PackageProgressStatus.Installing => "cyan",
                        PackageProgressStatus.Completed => "green",
                        PackageProgressStatus.Failed => "red",
                        _ => "white"
                    };

                    AnsiConsole.MarkupLine(
                        $"[{statusColor}][[{args.CurrentIndex}/{args.TotalCount}]] {args.PackageName}: {args.Status}[/]" +
                        (args.Message != null ? $" - {args.Message.EscapeMarkup()}" : ""));
                };

                AnsiConsole.MarkupLine($"[yellow]Installing AUR packages: {string.Join(", ", settings.Packages)}[/]");
                manager.InstallPackages(settings.Packages.ToList()).GetAwaiter().GetResult();
                AnsiConsole.MarkupLine("[green]Installation complete.[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Installation failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurInstallVersionCommand : Command<AurInstallVersionSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurInstallVersionSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Package))
            {
                AnsiConsole.MarkupLine("[red]No package specified.[/]");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(settings.Commit))
            {
                AnsiConsole.MarkupLine("[red]No commit specified.[/]");
                return 1;
            }

            try
            {
                var manager = new AurPackageManager();
                manager.Initialize(root: true).GetAwaiter().GetResult();

                manager.PackageProgress += (sender, args) =>
                {
                    var statusColor = args.Status switch
                    {
                        PackageProgressStatus.Downloading => "yellow",
                        PackageProgressStatus.Building => "blue",
                        PackageProgressStatus.Installing => "cyan",
                        PackageProgressStatus.Completed => "green",
                        PackageProgressStatus.Failed => "red",
                        _ => "white"
                    };

                    AnsiConsole.MarkupLine(
                        $"[{statusColor}][[{args.CurrentIndex}/{args.TotalCount}]] {args.PackageName}: {args.Status}[/]" +
                        (args.Message != null ? $" - {args.Message.EscapeMarkup()}" : ""));
                };

                AnsiConsole.MarkupLine(
                    $"[yellow]Installing AUR package {settings.Package} at commit {settings.Commit}[/]");
                manager.InstallPackageVersion(settings.Package, settings.Commit).GetAwaiter().GetResult();
                AnsiConsole.MarkupLine("[green]Installation complete.[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Installation failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurUpdateCommand : Command<AurPackageSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurPackageSettings settings)
        {
            if (settings.Packages.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No packages specified.[/]");
                return 1;
            }

            try
            {
                var manager = new AurPackageManager();
                manager.Initialize(root: true).GetAwaiter().GetResult();

                manager.PackageProgress += (sender, args) =>
                {
                    var statusColor = args.Status switch
                    {
                        PackageProgressStatus.Downloading => "yellow",
                        PackageProgressStatus.Building => "blue",
                        PackageProgressStatus.Installing => "cyan",
                        PackageProgressStatus.Completed => "green",
                        PackageProgressStatus.Failed => "red",
                        _ => "white"
                    };

                    AnsiConsole.MarkupLine(
                        $"[{statusColor}][[{args.CurrentIndex}/{args.TotalCount}]] {args.PackageName}: {args.Status}[/]" +
                        (args.Message != null ? $" - {args.Message.EscapeMarkup()}" : ""));
                };

                manager.PkgbuildDiffRequest += (sender, args) =>
                {
                    if (settings.NoConfirm)
                    {
                        args.ProceedWithUpdate = true;
                        return;
                    }

                    var showDiff = AnsiConsole.Confirm(
                        $"[yellow]PKGBUILD changed for {args.PackageName}. View diff?[/]", defaultValue: false);

                    if (showDiff)
                    {
                        AnsiConsole.MarkupLine("[blue]--- Old PKGBUILD ---[/]");
                        AnsiConsole.WriteLine(args.OldPkgbuild);
                        AnsiConsole.MarkupLine("[blue]--- New PKGBUILD ---[/]");
                        AnsiConsole.WriteLine(args.NewPkgbuild);
                    }

                    args.ProceedWithUpdate = AnsiConsole.Confirm(
                        $"[yellow]Proceed with update for {args.PackageName}?[/]", defaultValue: true);
                };

                AnsiConsole.MarkupLine($"[yellow]Updating AUR packages: {string.Join(", ", settings.Packages)}[/]");
                manager.UpdatePackages(settings.Packages.ToList()).GetAwaiter().GetResult();
                AnsiConsole.MarkupLine("[green]Update complete.[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Update failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurRemoveCommand : Command<AurPackageSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurPackageSettings settings)
        {
            if (settings.Packages.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]No packages specified.[/]");
                return 1;
            }

            try
            {
                var manager = new AurPackageManager();
                manager.Initialize(root: true).GetAwaiter().GetResult();

                AnsiConsole.MarkupLine($"[yellow]Removing AUR packages: {string.Join(", ", settings.Packages)}[/]");
                manager.RemovePackages(settings.Packages.ToList()).GetAwaiter().GetResult();
                AnsiConsole.MarkupLine("[green]Removal complete.[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Removal failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }

    public class AurUpgradeSettings : CommandSettings
    {
        [CommandOption("--no-confirm")]
        [Description("Skip confirmation prompts")]
        public bool NoConfirm { get; set; }
    }

    public class AurUpgradeCommand : Command<AurUpgradeSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] AurUpgradeSettings settings)
        {
            try
            {
                var manager = new AurPackageManager();
                manager.Initialize(root: true).GetAwaiter().GetResult();

                var updates = manager.GetPackagesNeedingUpdate().GetAwaiter().GetResult();

                if (updates.Count == 0)
                {
                    AnsiConsole.MarkupLine("[green]All AUR packages are up to date.[/]");
                    return 0;
                }

                AnsiConsole.MarkupLine($"[yellow]{updates.Count} AUR packages need updates:[/]");
                foreach (var pkg in updates)
                {
                    AnsiConsole.MarkupLine($"  {pkg.Name}: {pkg.Version} -> {pkg.NewVersion}");
                }

                if (!settings.NoConfirm)
                {
                    if (!AnsiConsole.Confirm("[yellow]Proceed with upgrade?[/]", defaultValue: true))
                    {
                        AnsiConsole.MarkupLine("[yellow]Upgrade cancelled.[/]");
                        return 0;
                    }
                }

                manager.PackageProgress += (sender, args) =>
                {
                    var statusColor = args.Status switch
                    {
                        PackageProgressStatus.Downloading => "yellow",
                        PackageProgressStatus.Building => "blue",
                        PackageProgressStatus.Installing => "cyan",
                        PackageProgressStatus.Completed => "green",
                        PackageProgressStatus.Failed => "red",
                        _ => "white"
                    };

                    AnsiConsole.MarkupLine(
                        $"[{statusColor}][[{args.CurrentIndex}/{args.TotalCount}]] {args.PackageName}: {args.Status}[/]" +
                        (args.Message != null ? $" - {args.Message.EscapeMarkup()}" : ""));
                };

                manager.PkgbuildDiffRequest += (sender, args) =>
                {
                    if (settings.NoConfirm)
                    {
                        args.ProceedWithUpdate = true;
                        return;
                    }

                    var showDiff = AnsiConsole.Confirm(
                        $"[yellow]PKGBUILD changed for {args.PackageName}. View diff?[/]", defaultValue: false);

                    if (showDiff)
                    {
                        AnsiConsole.MarkupLine("[blue]--- Old PKGBUILD ---[/]");
                        AnsiConsole.WriteLine(args.OldPkgbuild);
                        AnsiConsole.MarkupLine("[blue]--- New PKGBUILD ---[/]");
                        AnsiConsole.WriteLine(args.NewPkgbuild);
                    }

                    args.ProceedWithUpdate = AnsiConsole.Confirm(
                        $"[yellow]Proceed with update for {args.PackageName}?[/]", defaultValue: true);
                };

                var packageNames = updates.Select(u => u.Name).ToList();
                manager.UpdatePackages(packageNames).GetAwaiter().GetResult();
                AnsiConsole.MarkupLine("[green]Upgrade complete.[/]");

                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Upgrade failed:[/] {ex.Message.EscapeMarkup()}");
                return 1;
            }
        }
    }
}