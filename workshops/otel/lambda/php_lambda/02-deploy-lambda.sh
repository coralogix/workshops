#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
ROLE_ARN="arn:aws:iam::104013952213:role/service-role/slerner-lambda"
ZIP_FILE="myFunction.zip"
RUNTIME="provided.al2023"
HANDLER="bootstrap"
ENV_FILE="env_vars.json"  # Path to the environment variables file
TIMEOUT=900  # Timeout in seconds (15 minutes)

# Create the Lambda function with environment variables from the file
echo "Creating the Lambda function: $FUNCTION_NAME..."
aws lambda create-function \
  --function-name "$FUNCTION_NAME" \
  --runtime "$RUNTIME" \
  --handler "$HANDLER" \
  --role "$ROLE_ARN" \
  --zip-file=fileb://$ZIP_FILE \
  --environment=file://$ENV_FILE \
  --timeout "$TIMEOUT"