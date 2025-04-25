#!/bin/bash

# Update package list
sudo apt-get update

# Install ODBC driver manager and development files
sudo DEBIAN_FRONTEND=noninteractive apt-get install -y unixodbc unixodbc-dev

# Download and install Microsoft ODBC driver for SQL Server
if ! curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc; then
    echo "Failed to download Microsoft GPG key"
    exit 1
fi

sudo curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list

sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18
sudo ACCEPT_EULA=Y apt-get install -y mssql-tools18

# Add SQL Server tools to path
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc

echo "Dependencies installed successfully!" 