using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PackageManager.Alpm;
using PackageManager.Aur;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Aur;

public class AurInstallCommand : AsyncCommand<AurInstallSettings>
{
    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] AurInstallSettings settings)
    {
        AurPackageManager? manager = null;
        if (settings.Packages.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No packages specified.[/]");
            return 1;
        }

        var packageList = settings.Packages.ToList();

        AnsiConsole.MarkupLine($"[yellow]AUR packages to install:[/] {string.Join(", ", packageList)}");

        if (!settings.NoConfirm)
        {
            if (!AnsiConsole.Confirm("Do you want to proceed?"))
            {
                AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
                return 0;
            }
        }

        try
        {
            manager = new AurPackageManager();
            await manager.Initialize(root: true);

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

            manager.Progress += (sender, args) =>
            {
                AnsiConsole.MarkupLine($"[blue]{args.PackageName}[/]: {args.Percent}%");
            };

            manager.Question += (sender, args) =>
            {
                // Handle SelectProvider differently - it needs a selection, not yes/no
                if (args.QuestionType == AlpmQuestionType.SelectProvider && args.ProviderOptions?.Count > 0)
                {
                    if (settings.NoConfirm)
                    {
                        // Machine-readable format for UI integration
                        Console.Error.WriteLine($"[Shelly][ALPM_SELECT_PROVIDER]{args.DependencyName}");
                        for (int i = 0; i < args.ProviderOptions.Count; i++)
                        {
                            Console.Error.WriteLine($"[Shelly][ALPM_PROVIDER_OPTION]{i}:{args.ProviderOptions[i]}");
                        }

                        Console.Error.Flush();
                        var input = Console.ReadLine();
                        args.Response = int.TryParse(input?.Trim(), out var idx) ? idx : 0;
                    }
                    else
                    {
                        var selection = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title($"[yellow]{args.QuestionText}[/]")
                                .AddChoices(args.ProviderOptions));
                        args.Response = args.ProviderOptions.IndexOf(selection);
                    }
                }
                else if (settings.NoConfirm)
                {
                    // Machine-readable format for UI integration
                    Console.Error.WriteLine($"[Shelly][ALPM_QUESTION]{args.QuestionText}");
                    Console.Error.Flush();
                    var input = Console.ReadLine();
                    args.Response = input?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0;
                }
                else
                {
                    var response = AnsiConsole.Confirm($"[yellow]{args.QuestionText}[/]", defaultValue: true);
                    args.Response = response ? 1 : 0;
                }
            };

            if (settings.BuildDepsOn)
            {
                if (settings.Packages.Length > 1)
                {
                    AnsiConsole.MarkupLine("[yellow]Cannot build dependencies for multiple packages at once.[/]");
                    return 0;
                }

                if (settings.MakeDepsOn)
                {
                    AnsiConsole.MarkupLine("[yellow]Installing dependencies (including make dependencies)...[/]");
                    await manager.InstallDependenciesOnly(packageList.First(), true);
                    AnsiConsole.MarkupLine("[green]Dependencies installed successfully![/]");
                    return 0;
                }

                AnsiConsole.MarkupLine("[yellow]Installing dependencies...[/]");
                await manager.InstallDependenciesOnly(packageList.First(), false);
                AnsiConsole.MarkupLine("[green]Dependencies installed successfully![/]");
                return 0;
            }

            AnsiConsole.MarkupLine($"[yellow]Installing AUR packages: {string.Join(", ", settings.Packages)}[/]");
            await manager.InstallPackages(packageList);
            AnsiConsole.MarkupLine("[green]Installation complete.[/]");

           return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Installation failed:[/] {ex.Message.EscapeMarkup()}");
            return 1;
        }
        finally
        {
            manager?.Dispose();
        }
    }
}
