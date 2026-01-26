#!/bin/bash

# Check for root privileges
if [ "$EUID" -ne 0 ]; then
  echo "Please run as root"
  exit 1
fi

INSTALL_DIR="/opt/shelly"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Creating installation directory: $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"

echo "Copying files to $INSTALL_DIR"
cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/"

# Ensure the binary is executable
if [ -f "$INSTALL_DIR/Shelly-UI" ]; then
    chmod +x "$INSTALL_DIR/Shelly-UI"
    echo "Creating symlink for shelly-ui in /usr/bin"
    ln -sf "$INSTALL_DIR/Shelly-UI" /usr/bin/shelly-ui
fi

# Ensure the CLI binary is executable and accessible in PATH
if [ -f "$INSTALL_DIR/shelly" ]; then
    chmod +x "$INSTALL_DIR/shelly"
    echo "Creating symlink for shelly in /usr/bin"
    ln -sf "$INSTALL_DIR/shelly" /usr/bin/shelly
fi

# Install icon to standard location
echo "Installing icon..."
mkdir -p /usr/share/icons/hicolor/256x256/apps
cp "$INSTALL_DIR/shellylogo.png" /usr/share/icons/hicolor/256x256/apps/shelly.png 2>/dev/null || true

echo "Creating desktop entry"
cat <<EOF > /usr/share/applications/shelly.desktop
[Desktop Entry]
Name=Shelly
Exec=/usr/bin/shelly-ui
Icon=shelly
Type=Application
Categories=System;Utility;
Terminal=false
EOF

REAL_USER=${SUDO_USER:-$USER}
USER_HOME=$(getent passwd "$REAL_USER" | cut -d: -f6)


USER_DESKTOP="$USER_HOME/Desktop"

if [ -d "$USER_DESKTOP" ]; then
    echo "Creating desktop icon for user: $REAL_USER"
    cp /usr/share/applications/shelly.desktop "$USER_DESKTOP/shelly.desktop"
    
    # Ensure the user owns the file and it's executable
    chown "$REAL_USER":"$REAL_USER" "$USER_DESKTOP/shelly.desktop"
    chmod +x "$USER_DESKTOP/shelly.desktop"
    
    # Mark as trusted (specific to some desktop environments like GNOME/KDE)
    gio set "$USER_DESKTOP/shelly.desktop" metadata::trusted true 2>/dev/null || true
else
    echo "Desktop directory not found for $REAL_USER, skipping desktop icon."
fi

echo "Installation complete!"
