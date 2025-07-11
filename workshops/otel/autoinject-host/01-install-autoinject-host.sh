#!/bin/bash
set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root" 1>&2
    exit 1
fi

# Function to display installation options
show_installation_options() {
    echo "OpenTelemetry Injector Installation"
    echo "==================================="
    echo ""
    echo "Please choose an installation method:"
    echo "1) System-wide (affects all processes)"
    echo "2) Systemd services only"
    echo ""
    echo "Note: Only one method should be activated to prevent conflicts and duplicate traces/metrics."
    echo ""
}

# Function to install system-wide
install_system_wide() {
    echo "Installing OpenTelemetry Injector system-wide..."
    
    # Ensure the configuration directory exists
    CONFIG_DIR="/etc/opentelemetry/otelinject"
    sudo mkdir -p "$CONFIG_DIR"

    # Create default configuration files if they don't exist
    if [ ! -f "$CONFIG_DIR/java.conf" ]; then
        echo "Creating default java.conf..."
        echo 'JAVA_TOOL_OPTIONS=-javaagent:/usr/lib/opentelemetry/javaagent.jar' | sudo tee "$CONFIG_DIR/java.conf" > /dev/null
    fi

    if [ ! -f "$CONFIG_DIR/node.conf" ]; then
        echo "Creating default node.conf..."
        echo 'NODE_OPTIONS=-r /usr/lib/opentelemetry/otel-js/node_modules/@opentelemetry-js/otel/instrument' | sudo tee "$CONFIG_DIR/node.conf" > /dev/null
    fi

    if [ ! -f "$CONFIG_DIR/dotnet.conf" ]; then
        echo "Creating default dotnet.conf..."
        echo -e "CORECLR_ENABLE_PROFILING=1\nCORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318}\nCORECLR_PROFILER_PATH=/usr/lib/opentelemetry/otel-dotnet/linux-x64/OpenTelemetry.AutoInstrumentation.Native.so\nDOTNET_ADDITIONAL_DEPS=/usr/lib/opentelemetry/otel-dotnet/AdditionalDeps\nDOTNET_SHARED_STORE=/usr/lib/opentelemetry/otel-dotnet/store\nDOTNET_STARTUP_HOOKS=/usr/lib/opentelemetry/otel-dotnet/net/OpenTelemetry.AutoInstrumentation.StartupHook.dll\nOTEL_DOTNET_AUTO_HOME=/usr/lib/opentelemetry/otel-dotnet" | sudo tee "$CONFIG_DIR/dotnet.conf" > /dev/null
    fi

    echo "Adding OpenTelemetry Injector library to ld.so.preload..."
    echo /usr/lib/opentelemetry/libotelinject.so | sudo tee -a /etc/ld.so.preload
    echo "System-wide installation complete."
    echo "Note: Reboot the system or restart applications for changes to take effect."
}

# Function to install for systemd services only
install_systemd_only() {
    echo "Installing OpenTelemetry Injector for systemd services only..."
    
    # Create systemd drop-in directory
    SYSTEMD_DROPIN_DIR="/usr/lib/systemd/system.conf.d"
    sudo mkdir -p "$SYSTEMD_DROPIN_DIR"
    
    # Copy the sample systemd drop-in file
    SAMPLE_FILE="/usr/lib/opentelemetry/examples/systemd/00-opentelemetry-injector.conf"
    TARGET_FILE="$SYSTEMD_DROPIN_DIR/00-opentelemetry-injector.conf"
    
    if [ -f "$SAMPLE_FILE" ]; then
        echo "Copying systemd drop-in configuration..."
        sudo cp "$SAMPLE_FILE" "$TARGET_FILE"
        echo "Systemd services installation complete."
        echo "Note: Run 'systemctl daemon-reload' and restart applicable systemd services for changes to take effect."
    else
        echo "Warning: Sample systemd configuration file not found at $SAMPLE_FILE"
        echo "You may need to create the configuration manually."
    fi
}

# Prompt user for installation type
show_installation_options
read -p "Enter your choice (1 or 2): " choice

case $choice in
    1)
        INSTALL_TYPE="system-wide"
        ;;
    2)
        INSTALL_TYPE="systemd-only"
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac

REPO_DIR="opentelemetry-injector"

echo "Cloning OpenTelemetry Injector repository..."
if [ -d "$REPO_DIR" ]; then
    echo "Directory '$REPO_DIR' already exists. Removing it to get a fresh clone."
    sudo rm -rf "$REPO_DIR"
fi
git clone https://github.com/open-telemetry/opentelemetry-injector.git

echo "Navigating to $REPO_DIR and building packages..."
cd "$REPO_DIR"
sudo make deb-package

echo "Installing OpenTelemetry Injector DEB package..."
cd instrumentation/dist

PACKAGE_NAME=$(ls *.deb)
sudo dpkg -i "$PACKAGE_NAME"
echo "Successfully installed $PACKAGE_NAME"

# Perform the selected installation type
case $INSTALL_TYPE in
    "system-wide")
        install_system_wide
        ;;
    "systemd-only")
        install_systemd_only
        ;;
    *)
        echo "Invalid installation type. Exiting."
        exit 1
        ;;
esac

# Navigate back to the original directory before cleanup
cd ../../..

echo "Cleaning up cloned repository..."
sudo rm -rf "$REPO_DIR"
echo "Cleanup complete."

echo ""
echo "OpenTelemetry Injector setup complete!"
echo "======================================"
if [ "$INSTALL_TYPE" = "system-wide" ]; then
    echo "Configuration files are located in: /etc/opentelemetry/otelinject/"
    echo "You can update the config values in /etc/opentelemetry/otelinject/"
    echo "Remember to reboot the system or restart applications for changes to take effect."
else
    echo "Systemd configuration is located in: /usr/lib/systemd/system.conf.d/00-otelinject-instrumentation.conf"
    echo "You can update the configuration by editing the systemd drop-in file."
    echo "Remember to run 'systemctl daemon-reload' and restart applicable systemd services for changes to take effect."
fi