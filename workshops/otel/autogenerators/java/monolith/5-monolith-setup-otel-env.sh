#!/bin/sh

# Set environment variables
export OTEL_SERVICE_NAME=cx-java-autogen
export OTEL_RESOURCE_ATTRIBUTES="cx.application.name=cx-java-autogen,cx.subsystem.name=cx-java-autogen"

# Print to verify
echo "OTEL_SERVICE_NAME is set to $OTEL_SERVICE_NAME"
echo "OTEL_RESOURCE_ATTRIBUTES is set to $OTEL_RESOURCE_ATTRIBUTES"
