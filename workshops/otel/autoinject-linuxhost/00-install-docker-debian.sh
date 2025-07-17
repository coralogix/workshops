#!/bin/bash

#===============================================================================
# Docker Installation Script for Debian-based Systems
#===============================================================================
# 
# PURPOSE:
#   This script installs Docker Engine and related components on Ubuntu/Debian
#   systems using Docker's official repository. It follows Docker's recommended
#   installation procedure for maximum compatibility and security.
#
# WHAT IT INSTALLS:
#   1. Docker Engine (docker-ce) - Core container runtime
#   2. Docker CLI (docker-ce-cli) - Command-line interface
#   3. Container runtime (containerd.io) - Industry-standard runtime
#   4. Docker Buildx (docker-buildx-plugin) - Extended build capabilities
#   5. Docker Compose (docker-compose-plugin) - Multi-container orchestration
#   6. Essential utilities (curl, ca-certificates) - For secure downloads
#
# SECURITY FEATURES:
#   • Uses Docker's official GPG-signed repository
#   • Verifies package authenticity with cryptographic signatures
#   • Sets appropriate file permissions for keyring files
#   • Adds user to docker group for non-root container management
#
# COMPATIBILITY:
#   • Ubuntu 20.04 LTS and newer
#   • Debian 10 (Buster) and newer
#   • Requires systemd for service management
#   • Supports x86_64 (amd64) and ARM64 architectures
#
# POST-INSTALLATION:
#   • User must log out and back in for group changes to take effect
#   • Docker service starts automatically on boot
#   • Test installation with: docker run hello-world
#
# SECURITY WARNING:
#   Users in the docker group have root-equivalent privileges on the host
#   system. Only add trusted users to this group in production environments.
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
#   - Adding users to system groups (docker group)
#   - Modifying system files (/etc/apt/sources.list.d/, /etc/apt/keyrings/)
#   - Setting up package repositories and GPG keys
#   - Managing systemd services
#
if [ "$(id -u)" -ne 0 ]; then
    echo "ERROR: This script must be run as root to install Docker and configure the system." >&2
    echo "Please run with: sudo $0" >&2
    stop_script
fi

# Early termination check - stop here if root validation failed
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Get the actual user who ran sudo (not root)
ACTUAL_USER=${SUDO_USER:-$USER}
if [ "$ACTUAL_USER" = "root" ]; then
    echo "WARNING: Running as root user. Docker group will be added to root."
    echo "Consider running with 'sudo' as a regular user instead."
fi

#===============================================================================
# SYSTEM COMPATIBILITY CHECK
#===============================================================================
#
# Verify we're running on a supported Debian-based system
# Check for required system components and architecture
#
echo "Docker Installation for Debian-based Systems"
echo "============================================="
echo ""

echo "Checking system compatibility..."

# Check if we're on a Debian-based system
if ! command -v apt-get >/dev/null 2>&1; then
    echo "ERROR: This script requires a Debian-based system with apt package manager." >&2
    stop_script
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Check architecture
ARCH=$(dpkg --print-architecture)
case $ARCH in
    amd64|arm64)
        echo "Detected supported architecture: $ARCH"
        ;;
    *)
        echo "WARNING: Untested architecture: $ARCH"
        echo "This script is tested on amd64 and arm64 only."
        read -p "Continue anyway? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "Installation cancelled."
            stop_script
        fi
        ;;
esac

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Detect distribution
if [ -f /etc/os-release ]; then
    . /etc/os-release
    echo "Detected distribution: $NAME $VERSION"
    
    # Warn about unsupported distributions
    case $ID in
        ubuntu|debian)
            echo "Supported distribution detected."
            ;;
        *)
            echo "WARNING: This script is designed for Ubuntu and Debian."
            echo "Your distribution ($NAME) may not be fully supported."
            ;;
    esac
else
    echo "WARNING: Could not detect distribution. Proceeding with caution."
fi

echo ""

