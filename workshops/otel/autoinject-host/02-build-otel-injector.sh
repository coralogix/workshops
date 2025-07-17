#!/bin/bash

#===============================================================================
# OpenTelemetry Auto-Instrumentation Injector Builder
#===============================================================================
# 
# PURPOSE:
#   This script builds the OpenTelemetry auto-instrumentation injector package
#   from source. It clones the official repository, compiles the code, and
#   creates a .deb package ready for installation.
#
# WHAT IT DOES:
#   1. Validates system requirements and privileges
#   2. Clones the latest OpenTelemetry injector source code
#   3. Builds the injector .deb package using make
#   4. Saves the package to a predictable location for installation
#
# OUTPUT:
#   Creates opentelemetry-injector.deb in the current directory
#   This package can then be installed using the companion install script
#
# REQUIREMENTS:
#   - Root privileges (sudo) for building system packages
#   - git for cloning the repository
#   - make and build-essential for compilation
#   - Internet connection to clone repository
#
# SAFETY:
#   - Never exits terminal (uses return instead of exit)
#   - Validates all dependencies before proceeding
#   - Cleans up temporary files after building
#   - Provides clear error messages and guidance
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
# WHY ROOT IS REQUIRED FOR BUILDING:
#   - make deb-package may require system access for packaging
#   - Some build steps may need to install temporary dependencies
#   - Ensures consistent build environment across different systems
#   - Package creation often requires root for proper file permissions
#
# DETECTION METHODS:
#   We use multiple methods because different environments may have different
#   ways of reporting root status (containers, different shells, etc.)
#
echo "OpenTelemetry Injector Builder"
echo "=============================="
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
    echo "Running with root privileges - proceeding with build..."
    echo ""
else
    echo "ERROR: This script requires root privileges to build the OpenTelemetry injector package."
    echo "   The build process needs to:"
    echo "   • Access system build tools and dependencies"
    echo "   • Create .deb packages with proper permissions"
    echo "   • Install temporary build dependencies if needed"
    echo "   • Ensure consistent build environment"
    echo ""
    echo "Please run with: sudo $0"
    stop_script
fi

# Early termination check - stop here if root validation failed
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

#===============================================================================
# DEPENDENCY VALIDATION
#===============================================================================
#
# Check for required build tools and provide clear guidance if missing
# This prevents build failures midway through the process
#
echo "Dependency Check"
echo "================"
echo ""

# Check if git is available for repository cloning
if ! command -v git &> /dev/null; then
    echo "ERROR: git is not installed."
    echo "Git is required to clone the OpenTelemetry injector repository."
    echo ""
    echo "To install git:"
    echo "  sudo apt-get update"
    echo "  sudo apt-get install -y git"
    echo ""
    echo "Or run the setup script first: sudo ./00-setup-env.sh"
    stop_script
fi

# Check if make is available for building
if ! command -v make &> /dev/null; then
    echo "ERROR: make is not installed."
    echo "Make is required to build the OpenTelemetry injector package."
    echo ""
    echo "To install make and build tools:"
    echo "  sudo apt-get update"
    echo "  sudo apt-get install -y build-essential"
    echo ""
    echo "Or run the setup script first: sudo ./00-setup-env.sh"
    stop_script
fi

# Check if we should continue after dependency validation
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo "All required dependencies are available."
echo "• git: $(git --version | head -1)"
echo "• make: $(make --version | head -1)"
echo ""

#===============================================================================
# SOURCE CODE ACQUISITION
#===============================================================================
#
# Clone the official OpenTelemetry injector repository
# Clean up any existing directories to ensure fresh build
#
echo "Source Code Acquisition"
echo "======================="
echo ""

REPO_DIR="opentelemetry-injector"
REPO_URL="https://github.com/open-telemetry/opentelemetry-injector.git"

echo "Preparing to clone OpenTelemetry Injector repository..."
echo "Repository: $REPO_URL"
echo ""

# Remove existing directory if it exists to ensure clean clone
if [ -d "$REPO_DIR" ]; then
    echo "Directory '$REPO_DIR' already exists."
    echo "Removing existing directory to ensure a fresh clone..."
    sudo rm -rf "$REPO_DIR"
    echo "Cleanup complete."
fi

# Clone the repository
echo "Cloning repository..."
if git clone "$REPO_URL"; then
    echo "Repository cloned successfully."
