#!/bin/bash

# Variables
FUNCTION_NAME="slerner-myFunction"
ROLE_ARN="arn:aws:iam::104013952213:role/service-role/slerner-lambda"
ZIP_FILE="myFunction.zip"
RUNTIME="provided.al2023"
HANDLER="bootstrap"

# Delete the Lambda function
echo "Deleting the Lambda function: $FUNCTION_NAME..."
aws lambda delete-function --function-name "$FUNCTION_NAME"