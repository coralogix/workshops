#!/bin/bash

#===============================================================================
# OpenTelemetry Auto-Instrumentation Injector Installer
#===============================================================================
# 
# PURPOSE:
#   This script installs and configures the OpenTelemetry auto-instrumentation
#   injector package. It provides two installation methods and handles all
#   system configuration required for automatic instrumentation.
#
# WHAT IT DOES:
#   1. Validates system requirements and package availability
#   2. Installs the OpenTelemetry injector .deb package
#   3. Configures injection method (system-wide or systemd-only)
#   4. Sets up configuration files and system integration
#
# INJECTION METHODS:
#   - System-wide: Uses ld.so.preload to inject into ALL processes
#   - Systemd-only: Uses systemd environment to inject only into services
#
# REQUIREMENTS:
#   - Root privileges (sudo) for system configuration
#   - opentelemetry-injector.deb package (built by 02-build-otel-injector.sh)
#   - Compatible Ubuntu/Debian system
#
# SAFETY:
#   - Never exits terminal (uses return instead of exit)
#   - Validates package existence before installation
#   - Checks for existing installations to prevent conflicts
#   - Provides clear rollback guidance if needed
#
#===============================================================================

# SHELL CONFIGURATION
# Remove set -e to prevent automatic exits that could kill terminal sessions
# set -u: Treat unset variables as errors (helps catch typos)
# set -o pipefail: Make pipelines fail if any command in the pipeline fails
set -u
set -o pipefail

# GLOBAL EXECUTION CONTROL
# This flag allows us to stop script execution gracefully without using exit
# which could potentially kill the terminal session if script is sourced
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

#===============================================================================
# ROOT PRIVILEGE VALIDATION
#===============================================================================
# 
# WHY ROOT IS REQUIRED FOR INSTALLATION:
#   - Installing .deb packages using dpkg
#   - Modifying system files (/etc/ld.so.preload, /etc/opentelemetry/)
#   - Creating systemd configuration files in system directories
#   - Setting up system-wide configuration and permissions
#
# DETECTION METHODS:
#   We use multiple methods because different environments may have different
#   ways of reporting root status (containers, different shells, etc.)
#
echo "OpenTelemetry Injector Installer"
echo "================================="
echo ""
echo "Root Privilege Check"
echo "==================="
echo ""

# Initialize root detection flag
IS_ROOT=false

# Method 1: Check EUID (Effective User ID) variable
# EUID=0 means root user. This is the most reliable method in most environments
if [ "${EUID:-}" = "0" ]; then
    IS_ROOT=true
fi

# Method 2: Use 'id' command to get user ID
# Fallback method if EUID variable is not available (some minimal shells)
if [ "$IS_ROOT" = "false" ] && command -v id >/dev/null 2>&1; then
    if [ "$(id -u 2>/dev/null)" = "0" ]; then
        IS_ROOT=true
    fi
fi

# Method 3: Use 'whoami' command to check username
# Final fallback method - checks if username is literally "root"
if [ "$IS_ROOT" = "false" ] && command -v whoami >/dev/null 2>&1; then
    if [ "$(whoami 2>/dev/null)" = "root" ]; then
        IS_ROOT=true
    fi
fi

# Evaluate root status and proceed or stop
if [ "$IS_ROOT" = "true" ]; then
    echo "Running with root privileges - proceeding with installation..."
    echo ""
else
    echo "ERROR: This script requires root privileges to install and configure the OpenTelemetry injector."
    echo "   The installer needs to:"
    echo "   • Install .deb packages using dpkg"
    echo "   • Modify system configuration files"
    echo "   • Create configuration directories in /etc/"
    echo "   • Configure systemd environment settings"
    echo ""
    echo "Please run with: sudo $0"
    stop_script
fi

# Early termination check - stop here if root validation failed
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

#===============================================================================
# PACKAGE VALIDATION
#===============================================================================
#
# Verify that the OpenTelemetry injector package exists and is ready for installation
# This prevents installation failures and provides clear guidance
#
echo "Package Validation"
echo "=================="
echo ""

PACKAGE_NAME="opentelemetry-injector.deb"

echo "Checking for OpenTelemetry injector package..."
echo "Looking for: $PACKAGE_NAME"