# Function to check if a package is installed using dpkg
# Args: $1 - Package name to check
# Returns: 0 if installed, 1 if not installed
check_package() {
    dpkg -l "$1" >/dev/null 2>&1
}

# Function to check if a command/binary exists in PATH
# Args: $1 - Command name to check
# Returns: 0 if command exists, 1 if not found
check_command() {
    command -v "$1" >/dev/null 2>&1
}

#===============================================================================
# ESSENTIAL UTILITIES INSTALLATION
#===============================================================================
#
# Install required utilities for Docker repository setup:
#
# CA-CERTIFICATES:
#   • Contains trusted Certificate Authority certificates
#   • Required for HTTPS connections to Docker's repository
#   • Ensures package download authenticity
#
# CURL:
#   • Command-line tool for transferring data with URLs
#   • Used to download Docker's GPG signing key
#   • More reliable than wget for automated downloads
#
# GNUPG:
#   • GNU Privacy Guard for cryptographic operations
#   • Required for GPG key verification and management
#   • Ensures package authenticity
#
echo "Installing Essential Utilities..."
echo "================================"

PACKAGES_TO_INSTALL=""

# Check for ca-certificates (needed for secure HTTPS connections)
if ! check_package ca-certificates; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL ca-certificates"
    echo "• ca-certificates - for secure HTTPS connections"
fi

# Check for curl (needed to download Docker GPG key)
if ! check_package curl; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL curl"
    echo "• curl - for downloading Docker repository key"
fi

# Check for gnupg (needed for GPG key management)
if ! check_package gnupg; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL gnupg"
    echo "• gnupg - for GPG key verification"
fi

# Install any missing utilities
if [ -n "$PACKAGES_TO_INSTALL" ]; then
    echo "Installing missing utilities:$PACKAGES_TO_INSTALL"
    apt-get update
    apt-get install -y $PACKAGES_TO_INSTALL
    echo "Essential utilities installed successfully."
else
    echo "All required utilities are already installed."
fi

echo ""

#===============================================================================
# DOCKER REPOSITORY SETUP
#===============================================================================
#
# SECURITY PROCESS:
#   1. Remove any existing Docker packages from default repositories
#   2. Download Docker's official GPG key for package verification
#   3. Store key in system keyring with secure permissions
#   4. Add Docker's official repository with GPG signature verification
#   5. Update package index to include Docker packages
#
# WHY DOCKER'S OFFICIAL REPOSITORY:
#   • Ubuntu/Debian default repos often have outdated Docker versions
#   • Official repo provides latest stable versions with security updates
#   • Includes all Docker components (Engine, CLI, Buildx, Compose)
#   • Direct support and updates from Docker Inc.
#
# FILE LOCATIONS:
#   • GPG Key: /etc/apt/keyrings/docker.asc
#   • Repository config: /etc/apt/sources.list.d/docker.list
#
echo "Setting Up Docker Official Repository..."
echo "======================================="

# Remove conflicting packages that might interfere with Docker installation
echo "Removing conflicting packages..."
CONFLICTING_PACKAGES="docker.io docker-doc docker-compose podman-docker containerd runc"
for pkg in $CONFLICTING_PACKAGES; do
    if check_package "$pkg"; then
        echo "Removing conflicting package: $pkg"
        apt-get remove -y "$pkg" || true
    fi
done

# Create secure directory for APT keyrings if it doesn't exist
echo "Setting up APT keyring directory..."
install -m 0755 -d /etc/apt/keyrings

# Download and install Docker's official GPG key
if [ ! -f /etc/apt/keyrings/docker.asc ]; then
    echo "Downloading Docker's official GPG key..."
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
    
    # Set appropriate permissions (readable by all, writable by root only)
    echo "Setting secure permissions for Docker GPG key..."
    chmod a+r /etc/apt/keyrings/docker.asc
    
    echo "Docker GPG key installed successfully."
