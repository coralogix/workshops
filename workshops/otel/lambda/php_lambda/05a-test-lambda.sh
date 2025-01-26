#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"

# Invoke the Lambda function to test the tracer
aws lambda invoke \
  --function-name "$FUNCTION_NAME" \
  --cli-binary-format raw-in-base64-out \
  --payload '{"httpMethod":"GET","path":"/test-tracer","queryStringParameters":{},"headers":{"Content-Type":"application/json"}}' \
  /dev/stdout
