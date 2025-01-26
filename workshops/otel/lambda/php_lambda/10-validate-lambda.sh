#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"

# Get and display environment variables, layers, and runtime
echo "Fetching configuration details for: $FUNCTION_NAME..."
aws lambda get-function-configuration \
  --function-name "$FUNCTION_NAME" \
  --query "{Environment: Environment.Variables, Layers: Layers, Runtime: Runtime}" \
  --output json
