#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"

# Invoke the Lambda function
aws lambda invoke \
  --function-name "$FUNCTION_NAME" \
  --cli-binary-format raw-in-base64-out \
  --payload '{"httpMethod":"GET","path":"/rolldice","queryStringParameters":{},"headers":{"Content-Type":"application/json"}}' \
  /dev/stdout
