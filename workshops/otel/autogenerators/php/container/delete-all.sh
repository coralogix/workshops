#!/bin/bash

# Define an array of deployments to delete
deployments=(
  "cx-php-server"
  "cx-php-client"
)

# Define an array of services to delete
services=(
  "cx-php-server"
  "cx-php-client"
)

# Function to delete deployments
delete_deployments() {
  for deployment in "${deployments[@]}"; 
  do
    kubectl delete deployment "$deployment" --ignore-not-found=true &
  done
}

# Function to delete services
delete_services() {
  for service in "${services[@]}"; 
  do
    kubectl delete service "$service" --ignore-not-found=true &
  done
}

# Delete deployments and services
delete_deployments
delete_services

# Wait for all background processes to complete
wait

# Check for any errors
if [ $? -eq 0 ]; then
  echo "All specified Kubernetes deployments and services have been deleted successfully."
else
  echo "There was an error deleting some Kubernetes deployments or services." >&2
fi
