#!/bin/bash

# Path to YAML files
YAML_DIR="./yaml"

# Apply the good deployment to revert the changes
kubectl apply -f "${YAML_DIR}/deploy-good.yaml"

# Scale up the good deployment to the desired number of replicas
kubectl scale deployment cx-payment-gateway-flask --replicas=2

# Ensure the good deployment is up and running
kubectl rollout status deployment/cx-payment-gateway-flask

echo "Reverted to good deployment successfully."