if [ ! -f "$PACKAGE_NAME" ]; then
    echo "ERROR: OpenTelemetry injector package not found."
    echo ""
    echo "Expected package: $(pwd)/$PACKAGE_NAME"
    echo ""
    echo "The package must be built before installation. To build the package:"
    echo "1. Run the build script: sudo ./02-build-otel-injector.sh"
    echo "2. Wait for the build to complete successfully"
    echo "3. Run this installation script again"
    echo ""
    echo "Alternatively, if you have the package in a different location:"
    echo "• Copy it to the current directory as '$PACKAGE_NAME'"
    echo "• Or run this script from the directory containing the package"
    stop_script
fi

# Check if we should continue after package validation
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo "Package found: $PACKAGE_NAME"
echo "Package size: $(du -h "$PACKAGE_NAME" | cut -f1)"

# Display package information if available
if command -v dpkg >/dev/null 2>&1; then
    echo "Package details:"
    echo "• $(dpkg --info "$PACKAGE_NAME" | grep "Package:" || echo "Package info not available")"
    echo "• $(dpkg --info "$PACKAGE_NAME" | grep "Version:" || echo "Version info not available")"
    echo "• $(dpkg --info "$PACKAGE_NAME" | grep "Description:" || echo "Description not available")"
fi

echo ""

#===============================================================================
# INSTALLATION CONFIGURATION FUNCTIONS
#===============================================================================

# Function to display installation method options to the user
# 
# INSTALLATION METHODS EXPLAINED:
#
# 1) SYSTEM-WIDE INJECTION:
#    • Uses ld.so.preload mechanism
#    • Affects ALL processes on the system (including new ones)
#    • Automatically instruments any supported application language
#    • Higher impact but broader coverage
#    • Requires modifying critical system file (/etc/ld.so.preload)
#
# 2) SYSTEMD SERVICES ONLY:
#    • Uses systemd environment configuration
#    • Only affects systemd-managed services
#    • More controlled and safer approach
#    • Easier to disable/remove
#    • No modification of critical system files
#
show_installation_options() {
    echo "Installation Method Selection"
    echo "============================="
    echo ""
    echo "Please choose an installation method:"
    echo ""
    echo "1) System-wide (affects all processes)"
    echo "   • Uses ld.so.preload to inject into ALL processes"
    echo "   • Automatically instruments any supported application"
    echo "   • Broader coverage but higher system impact"
    echo "   • Requires system reboot or process restart for activation"
    echo ""
    echo "2) Systemd services only"
    echo "   • Uses systemd environment to inject only into services"
    echo "   • More controlled and safer approach"
    echo "   • Easier to manage and remove"
    echo "   • Requires systemctl daemon-reload and service restart"
    echo ""
    echo "IMPORTANT: Only one method should be activated to prevent conflicts and duplicate traces/metrics."
    echo ""
}

