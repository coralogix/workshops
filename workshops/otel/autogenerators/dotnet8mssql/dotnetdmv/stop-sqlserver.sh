#!/bin/bash

# Script to stop and remove SQL Server Docker container

echo "Stopping SQL Server container..."
if docker ps -a --format '{{.Names}}' | grep -q "^sql1$"; then
    # Stop the container if it's running
    docker stop sql1

    # Remove the container
    echo "Removing SQL Server container..."
    docker rm sql1
    echo "SQL Server container removed successfully."
else
    echo "SQL Server container 'sql1' not found."
fi

# Optional: Remove the SQL Server image as well
read -p "Do you want to remove the SQL Server Docker image as well? (y/n): " remove_image
if [[ $remove_image == "y" || $remove_image == "Y" ]]; then
    echo "Removing SQL Server Docker image..."
    docker rmi mcr.microsoft.com/mssql/server:2022-latest
    echo "SQL Server image removed."
fi

echo "Cleanup completed." 