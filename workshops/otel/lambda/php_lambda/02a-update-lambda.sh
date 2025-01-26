#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
ZIP_FILE="myFunction.zip"
ENV_FILE="env_vars.json"

# Check if the environment variables file exists
if [ ! -f "$ENV_FILE" ]; then
  echo "Environment variables file ($ENV_FILE) not found. Exiting."
  exit 1
fi

# Update the Lambda function code
echo "Updating code for Lambda function: $FUNCTION_NAME..."
aws lambda update-function-code \
  --function-name "$FUNCTION_NAME" \
  --zip-file "fileb://$ZIP_FILE"

if [ $? -eq 0 ]; then
  echo "Successfully updated code for Lambda function: $FUNCTION_NAME."
else
  echo "Failed to update code for Lambda function: $FUNCTION_NAME."
  exit 1
fi

# Update the Lambda function environment variables
echo "Updating environment variables for Lambda function: $FUNCTION_NAME..."
aws lambda update-function-configuration \
  --function-name "$FUNCTION_NAME" \
  --environmen=file://$ENV_FILE