# Function to install system-wide injection configuration
install_system_wide() {
    echo "Configuring System-wide Injection"
    echo "================================="
    echo ""
    
    # Ensure the configuration directory exists
    CONFIG_DIR="/etc/opentelemetry/otelinject"
    echo "Creating configuration directory: $CONFIG_DIR"
    sudo mkdir -p "$CONFIG_DIR"

    # Create default configuration files if they don't exist
    echo "Setting up language-specific configuration files..."
    
    if [ ! -f "$CONFIG_DIR/java.conf" ]; then
        echo "• Creating default java.conf..."
        echo 'JAVA_TOOL_OPTIONS=-javaagent:/usr/lib/opentelemetry/javaagent.jar' | sudo tee "$CONFIG_DIR/java.conf" > /dev/null
    else
        echo "• java.conf already exists, skipping creation"
    fi

    if [ ! -f "$CONFIG_DIR/node.conf" ]; then
        echo "• Creating default node.conf..."
        echo 'NODE_OPTIONS=-r /usr/lib/opentelemetry/otel-js/node_modules/@opentelemetry-js/otel/instrument' | sudo tee "$CONFIG_DIR/node.conf" > /dev/null
    else
        echo "• node.conf already exists, skipping creation"
    fi

    if [ ! -f "$CONFIG_DIR/dotnet.conf" ]; then
        echo "• Creating default dotnet.conf..."
        echo -e "CORECLR_ENABLE_PROFILING=1\nCORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318}\nCORECLR_PROFILER_PATH=/usr/lib/opentelemetry/otel-dotnet/linux-x64/OpenTelemetry.AutoInstrumentation.Native.so\nDOTNET_ADDITIONAL_DEPS=/usr/lib/opentelemetry/otel-dotnet/AdditionalDeps\nDOTNET_SHARED_STORE=/usr/lib/opentelemetry/otel-dotnet/store\nDOTNET_STARTUP_HOOKS=/usr/lib/opentelemetry/otel-dotnet/net/OpenTelemetry.AutoInstrumentation.StartupHook.dll\nOTEL_DOTNET_AUTO_HOME=/usr/lib/opentelemetry/otel-dotnet" | sudo tee "$CONFIG_DIR/dotnet.conf" > /dev/null
    else
        echo "• dotnet.conf already exists, skipping creation"
    fi

    echo ""
    echo "Configuring system-wide library preloading..."
    
    # CRITICAL: Validate library exists before modifying ld.so.preload
    OTEL_LIB="/usr/lib/opentelemetry/libotelinject.so"
    if [ -f "$OTEL_LIB" ]; then
        echo "• OpenTelemetry library found at: $OTEL_LIB"
        
        # Check if the library is already in ld.so.preload to avoid duplicates
        if ! grep -Fxq "$OTEL_LIB" /etc/ld.so.preload 2>/dev/null; then
            echo "• Adding OpenTelemetry library to ld.so.preload..."
            echo "$OTEL_LIB" | sudo tee -a /etc/ld.so.preload > /dev/null
            echo "• Successfully added $OTEL_LIB to ld.so.preload"
        else
            echo "• Library already present in ld.so.preload, skipping addition"
        fi
    else
        echo "ERROR: OpenTelemetry library not found at $OTEL_LIB"
        echo "This indicates the package installation may have failed."
        echo "Please check the package installation output above for errors."
        echo ""
        echo "Cannot proceed with system-wide installation without the library file."
        stop_script
        return 2>/dev/null || true
    fi
    
    echo ""
    echo "System-wide installation complete!"
    echo ""
    echo "Configuration Details:"
    echo "• Configuration directory: $CONFIG_DIR"
    echo "• Library preload file: /etc/ld.so.preload"
    echo "• Injector library: $OTEL_LIB"
    echo ""
    echo "IMPORTANT: Reboot the system or restart applications for changes to take effect."
    echo "The injector will automatically instrument supported applications on next startup."
}

# Function to install systemd-only injection configuration
install_systemd_only() {
    echo "Configuring Systemd-only Injection"
    echo "=================================="
    echo ""
    
    # Create systemd drop-in directory
    SYSTEMD_DROPIN_DIR="/usr/lib/systemd/system.conf.d"
    echo "Creating systemd drop-in directory: $SYSTEMD_DROPIN_DIR"
    sudo mkdir -p "$SYSTEMD_DROPIN_DIR"
    
    # Look for the sample systemd drop-in file
    SAMPLE_FILE="/usr/lib/opentelemetry/examples/systemd/00-opentelemetry-injector.conf"
    TARGET_FILE="$SYSTEMD_DROPIN_DIR/00-opentelemetry-injector.conf"
    
    echo "Setting up systemd environment configuration..."
    
    if [ -f "$SAMPLE_FILE" ]; then
        echo "• Found sample configuration: $SAMPLE_FILE"
        echo "• Copying systemd drop-in configuration to: $TARGET_FILE"
        sudo cp "$SAMPLE_FILE" "$TARGET_FILE"
        echo "• Systemd configuration installed successfully"
    else
        echo "WARNING: Sample systemd configuration file not found at $SAMPLE_FILE"
        echo ""
        echo "Creating basic systemd configuration manually..."
        
        # Create a basic systemd configuration if the sample doesn't exist
        cat << 'EOF' | sudo tee "$TARGET_FILE" > /dev/null
[Manager]
# OpenTelemetry Auto-Instrumentation Environment
# This configures environment variables for systemd-managed services
DefaultEnvironment=LD_PRELOAD=/usr/lib/opentelemetry/libotelinject.so
EOF
        echo "• Created basic systemd configuration at: $TARGET_FILE"
        echo "• You may need to customize this configuration for your specific needs"
    fi
    
    echo ""
    echo "Systemd-only installation complete!"
    echo ""
    echo "Configuration Details:"
    echo "• Systemd configuration: $TARGET_FILE"
    echo "• Affects: All systemd-managed services"
    echo "• Scope: Systemd services only (not interactive processes)"
    echo ""
    echo "IMPORTANT: Run the following commands to activate the configuration:"
    echo "1. Reload systemd configuration: sudo systemctl daemon-reload"
    echo "2. Restart affected systemd services to apply instrumentation"
    echo ""
    echo "The injector will instrument systemd services on their next restart."
}

