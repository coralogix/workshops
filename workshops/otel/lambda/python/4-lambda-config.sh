#!/bin/bash

# Function name and region
FUNCTION_NAME="slerner-python"
AWS_REGION="us-west-2"

# Layer ARN
LAYER_ARN="arn:aws:lambda:$AWS_REGION:625240141681:layer:coralogix-python-wrapper-and-exporter-x86_64:24"

# Timeout in seconds
TIMEOUT=900

# Function to check if the Lambda function exists
function_exists() {
    aws lambda get-function --function-name "$FUNCTION_NAME" --region "$AWS_REGION" > /dev/null 2>&1
    return $?
}

# Validate that the Lambda function exists
if function_exists; then
    echo "Lambda function $FUNCTION_NAME found in region $AWS_REGION."

    echo "Updating Lambda function configuration with environment variables and timeout..."
    aws lambda update-function-configuration \
        --function-name "$FUNCTION_NAME" \
        --region "$AWS_REGION" \
        --timeout "$TIMEOUT" \
        --environment '{
            "Variables": {
                "CX_API_KEY": "YOURKEY",
                "CX_DOMAIN": "cx498.coralogix.com",
                "AWS_LAMBDA_EXEC_WRAPPER": "/opt/otel-handler",
                "CX_REPORTING_STRATEGY": "REPORT_AFTER_INVOCATION",
                "CX_LOGS_METADATA_INCLUDE_TRACE_REF": "true",
                "CX_TRACES_RESOURCE_EXTRA_ATTRIBUTES": "testkey=testvalue"
            }
        }'

    if [ $? -eq 0 ]; then
        echo "Lambda function configuration updated successfully."
    else
        echo "Error: Failed to update Lambda function configuration."
    fi

    echo "Updating Lambda function to use the specified layer ARN..."
    aws lambda update-function-configuration \
        --function-name "$FUNCTION_NAME" \
        --region "$AWS_REGION" \
        --layers "$LAYER_ARN"

    if [ $? -eq 0 ]; then
        echo "Lambda function updated to use the specified layer ARN."
    else
        echo "Error: Failed to update the Lambda function with the specified layer."
    fi
else
    echo "Error: Lambda function $FUNCTION_NAME not found in region $AWS_REGION."
fi

echo "Script completed."
