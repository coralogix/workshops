#!/bin/bash

# Exit on any error
set -e

echo "Starting SQL Server installation and setup..."

# Variables
SQL_PASSWORD="Toortoor9#"
CONTAINER_NAME="sql1"

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check and install Docker if not present
if ! command_exists docker; then
    echo "Docker not found. Installing Docker..."
    sudo apt-get update
    sudo apt-get install -y ca-certificates curl gnupg
    sudo install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
    sudo chmod a+r /etc/apt/keyrings/docker.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt-get update
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
    sudo usermod -aG docker $USER
    echo "Docker installed successfully."
else
    echo "Docker already installed."
fi

# Stop and remove existing SQL Server container if it exists
echo "Checking for existing SQL Server container..."
if sudo docker ps -a | grep -q $CONTAINER_NAME; then
    echo "Removing existing SQL Server container..."
    sudo docker stop $CONTAINER_NAME || true
    sudo docker rm $CONTAINER_NAME || true
fi

# Pull and run SQL Server container
echo "Pulling and running SQL Server container..."
sudo docker run \
    -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=$SQL_PASSWORD" \
    -p 1433:1433 \
    --name $CONTAINER_NAME \
    --hostname $CONTAINER_NAME \
    -d mcr.microsoft.com/mssql/server:2022-latest

# Wait for SQL Server to start
echo "Waiting for SQL Server to start..."
sleep 20

# Install SQL Server command-line tools
echo "Installing SQL Server command-line tools..."
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev

# Add SQL Tools to PATH
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
export PATH="$PATH:/opt/mssql-tools18/bin"

# Test the connection
echo "Testing SQL Server connection..."
for i in {1..30}; do
    if sqlcmd -S localhost -U sa -P "$SQL_PASSWORD" -Q "SELECT @@VERSION" -C > /dev/null 2>&1; then
        echo "SQL Server is ready!"
        echo "Testing database creation..."
        sqlcmd -S localhost -U sa -P "$SQL_PASSWORD" -Q "CREATE DATABASE TestDB; SELECT name FROM sys.databases;" -C
        break
    fi
    echo "Waiting for SQL Server to be ready... ($i/30)"
    sleep 2
done

echo "
Installation completed!

SQL Server connection details:
-----------------------------
Server: localhost
Port: 1433
Username: sa
Password: $SQL_PASSWORD

To connect using sqlcmd:
sqlcmd -S localhost -U sa -P '$SQL_PASSWORD' -C

To stop the SQL Server:
sudo docker stop $CONTAINER_NAME

To start the SQL Server:
sudo docker start $CONTAINER_NAME

To remove the SQL Server container:
sudo docker rm -f $CONTAINER_NAME
" 