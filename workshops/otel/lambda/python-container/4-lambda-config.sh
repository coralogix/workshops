#!/bin/bash

# Function name and region
FUNCTION_NAME="slerner-python-container"
AWS_REGION="us-west-2"

# Timeout in seconds
TIMEOUT=900

# Environment variables
CX_API_KEY=""
CX_DOMAIN="cx498.coralogix.com"
CX_APPLICATION="LambdaContainer"
CX_SUBSYSTEM="LambdaContainer"
CX_TAGS_ENABLED="TRUE"
CX_TRACING_MODE="OTEL"
CX_REPORTING_STRATEGY="REPORT_AFTER_INVOCATION"
# OTEL_TRACES_EXPORTER="otlp"
OTEL_METRICS_EXPORTER="none"
OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
OTEL_SERVICE_NAME="python-container-lambda"
OTEL_RESOURCE_ATTRIBUTES="service.name=python-container-lambda"

# Function to check if the Lambda function exists
function_exists() {
    aws lambda get-function --function-name "$FUNCTION_NAME" --region "$AWS_REGION" > /dev/null 2>&1
    return $?
}

# Validate that the Lambda function exists
if function_exists; then
    echo "Lambda function $FUNCTION_NAME found in region $AWS_REGION."

    # Update the Lambda function configuration with environment variables and timeout
    echo "Updating Lambda function configuration with environment variables and timeout..."
    aws lambda update-function-configuration \
        --function-name "$FUNCTION_NAME" \
        --region "$AWS_REGION" \
        --timeout "$TIMEOUT" \
        --environment "Variables={
            CX_API_KEY=$CX_API_KEY,
            CX_DOMAIN=$CX_DOMAIN,
            CX_APPLICATION=$CX_APPLICATION,
            CX_SUBSYSTEM=$CX_SUBSYSTEM,
            CX_TAGS_ENABLED=$CX_TAGS_ENABLED,
            CX_TRACING_MODE=$CX_TRACING_MODE,
            CX_REPORTING_STRATEGY=$CX_REPORTING_STRATEGY,
            OTEL_TRACES_EXPORTER=$OTEL_TRACES_EXPORTER,
            OTEL_METRICS_EXPORTER=$OTEL_METRICS_EXPORTER,
            OTEL_EXPORTER_OTLP_PROTOCOL=$OTEL_EXPORTER_OTLP_PROTOCOL,
            OTEL_EXPORTER_OTLP_ENDPOINT=$OTEL_EXPORTER_OTLP_ENDPOINT,
            OTEL_SERVICE_NAME=$OTEL_SERVICE_NAME,
            OTEL_RESOURCE_ATTRIBUTES=$OTEL_RESOURCE_ATTRIBUTES
        }"

    if [ $? -eq 0 ]; then
        echo "Lambda function configuration updated successfully."
    else
        echo "Error: Failed to update Lambda function configuration."
    fi
else
    echo "Error: Lambda function $FUNCTION_NAME not found in region $AWS_REGION."
fi

echo "Script completed."
