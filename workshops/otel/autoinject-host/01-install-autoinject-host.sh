#!/bin/bash
#set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root" 1>&2
    exit 1
fi

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
    echo -e "CORECLR_ENABLE_PROFILING=1\nCORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318}\nCORECLR_PROFILER_PATH=/usr/lib/opentelemetry/dotnet/linux-x64/OpenTelemetry.AutoInstrumentation.Native.so\nDOTNET_ADDITIONAL_DEPS=/usr/lib/opentelemetry/dotnet/AdditionalDeps\nDOTNET_SHARED_STORE=/usr/lib/opentelemetry/dotnet/store\nDOTNET_STARTUP_HOOKS=/usr/lib/opentelemetry/dotnet/net/OpenTelemetry.AutoInstrumentation.StartupHook.dll\nOTEL_DOTNET_AUTO_HOME=/usr/lib/opentelemetry/dotnet" | sudo tee "$CONFIG_DIR/dotnet.conf" > /dev/null
fi

echo "Adding OpenTelemetry Injector library to ld.so.preload..."
echo /usr/lib/opentelemetry/libotelinject.so | sudo tee -a /etc/ld.so.preload
echo "OpenTelemetry Injector setup complete."

# Navigate back to the original directory before cleanup
cd ../../..

echo "Cleaning up cloned repository..."
sudo rm -rf "$REPO_DIR"
echo "Cleanup complete."
# You can update the config values in /etc/opentelemetry/otelinject