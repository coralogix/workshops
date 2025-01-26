#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
LAYER_ARN="arn:aws:lambda:us-west-2:625240141681:layer:coralogix-aws-lambda-telemetry-exporter-x86_64:34"

# Update the Lambda function configuration to use the specified layer
echo "Updating function configuration for: $FUNCTION_NAME..."
aws lambda update-function-configuration \
  --function-name "$FUNCTION_NAME" \
  --layers "$LAYER_ARN"

# Retrieve and display the updated function configuration to confirm the layer update
echo "Fetching updated configuration for: $FUNCTION_NAME..."
aws lambda get-function-configuration \
  --function-name "$FUNCTION_NAME" \
  --query "Layers"