else
    echo "ERROR: Failed to clone the OpenTelemetry injector repository."
    echo "This could be due to:"
    echo "• Network connectivity issues"
    echo "• GitHub being temporarily unavailable"
    echo "• Repository URL changes"
    echo ""
    echo "Please check your internet connection and try again."
    stop_script
fi

# Check if we should continue after cloning
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo ""

#===============================================================================
# PACKAGE BUILDING
#===============================================================================
#
# Build the OpenTelemetry injector .deb package using the official build process
# This creates a distributable package that can be installed on target systems
#
echo "Package Building"
echo "================"
echo ""

echo "Navigating to source directory: $REPO_DIR"
cd "$REPO_DIR"

echo "Starting build process..."
echo "This may take several minutes depending on your system..."
echo ""

# Build the .deb package using the official makefile target
# The make command will handle all compilation and packaging steps
if sudo make deb-package; then
    echo ""
    echo "Build completed successfully!"
else
    echo ""
    echo "ERROR: Build process failed."
    echo "This could be due to:"
    echo "• Missing build dependencies"
    echo "• Compilation errors in the source code"
    echo "• Insufficient system resources"
    echo ""
    echo "Please check the build output above for specific error messages."
    echo "You may need to install additional build dependencies."
    stop_script
    
    # Navigate back before stopping
    cd ..
    return 2>/dev/null || true
fi

echo ""

#===============================================================================
# PACKAGE EXTRACTION AND CLEANUP
#===============================================================================
#
# Move the built package to a predictable location and clean up build artifacts
# This makes it easy for the installation script to find the package
#
echo "Package Extraction"
echo "=================="
echo ""

BUILD_DIR="instrumentation/dist"
PACKAGE_NAME="opentelemetry-injector.deb"

echo "Navigating to build output directory: $BUILD_DIR"
cd "$BUILD_DIR"

# Check if any .deb files were created
if ! ls *.deb 1> /dev/null 2>&1; then
    echo "ERROR: No .deb package files found in $(pwd)"
    echo "Build may have completed but package creation failed."
    echo "Please check the build output above for errors."
    stop_script
    
    # Navigate back before stopping
    cd ../../..
    return 2>/dev/null || true
fi

# Get the actual package name that was built
BUILT_PACKAGE=$(ls *.deb | head -1)
echo "Found built package: $BUILT_PACKAGE"

# Navigate back to the original directory
echo "Returning to original directory..."
cd ../../..

# Copy the package to the current directory with a standard name
echo "Copying package to current directory as '$PACKAGE_NAME'..."
if cp "$REPO_DIR/$BUILD_DIR/$BUILT_PACKAGE" "./$PACKAGE_NAME"; then
    echo "Package copied successfully."
else
    echo "ERROR: Failed to copy package to current directory."
    echo "Source: $REPO_DIR/$BUILD_DIR/$BUILT_PACKAGE"
    echo "Destination: ./$PACKAGE_NAME"
    stop_script
fi

# Clean up the source directory
echo ""
echo "Cleaning up temporary files..."
echo "Removing source directory: $REPO_DIR"
if sudo rm -rf "$REPO_DIR"; then
    echo "Cleanup completed successfully."
else
    echo "WARNING: Failed to remove source directory."
    echo "You may want to manually remove: $REPO_DIR"
fi

# Check if we should continue after package operations
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo ""

#===============================================================================
# BUILD COMPLETE
#===============================================================================
echo "Build Process Complete!"
echo "======================="
echo ""
echo "Package Information:"
echo "• Package name: $PACKAGE_NAME"
echo "• Package location: $(pwd)/$PACKAGE_NAME"
echo "• Package size: $(du -h "$PACKAGE_NAME" | cut -f1)"
echo ""
echo "Next Steps:"
echo "1. The OpenTelemetry injector package has been built successfully"
echo "2. Run the installation script: sudo ./03-install-otel-injector.sh"
echo "3. Choose your preferred injection method (system-wide or systemd-only)"
echo ""
echo "Package Details:"
if command -v dpkg >/dev/null 2>&1; then
    echo "• Package info: $(dpkg --info "$PACKAGE_NAME" | grep Package: || echo 'Package info not available')"
    echo "• Version: $(dpkg --info "$PACKAGE_NAME" | grep Version: || echo 'Version info not available')"
fi
echo ""
echo "The package is ready for installation on this or other compatible systems."
echo "You can copy '$PACKAGE_NAME' to other Ubuntu/Debian systems for installation." 