#!/bin/bash

# Check for root privileges
if [ "$EUID" -ne 0 ]; then
  echo "Please run as root"
  exit 1
fi

INSTALL_DIR="/usr/bin/Shelly"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Creating installation directory: $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"

echo "Copying files to $INSTALL_DIR"
cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/"

# Ensure the binary is executable
if [ -f "$INSTALL_DIR/Shelly-UI" ]; then
    chmod +x "$INSTALL_DIR/Shelly-UI"
fi

# Ensure the CLI binary is executable and accessible in PATH
if [ -f "$INSTALL_DIR/Shelly-CLI" ]; then
    chmod +x "$INSTALL_DIR/Shelly-CLI"
    echo "Creating symlink for shelly-cli in /usr/local/bin"
    ln -sf "$INSTALL_DIR/Shelly-CLI" /usr/local/bin/shelly-cli
fi

echo "Creating desktop entry"
cat <<EOF > /usr/share/applications/shelly.desktop
[Desktop Entry]
Name=Shelly
Exec=$INSTALL_DIR/Shelly-UI
Icon=$INSTALL_DIR/shellylogo.png
Type=Application
Categories=System;Utility;
Terminal=false
EOF

echo "Installation complete!"