else
    echo "Docker GPG key already exists, skipping download."
fi

# Add Docker repository to APT sources with GPG verification
echo "Adding Docker repository to APT sources..."
# Determine the codename for the repository
if [ -f /etc/os-release ]; then
    . /etc/os-release
    # Use Ubuntu codename for Ubuntu, or VERSION_CODENAME for Debian
    REPO_CODENAME="${UBUNTU_CODENAME:-$VERSION_CODENAME}"
else
    # Fallback to lsb_release if available
    if check_command lsb_release; then
        REPO_CODENAME=$(lsb_release -cs)
    else
        echo "ERROR: Cannot determine distribution codename for repository setup." >&2
        stop_script
    fi
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo "Using repository codename: $REPO_CODENAME"

# Create repository configuration
echo "deb [arch=$ARCH signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $REPO_CODENAME stable" > /etc/apt/sources.list.d/docker.list

# Update package index with Docker repository
echo "Updating package index with Docker repository..."
apt-get update

echo "Docker repository setup complete."
echo ""

#===============================================================================
# DOCKER COMPONENTS INSTALLATION
#===============================================================================
#
# DOCKER COMPONENTS EXPLAINED:
#
# docker-ce (Community Edition):
#   • The main Docker Engine daemon (dockerd)
#   • Manages containers, images, networks, and volumes
#   • Provides the core containerization functionality
#   • Communicates with containerd for container operations
#
# docker-ce-cli:
#   • Command-line interface for Docker
#   • Provides the 'docker' command and all its subcommands
#   • Can manage both local and remote Docker instances
#   • Essential for all Docker operations
#
# containerd.io:
#   • Industry-standard container runtime (CNCF project)
#   • Handles container lifecycle management
#   • Used by Docker Engine as the default runtime
#   • Provides container execution and image management
#
# docker-buildx-plugin:
#   • Extended build capabilities for Docker
#   • Supports multi-platform builds (cross-architecture)
#   • Provides advanced Dockerfile features and caching
#   • Required for modern Docker build workflows
#
# docker-compose-plugin:
#   • Tool for defining and running multi-container applications
#   • Uses YAML files to configure application services
#   • Essential for complex applications with multiple components
#   • Integrates with Docker CLI as 'docker compose' command
#
echo "Installing Docker Components..."
echo "=============================="

# List of Docker packages to install
DOCKER_PACKAGES=(
    "docker-ce"
    "docker-ce-cli" 
    "containerd.io"
    "docker-buildx-plugin"
    "docker-compose-plugin"
)

echo "Installing Docker packages..."
for package in "${DOCKER_PACKAGES[@]}"; do
    echo "• $package"
done

# Install all Docker components
apt-get install -y "${DOCKER_PACKAGES[@]}"

echo "Docker components installed successfully."
echo ""

#===============================================================================
# DOCKER SERVICE CONFIGURATION
#===============================================================================
#
# Configure Docker service to start automatically and ensure it's running
# This step ensures Docker daemon is available immediately after installation
#
echo "Configuring Docker Service..."
echo "============================="

# Enable Docker service to start automatically on boot
echo "Enabling Docker service for automatic startup..."
systemctl enable docker

# Start Docker service now
echo "Starting Docker service..."
systemctl start docker

# Verify Docker service is running
if systemctl is-active --quiet docker; then
    echo "Docker service is running successfully."
else
    echo "ERROR: Docker service failed to start." >&2
    echo "Check system logs with: journalctl -u docker.service" >&2
    stop_script
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

echo ""

#===============================================================================
# USER GROUP CONFIGURATION  
#===============================================================================
#
# DOCKER GROUP SETUP:
#   Add the user to the 'docker' group to allow running Docker commands
#   without sudo. This is a convenience feature for development.
#
# SECURITY IMPLICATIONS:
#   • Users in the docker group have root-equivalent privileges
#   • They can mount any directory and access any file on the host
#   • They can run containers with root privileges
#   • This should be used cautiously in production environments
#
# ALTERNATIVES:
#   • Use 'sudo docker' for each command (more secure)
#   • Use rootless Docker (experimental, for advanced users)
#   • Use Docker contexts with remote Docker instances
#
echo "Configuring User Permissions..."
echo "=============================="

