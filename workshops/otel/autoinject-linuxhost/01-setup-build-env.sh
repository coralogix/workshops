#!/bin/bash

#===============================================================================
# OpenTelemetry Auto-Instrumentation Host Environment Setup
#===============================================================================
# 
# PURPOSE:
#   This script prepares a Linux system for OpenTelemetry auto-instrumentation
#   by installing essential build tools and utilities. It's designed to run on
#   Ubuntu/Debian-based systems.
#
# WHAT IT INSTALLS:
#   1. Build tools (build-essential) - Required for compiling source code
#   2. Essential utilities (curl, ca-certificates) - For secure downloads
#   3. Git - For cloning repositories
#
# WHY THESE DEPENDENCIES ARE NEEDED:
#   • build-essential: Provides gcc, make, and other tools for building packages
#   • git: Required for cloning the OpenTelemetry injector repository
#   • curl: Required for downloading packages and keys
#   • ca-certificates: Ensures secure HTTPS connections for package downloads
#
# SECURITY CONSIDERATIONS:
#   • Requires root privileges to install system packages
#   • Uses official Ubuntu/Debian repositories only
#   • No additional group memberships or special privileges
#
# COMPATIBILITY:
#   • Designed for Ubuntu/Debian-based systems
#   • Uses apt package manager
#   • Minimal dependencies for maximum compatibility
#
#===============================================================================

# SHELL CONFIGURATION
# set -e: Exit immediately if any command fails (strict error handling)
# set -u: Treat unset variables as errors (prevents typos)
# set -o pipefail: Make pipes fail if any command in the pipeline fails
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

#===============================================================================
# ROOT PRIVILEGE VALIDATION
#===============================================================================
# 
# WHY ROOT IS REQUIRED:
#   - Installing system packages via apt
#   - Updating package repositories
#   - Installing build dependencies system-wide
#
if [ "$(id -u)" -ne 0 ]; then
    echo "ERROR: This script must be run as root to install system packages." >&2
    echo "Please run with: sudo $0" >&2
    stop_script
fi

# Early termination check - stop here if root validation failed
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Function to check if a package is installed using dpkg
# This is more reliable than checking if commands exist because some packages
# install multiple binaries or may not be in PATH immediately after installation
# Args: $1 - Package name to check
# Returns: 0 if installed, 1 if not installed
check_package() {
    dpkg -l "$1" >/dev/null 2>&1
}

#===============================================================================
# BUILD TOOLS INSTALLATION
#===============================================================================
# 
# build-essential is a meta-package that installs:
#   • gcc (GNU Compiler Collection) - C/C++ compiler
#   • g++ - C++ compiler
#   • make - Build automation tool
#   • libc6-dev - Standard C library development files
#   • dpkg-dev - Debian package development tools
#
# These tools are essential for:
#   • Compiling OpenTelemetry components from source
#   • Building the injector .deb package
#   • Installing packages that require native compilation
#
echo "Installing Build Tools..."
echo "=========================="
if ! check_package build-essential; then
    echo "Installing build-essential (gcc, make, development tools)..."
    sudo apt-get update && sudo apt-get install -y build-essential
    echo "build-essential installed successfully."
else
    echo "build-essential is already installed, skipping."
fi
echo ""

#===============================================================================
# ESSENTIAL UTILITIES INSTALLATION
#===============================================================================
#
# REQUIRED PACKAGES:
#
# GIT:
#   • Version control system
#   • Required to clone the OpenTelemetry injector repository
#   • Used by the main installation script (01-install-autoinject-host.sh)
#
# CA-CERTIFICATES:
#   • Contains trusted Certificate Authority certificates
#   • Required for HTTPS connections to package repositories
#   • Ensures secure downloads of packages and keys
#
# CURL:
#   • Command-line tool for transferring data with URLs
#   • Used for downloading files securely
#   • More reliable than wget for scripted downloads
#
echo "Installing Essential Utilities..."
echo "================================="
PACKAGES_TO_INSTALL=""

# Check for git (required for cloning repositories)
if ! check_package git; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL git"
    echo "• git - for cloning OpenTelemetry injector repository"
fi

# Check for ca-certificates (needed for secure HTTPS connections)
if ! check_package ca-certificates; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL ca-certificates"
    echo "• ca-certificates - for secure HTTPS connections"
fi

# Check for curl (needed for secure downloads)
if ! check_package curl; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL curl"
    echo "• curl - for downloading packages and keys"
fi

# Install any missing utilities
if [ -n "$PACKAGES_TO_INSTALL" ]; then
    echo "Installing missing utilities:$PACKAGES_TO_INSTALL"
    sudo apt-get update
    sudo apt-get install -y $PACKAGES_TO_INSTALL
    echo "Essential utilities installed successfully."
else
    echo "All required utilities (git, ca-certificates, curl) are already installed."
fi
echo ""

#===============================================================================
# INSTALLATION COMPLETE
#===============================================================================
echo "Environment Setup Complete!"
echo "============================"
echo ""
echo "What was installed:"
echo "  • Build tools (gcc, make, development libraries)"
echo "  • Git (version control for repository cloning)"
echo "  • Curl (secure file downloads)"
echo "  • CA certificates (secure HTTPS connections)"
echo ""
echo "Next Steps:"
echo "  1. Run the auto-injector installer: sudo ./01-install-autoinject-host.sh"
echo "  2. Choose your injection method (system-wide or systemd-only)"
echo "  3. Test with the sample applications in java/, node/, or dotnet8-linux/"
echo ""
echo "What each tool does:"
echo "  • build-essential: Compiles the OpenTelemetry injector from source"
echo "  • git: Downloads the latest injector code from GitHub"
echo "  • curl: Securely downloads any additional dependencies"
echo ""
echo "Your system is now ready for OpenTelemetry auto-instrumentation!"