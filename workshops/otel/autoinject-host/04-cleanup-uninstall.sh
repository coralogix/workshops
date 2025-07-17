#!/bin/bash
set -e
set -u
set -o pipefail

# Global flag to control script execution - prevents terminal crashes when sourced
SCRIPT_SHOULD_CONTINUE=true

#===============================================================================
# UTILITY FUNCTIONS
#===============================================================================

# Function to stop script execution without exiting terminal
# This is safer than using 'exit' which could kill terminal sessions
# Args: $1 - Optional custom message (defaults to generic stop message)
stop_script() {
    local message=${1:-"Script execution stopped."}
    echo "$message"
    SCRIPT_SHOULD_CONTINUE=false
}

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

# Function to remove build artifacts and .deb package
remove_build_artifacts() {
    echo "Removing build artifacts and .deb package..."
    
    # Remove the build directory if it exists
    if [ -d "opentelemetry-injector" ]; then
        echo "Removing opentelemetry-injector build directory..."
        rm -rf opentelemetry-injector
    fi
    
    # Remove any .deb files that might have been created
    DEB_FILES=$(find . -maxdepth 1 -name "opentelemetry-injector*.deb" 2>/dev/null || true)
    if [ -n "$DEB_FILES" ]; then
        echo "Removing .deb package files..."
        rm -f opentelemetry-injector*.deb
        echo "Removed: $DEB_FILES"
    else
        echo "No .deb package files found in current directory."
    fi
    
    # Also check common build locations
    if [ -f "/tmp/opentelemetry-injector.deb" ]; then
        echo "Removing .deb package from /tmp..."
        sudo rm -f /tmp/opentelemetry-injector*.deb
    fi
    
    echo "Build artifacts cleanup complete."
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
        echo "Invalid choice."
        stop_script "Invalid choice selected. Uninstallation cancelled."
        ;;
esac

# Check if we should continue after user choice validation
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Remove the package first (this handles common cleanup)
echo "Removing OpenTelemetry Injector package..."
if dpkg -l | grep -q opentelemetry-injector; then
    sudo dpkg --purge opentelemetry-injector
else
    echo "OpenTelemetry Injector package not found (may already be removed)."
fi

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
        echo "Invalid uninstallation type."
        stop_script "Internal error: Invalid uninstallation type. This should not happen."
        ;;
esac

# Check if we should continue after uninstallation operations
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Always remove build artifacts and .deb package
remove_build_artifacts

# Final cleanup - remove empty opentelemetry directory if it exists
if [ -d /usr/lib/opentelemetry ] && [ -z "$(ls -A /usr/lib/opentelemetry 2>/dev/null)" ]; then
    echo "Removing empty OpenTelemetry directory..."
    sudo rmdir /usr/lib/opentelemetry
fi

echo "OpenTelemetry Injector uninstallation complete."