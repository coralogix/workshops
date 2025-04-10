#!/bin/bash

FUNCTION_NAME="slerner-python"
ZIP_FILE="function.zip"
RUNTIME="python3.12"
ROLE_ARN="arn:aws:iam::104013952213:role/service-role/slerner-lambda"
# ROLE_ARN="arn:aws:iam::<your-account-id>:role/<your-lambda-execution-role>"
HANDLER="lambda_function.lambda_handler"  # Update if your handler is different

# Check if the function exists
aws lambda get-function --function-name "$FUNCTION_NAME" > /dev/null 2>&1

if [ $? -ne 0 ]; then
  echo "Function $FUNCTION_NAME does not exist. Creating and updating it..."

  aws lambda create-function \
    --function-name "$FUNCTION_NAME" \
    --runtime "$RUNTIME" \
    --role "$ROLE_ARN" \
    --handler "$HANDLER" \
    --zip-file "fileb://$ZIP_FILE"

  aws lambda update-function-code \
    --function-name "$FUNCTION_NAME" \
    --zip-file "fileb://$ZIP_FILE"

else
  echo "Function $FUNCTION_NAME exists. Updating code..."

  aws lambda update-function-code \
    --function-name "$FUNCTION_NAME" \
    --zip-file "fileb://$ZIP_FILE"
fi