#===============================================================================
# PACKAGE INSTALLATION
#===============================================================================
echo "Package Installation"
echo "==================="
echo ""

echo "Installing OpenTelemetry injector package..."
echo "Package: $PACKAGE_NAME"
echo ""

# Install the .deb package using dpkg
if sudo dpkg -i "$PACKAGE_NAME"; then
    echo ""
    echo "Package installed successfully!"
else
    echo ""
    echo "ERROR: Package installation failed."
    echo "This could be due to:"
    echo "• Missing dependencies"
    echo "• Package corruption"
    echo "• Insufficient disk space"
    echo "• Conflicting packages"
    echo ""
    echo "You may need to resolve dependencies with:"
    echo "  sudo apt-get install -f"
    echo ""
    echo "Please check the installation output above for specific errors."
    stop_script
fi

# Check if we should continue after package installation
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo ""

#===============================================================================
# INSTALLATION METHOD SELECTION AND CONFIGURATION
#===============================================================================

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
        echo "Invalid choice: $choice"
        echo "Please run the script again and choose either 1 or 2."
        stop_script
        ;;
esac

# Check if we should continue after user choice
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo ""

# Perform the selected installation type configuration
case $INSTALL_TYPE in
    "system-wide")
        install_system_wide
        ;;
    "systemd-only")
        install_systemd_only
        ;;
esac

#===============================================================================
# INSTALLATION COMPLETE
#===============================================================================
echo ""
echo "OpenTelemetry Injector Installation Complete!"
echo "============================================="
echo ""

if [ "$INSTALL_TYPE" = "system-wide" ]; then
    echo "Installation Type: System-wide injection"
    echo "Configuration Location: /etc/opentelemetry/otelinject/"
    echo ""
    echo "What happens next:"
    echo "• All new processes will be automatically instrumented"
    echo "• Existing processes need to be restarted to be instrumented"
    echo "• System reboot will ensure all processes are instrumented"
    echo ""
    echo "Configuration Management:"
    echo "• Java settings: /etc/opentelemetry/otelinject/java.conf"
    echo "• Node.js settings: /etc/opentelemetry/otelinject/node.conf"
    echo "• .NET settings: /etc/opentelemetry/otelinject/dotnet.conf"
    echo ""
    echo "To activate instrumentation:"
    echo "1. Reboot the system (recommended): sudo reboot"
    echo "2. Or restart individual applications/services"
else
    echo "Installation Type: Systemd services only"
    echo "Configuration Location: /usr/lib/systemd/system.conf.d/00-opentelemetry-injector.conf"
    echo ""
    echo "What happens next:"
    echo "• Only systemd-managed services will be instrumented"
    echo "• Interactive shell processes are not affected"
    echo "• More controlled and safer than system-wide injection"
    echo ""
    echo "To activate instrumentation:"
    echo "1. Reload systemd configuration: sudo systemctl daemon-reload"
    echo "2. Restart the services you want to instrument"
    echo "3. Verify instrumentation is working with service logs"
fi

echo ""
echo "Testing and Validation:"
echo "• Test with sample applications in: java/, node/, or dotnet8-linux/"
echo "• Check application logs for OpenTelemetry initialization messages"
echo "• Verify telemetry data is being sent to your observability backend"
echo ""
echo "Troubleshooting:"
echo "• Check system logs: journalctl -f"
echo "• Verify library exists: ls -la /usr/lib/opentelemetry/"
echo "• Test configuration: Check application startup logs"
echo ""
echo "Removal:"
echo "• To uninstall: sudo ./uninstall.sh (if available)"
echo "• Or manually remove package: sudo dpkg -r opentelemetry-injector"
echo ""
echo "Your system is now configured for OpenTelemetry auto-instrumentation!" 