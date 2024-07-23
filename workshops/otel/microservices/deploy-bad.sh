#!/bin/bash

# Path to YAML files
YAML_DIR="./yaml"

# Delete the existing good deployment
kubectl delete deployment cx-payment-gateway-flask --ignore-not-found=true

# Wait for the deletion to complete
while kubectl get deployment cx-payment-gateway-flask; do
  echo "Waiting for cx-payment-gateway-flask to be deleted..."
  sleep 1
done

# Apply the bad deployment
kubectl apply -f "${YAML_DIR}/deploy-bad.yaml"

# Ensure the bad deployment is up and running
kubectl rollout status deployment/cx-payment-gateway-flask

echo "Bad deployment applied successfully."
