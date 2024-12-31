#!/bin/bash

# Define an array of deployments to delete
deployments=(
  "cx-payment-gateway-flask"
  "cx-client-shopping-cart"
  "cx-server-payments"
  "cx-redis"
  "cx-shopping-cart-reqs"
  "cx-inventory-java-reqs"
  "cx-marketing-node-reqs"
  "dotnet-autogen"
  "prometheus-client"
  "cx-node-autogen"
  "cx-py-autogen"
  "cx-fastapi-autogen"
  "cx-payment-gateway-flask-bad"
)

# Define an array of services to delete
services=(
  "cx-payment-gateway-flask"
  "cx-redis"
)

# Function to delete deployments
delete_deployments() {
  for deployment in "${deployments[@]}"; 
  do
    kubectl delete deployment "$deployment" --ignore-not-found=true > /dev/null 2>&1 &
  done
}

# Function to delete services
delete_services() {
  for service in "${services[@]}"; 
  do
    kubectl delete service "$service" --ignore-not-found=true > /dev/null 2>&1 &
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
