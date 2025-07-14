#!/bin/bash
set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root" 1>&2
    exit 1
fi

# Function to display uninstallation options
show_uninstallation_options() {
    echo "OpenTelemetry Injector Uninstallation"
    echo "====================================="
    echo ""
    echo "Please choose what to uninstall:"
    echo "1) System-wide installation (removes all configurations)"
    echo "2) Systemd services only (removes systemd configuration)"
    echo "3) Everything (both system-wide and systemd configurations)"
    echo ""
}

# Function to remove system-wide installation
remove_system_wide() {
    echo "Removing system-wide OpenTelemetry Injector configuration..."
    
    echo "Removing OpenTelemetry Injector configuration directory..."
    sudo rm -rf /etc/opentelemetry/otelinject

    echo "Removing OpenTelemetry Injector library..."
    sudo rm -rf /usr/lib/opentelemetry/libotelinject.so

    echo "Removing OpenTelemetry Injector preload entry..."
    if [ -f /etc/ld.so.preload ]; then
        sudo sed -i '\|/usr/lib/opentelemetry/libotelinject.so|d' /etc/ld.so.preload
    fi
    
    # Clean up the opentelemetry directory if it's empty
    if [ -d /usr/lib/opentelemetry ] && [ -z "$(ls -A /usr/lib/opentelemetry 2>/dev/null)" ]; then
        echo "Removing empty OpenTelemetry directory..."
        sudo rmdir /usr/lib/opentelemetry
    fi
    
    echo "System-wide configuration removal complete."
}

# Function to remove systemd configuration
remove_systemd_config() {
    echo "Removing systemd OpenTelemetry Injector configuration..."
    
    SYSTEMD_DROPIN_FILE="/usr/lib/systemd/system.conf.d/00-otelinject-instrumentation.conf"
    if [ -f "$SYSTEMD_DROPIN_FILE" ]; then
        echo "Removing systemd drop-in configuration file..."
        sudo rm -f "$SYSTEMD_DROPIN_FILE"
        echo "Systemd configuration removal complete."
    else
        echo "Systemd configuration file not found at $SYSTEMD_DROPIN_FILE"
    fi
}

# Prompt user for uninstallation type
show_uninstallation_options
read -p "Enter your choice (1, 2, or 3): " choice

case $choice in
    1)
        UNINSTALL_TYPE="system-wide"
        ;;
    2)
        UNINSTALL_TYPE="systemd-only"
        ;;
    3)
        UNINSTALL_TYPE="everything"
        ;;
    *)
        echo "Invalid choice. Exiting."
        exit 1
        ;;
esac

# Remove the package first (this handles common cleanup)
echo "Removing OpenTelemetry Injector package..."
sudo dpkg --purge opentelemetry-injector

# Perform the selected uninstallation type
case $UNINSTALL_TYPE in
    "system-wide")
        remove_system_wide
        ;;
    "systemd-only")
        remove_systemd_config
        ;;
    "everything")
        remove_system_wide
        remove_systemd_config
        ;;
    *)
        echo "Invalid uninstallation type. Exiting."
        exit 1
        ;;
esac

# Final cleanup - remove empty opentelemetry directory if it exists
if [ -d /usr/lib/opentelemetry ] && [ -z "$(ls -A /usr/lib/opentelemetry 2>/dev/null)" ]; then
    echo "Removing empty OpenTelemetry directory..."
    sudo rmdir /usr/lib/opentelemetry
fi

echo "OpenTelemetry Injector uninstallation complete."