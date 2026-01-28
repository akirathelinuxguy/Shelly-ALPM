# Shelly Wiki

Welcome to the Shelly Wiki!

## About

Shelly is a modern reimagination of the Arch Linux package manager, designed to be a more intuitive and user-friendly alternative to `pacman` and `octopi`. Unlike other Arch package managers, Shelly offers a modern, visual interface with a focus on user experience and ease of use. It **IS NOT** built as a `pacman` wrapper or front-end — it is a complete reimagination of how a user interacts with their Arch Linux system.

## Features

- **Modern-CLI**: Provides a command-line interface for advanced users and automation, with a focus on ease of use.
- **Native Arch Integration**: Directly interacts with `libalpm` for accurate and fast package management.
- **Modern UI Framework**: Built using [Avalonia UI](https://avaloniaui.net/), ensuring a modern and responsive user interface.
- **Package Management**: Supports searching for, installing, updating, and removing packages.
- **Repository Management**: Synchronizes with official repositories to keep package lists up to date.

## Quick Install

Install Shelly with a single command:

```bash
curl -fsSL https://raw.githubusercontent.com/ZoeyErinBauer/Shelly-ALPM/master/web-install.sh | sudo bash
```

## Prerequisites

- **Arch Linux** (or an Arch-based distribution)
- **.NET 10.0 Runtime** (for running only if installed from non -bin AUR package)
- **.NET 10.0 SDK** (for building)
- **libalpm** (provided by `pacman`)

## Project Structure

- **Shelly-UI**: The main Avalonia-based desktop application.
- **Shelly-CLI**: Command-line interface for terminal-based package management.
- **PackageManager**: The core logic library providing bindings and abstractions for `libalpm`.

## Getting Started

See the [User Guide](wiki.md) for detailed instructions on using Shelly.

## License

This project is licensed under the GPL-2.0 License.
