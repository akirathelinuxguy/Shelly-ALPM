# Shelly: A Visual Arch Package Manager

![Shelly Wiki](https://img.shields.io/badge/Shelly-Wiki-blue)

Shelly is a modern, visual package manager for Arch Linux, built with .NET 10 and Avalonia. It provides a user-friendly
interface for managing your Arch Linux system's packages by leveraging the power of `libalpm`.

## Features

- **Native Arch Integration**: Directly interacts with `libalpm` for accurate and fast package management.
- **Modern UI Framework**: Built using [Avalonia UI](https://avaloniaui.net/), ensuring a modern and responsive
  user interface.
- **Package Management**: Supports searching for, installing, updating, and removing packages.
- **Repository Management**: Synchronizes with official repositories to keep package lists up to date.

## Roadmap

Upcoming features and development targets:

- **Repository Modification**: Allow modification of supported repositories (First future release).
- **AUR Support**: Integration with the Arch User Repository for a wider range of software.
- **Package Grouping**: Group related packages for easier management.
- **Flatpak Support**: Manage Flatpak applications alongside native packages.
- **Snapd Support**: Support for Snap packages.

## Prerequisites

- **Arch Linux** (or an Arch-based distribution)
- **.NET 10.0 Runtime** (for running only if installed from aur)
- **.NET 10.0 SDK** (for building)
- **libalpm** (provided by `pacman`)

## Installation

### Using PKGBUILD

Since Shelly is designed for Arch Linux, you can build and install it using the provided `PKGBUILD`:
There is currently an issue with the package build and I suggest you run the local-install.sh after building it manually.
```bash
git clone https://github.com/ZoeyErinBauer/Shelly-ALPM.git
cd Shelly-ALPM
makepkg -si
```

### Manual Build

You can also build the project manually using the .NET CLI:

```bash
dotnet publish Shelly-UI/Shelly-UI.csproj -c Release -o publish/shelly-ui
dotnet publish Shelly-CLI/Shelly-CLI.csroj -C Release -o publish/shelly-cli
```
alternatively you can run
```bash
sudo ./local-install.sh
```
This will build and perform the functions of install.sh

The binary will be located in the `/publish/shelly-ui/` directory.

## Usage

Run the application from your terminal or application launcher:

```bash
shelly-ui
```

*Note: Shelly will relaunch itself as root using pkexec if not already running as root.*

## Shelly-CLI

Shelly also includes a command-line interface (`shelly-cli`) for users who prefer terminal-based package management. The
CLI provides the same core functionality as the UI but in a scriptable, terminal-friendly format.

### CLI Commands

| Command              | Description                     |
|----------------------|---------------------------------|
| `sync`               | Synchronize package databases   |
| `list-installed`     | List all installed packages     |
| `list-available`     | List all available packages     |
| `list-updates`       | List packages that need updates |
| `install <packages>` | Install one or more packages    |
| `remove <packages>`  | Remove one or more packages     |
| `update <packages>`  | Update one or more packages     |
| `upgrade`            | Perform a full system upgrade   |

### CLI Options

**Global options:**

- `--help` - Display help information
- `--version` - Display version information

**sync command:**

- `-f, --force` - Force synchronization even if databases are up to date

**install, remove, update commands:**

- `--no-confirm` - Skip confirmation prompt

**upgrade command:**

- `--no-confirm` - Skip confirmation prompt

### CLI Examples

```bash
# Synchronize package databases
shelly-cli sync

# Force sync even if up to date
shelly-cli sync --force

# List all installed packages
shelly-cli list-installed

# List packages needing updates
shelly-cli list-updates

# Install packages
shelly-cli install firefox vim

# Install without confirmation
shelly-cli install --no-confirm firefox

# Remove packages
shelly-cli remove firefox

# Update specific packages
shelly-cli update firefox vim

# Perform full system upgrade
shelly-cli upgrade

# System upgrade without confirmation
shelly-cli upgrade --no-confirm
```

## Development

Shelly is structured into several components:

- **Shelly-UI**: The main Avalonia-based desktop application.
- **Shelly-CLI**: Command-line interface for terminal-based package management.
- **PackageManager**: The core logic library providing bindings and abstractions for `libalpm`.
- **PackageManager.Tests**: Comprehensive tests for the package management logic.
- **Shelly-UI.Tests**: Unit tests for the Avalonia UI components.

### Building for Development

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## License

This project is licensed under the GPL-3.0 License - see the [PKGBUILD](PKGBUILD) or project files for details.

## Disclaimer

Shelly is in active development. It comes with no guarantees and may contain bugs, however if you experience issues
please report them by opening an issue on this page and we will do our best to resolve the issues.
