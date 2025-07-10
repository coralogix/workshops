#!/bin/bash
set -e
set -u
set -o pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "This script must be run as root to install Docker and other dependencies." 1>&2
    exit 1
fi

sudo apt-get update && sudo apt-get install -y build-essential

echo "Adding current user to the 'docker' group..."
sudo usermod -aG docker "$USER"
echo "Please log out and log back in for the 'docker' group changes to take effect."

echo "Updating apt package index again..."
sudo apt-get update

echo "Installing CA certificates and curl..."
sudo apt-get install -y ca-certificates curl

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

echo "Installing Docker components: docker-ce, docker-ce-cli, containerd.io, docker-buildx-plugin, docker-compose-plugin..."
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo "Docker installation complete. Please verify Docker installation by running 'docker run hello-world' after re-logging in."