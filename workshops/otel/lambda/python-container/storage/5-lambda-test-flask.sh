#!/bin/bash

# Variables
FUNCTION_NAME="slerner-python-container"

# Invoke the Lambda function and print the output to stdout
aws lambda invoke \
    --function-name $FUNCTION_NAME \
    --payload '{}' \
    --cli-binary-format raw-in-base64-out \
    >(cat) \
    2>&1
