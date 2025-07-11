#!/bin/bash
set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root to install Docker and other dependencies." 1>&2
    exit 1
fi

# Function to check if a package is installed
check_package() {
    dpkg -l "$1" >/dev/null 2>&1
}

# Function to check if a command exists
check_command() {
    command -v "$1" >/dev/null 2>&1
}

echo "Checking and installing build-essential..."
if ! check_package build-essential; then
    sudo apt-get update && sudo apt-get install -y build-essential
    echo "build-essential installed successfully."
else
    echo "build-essential is already installed, skipping."
fi

echo "Adding current user to the 'docker' group..."
if ! groups "$USER" | grep -q docker; then
    sudo usermod -aG docker "$USER"
    echo "User added to docker group. Please log out and log back in for the changes to take effect."
else
    echo "User is already in the docker group, skipping."
fi

echo "Checking and installing CA certificates and curl..."
PACKAGES_TO_INSTALL=""
if ! check_package ca-certificates; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL ca-certificates"
fi
if ! check_package curl; then
    PACKAGES_TO_INSTALL="$PACKAGES_TO_INSTALL curl"
fi

if [ -n "$PACKAGES_TO_INSTALL" ]; then
    sudo apt-get update
    sudo apt-get install -y $PACKAGES_TO_INSTALL
    echo "CA certificates and curl installed successfully."
else
    echo "CA certificates and curl are already installed, skipping."
fi

echo "Setting up Docker repository..."
if [ ! -f /etc/apt/keyrings/docker.asc ]; then
    echo "Creating Docker APT keyrings directory..."
    sudo install -m 0755 -d /etc/apt/keyrings

    echo "Downloading Docker GPG key..."
    sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc

    echo "Setting permissions for Docker GPG key..."
    sudo chmod a+r /etc/apt/keyrings/docker.asc

    echo "Adding Docker APT repository to sources list..."
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}" | tr -d '\n') stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

    echo "Updating apt package index after adding Docker repository..."
    sudo apt-get update
    echo "Docker repository setup complete."
else
    echo "Docker repository is already configured, skipping."
fi

echo "Checking and installing Docker components..."
DOCKER_PACKAGES_TO_INSTALL=""
if ! check_package docker-ce; then
    DOCKER_PACKAGES_TO_INSTALL="$DOCKER_PACKAGES_TO_INSTALL docker-ce"
fi
if ! check_package docker-ce-cli; then
    DOCKER_PACKAGES_TO_INSTALL="$DOCKER_PACKAGES_TO_INSTALL docker-ce-cli"
fi
if ! check_package containerd.io; then
    DOCKER_PACKAGES_TO_INSTALL="$DOCKER_PACKAGES_TO_INSTALL containerd.io"
fi
if ! check_package docker-buildx-plugin; then
    DOCKER_PACKAGES_TO_INSTALL="$DOCKER_PACKAGES_TO_INSTALL docker-buildx-plugin"
fi
if ! check_package docker-compose-plugin; then
    DOCKER_PACKAGES_TO_INSTALL="$DOCKER_PACKAGES_TO_INSTALL docker-compose-plugin"
fi

if [ -n "$DOCKER_PACKAGES_TO_INSTALL" ]; then
    sudo apt-get install -y $DOCKER_PACKAGES_TO_INSTALL
    echo "Docker components installed successfully."
else
    echo "All Docker components are already installed, skipping."
fi

echo "Docker installation complete. Please verify Docker installation by running 'docker run hello-world' after re-logging in."