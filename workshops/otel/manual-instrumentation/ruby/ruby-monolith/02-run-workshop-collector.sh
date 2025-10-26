#!/bin/bash

# Ruby OpenTelemetry Workshop Runner
# Following: https://opentelemetry.io/docs/languages/ruby/getting-started/

# Note: Not using 'set -e' to allow graceful exit with Ctrl+C

# Handle Ctrl+C gracefully
trap 'echo -e "\n\nWorkshop stopped. Terminal remains open."; exit 0' INT

echo "=== Ruby OpenTelemetry Workshop ==="

# Set up PATH for user-installed gems
export PATH="$HOME/.local/share/gem/ruby/3.2.0/bin:$PATH"

# Navigate to the dice-ruby application
cd dice-ruby

# Configuration for OpenTelemetry Collector
OTEL_EXPORTER_OTLP_ENDPOINT="${OTEL_EXPORTER_OTLP_ENDPOINT:-http://localhost:4318}"
OTEL_SERVICE_NAME="${OTEL_SERVICE_NAME:-dice-ruby-workshop}"

echo "Starting Rails application with OpenTelemetry OTLP exporter..."
echo "Sending traces to OpenTelemetry Collector at: $OTEL_EXPORTER_OTLP_ENDPOINT"
echo "Service name: $OTEL_SERVICE_NAME"
echo ""
echo "Available endpoints:"
echo "  - http://localhost:8081/rolldice - Roll a dice (1-6)"
echo "  - http://localhost:8081/up - Health check"
echo ""
echo "In another terminal, test the endpoints:"
echo "  curl http://localhost:8081/rolldice"
echo "  curl http://localhost:8081/up"
echo ""
echo "Environment variables (override as needed):"
echo "  OTEL_EXPORTER_OTLP_ENDPOINT=$OTEL_EXPORTER_OTLP_ENDPOINT"
echo "  OTEL_SERVICE_NAME=$OTEL_SERVICE_NAME"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

# Run with OTLP exporter to collector
env OTEL_EXPORTER_OTLP_ENDPOINT="$OTEL_EXPORTER_OTLP_ENDPOINT" \
    OTEL_SERVICE_NAME="$OTEL_SERVICE_NAME" \
    rails server -p 8081
