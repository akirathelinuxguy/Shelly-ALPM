using System.ComponentModel;
using Spectre.Console.Cli;

namespace Shelly_CLI.Commands.Aur;

public class AurInstallSettings : AurPackageSettings
{
    [CommandOption("-o|--build-deps")]
    [Description("Install build dependencies only for the specified AUR packages")]
    public bool BuildDepsOn { get; set; }
    
    [CommandOption("-m|--make-deps")]
    [Description("Install make dependencies only for the specified AUR packages")]
    public bool MakeDepsOn { get; set; }
}