if [ "$ACTUAL_USER" != "root" ]; then
    echo "Adding user '$ACTUAL_USER' to the docker group..."
    
    # Check if docker group exists (it should after package installation)
    if ! getent group docker >/dev/null; then
        echo "Creating docker group..."
        groupadd docker
    fi
    
    # Add user to docker group
    usermod -aG docker "$ACTUAL_USER"
    
    echo "User '$ACTUAL_USER' added to docker group successfully."
    echo ""
    echo "IMPORTANT: Log out and log back in for group changes to take effect."
    echo "After re-login, you can run Docker commands without sudo."
else
    echo "Running as root - skipping docker group configuration."
fi

echo ""

#===============================================================================
# INSTALLATION VERIFICATION
#===============================================================================
#
# Verify Docker installation by checking versions and basic functionality
# This helps ensure everything was installed correctly
#
echo "Verifying Docker Installation..."
echo "==============================="

# Check Docker Engine version
if check_command docker; then
    echo "Docker Engine version:"
    docker --version
else
    echo "ERROR: Docker command not found after installation." >&2
    stop_script
fi

# Early termination check
if [ "$SCRIPT_SHOULD_CONTINUE" = "false" ]; then
    return 2>/dev/null || true
fi

# Check Docker Compose version
echo "Docker Compose version:"
docker compose version

# Check if Docker daemon is accessible
echo "Testing Docker daemon connectivity..."
if docker info >/dev/null 2>&1; then
    echo "Docker daemon is accessible and responding."
else
    echo "WARNING: Docker daemon is not accessible."
    echo "This may be normal if running without docker group membership."
fi

echo ""

#===============================================================================
# INSTALLATION COMPLETE
#===============================================================================
echo "Docker Installation Complete!"
echo "============================="
echo ""
echo "What was installed:"
echo "  • Docker Engine (container runtime)"
echo "  • Docker CLI (command-line interface)"  
echo "  • Containerd (container runtime)"
echo "  • Docker Buildx (extended build features)"
echo "  • Docker Compose (multi-container management)"
echo "  • Required utilities (curl, ca-certificates, gnupg)"
echo ""
echo "Next Steps:"
if [ "$ACTUAL_USER" != "root" ]; then
    echo "  1. Log out and log back in to activate docker group membership"
    echo "  2. Test Docker installation: docker run hello-world"
    echo "  3. Verify Docker Compose: docker compose version"
else
    echo "  1. Test Docker installation: docker run hello-world"
    echo "  2. Verify Docker Compose: docker compose version"
    echo "  3. Consider creating a non-root user for Docker operations"
fi
echo ""
echo "Useful Commands:"
echo "  • Check Docker status: systemctl status docker"
echo "  • View Docker info: docker info"
echo "  • List Docker images: docker images"
echo "  • List running containers: docker ps"
echo "  • Clean up unused resources: docker system prune"
echo ""
echo "Security Notes:"
if [ "$ACTUAL_USER" != "root" ]; then
    echo "  • User '$ACTUAL_USER' can now run Docker without sudo"
    echo "  • Docker group membership grants root-equivalent privileges"
    echo "  • Use 'sudo docker' instead if higher security is needed"
else
    echo "  • Running Docker as root has full system privileges"
    echo "  • Consider using a non-root user for regular Docker operations"
fi
echo ""
echo "Documentation:"
echo "  • Docker documentation: https://docs.docker.com/"
echo "  • Docker Compose documentation: https://docs.docker.com/compose/"
echo "  • Security best practices: https://docs.docker.com/engine/security/"
echo ""
echo "Your system is now ready for Docker containerization!" 