#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
ZIP_FILE="myFunction.zip"

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