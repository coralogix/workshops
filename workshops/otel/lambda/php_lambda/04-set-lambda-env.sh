#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
ENV_FILE="env_vars.json"  # Path to the environment variables file

# Check if the environment variables file exists
if [ ! -f "$ENV_FILE" ]; then
  echo "Environment variables file ($ENV_FILE) not found. Exiting."
  exit 1
fi

# Update the Lambda function environment variables
echo "Updating environment variables for Lambda function: $FUNCTION_NAME..."
aws lambda update-function-configuration \
  --function-name "$FUNCTION_NAME" \
  --environmen=file://$ENV_FILE
