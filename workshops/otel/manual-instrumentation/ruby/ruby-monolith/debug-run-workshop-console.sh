#!/bin/bash

# Ruby OpenTelemetry Workshop Runner (Console Version)
# Following: https://opentelemetry.io/docs/languages/ruby/getting-started/

# Note: Not using 'set -e' to allow graceful exit with Ctrl+C

# Handle Ctrl+C gracefully
trap 'echo -e "\n\nWorkshop stopped. Terminal remains open."; exit 0' INT

echo "=== Ruby OpenTelemetry Workshop (Console Traces) ==="

# Set up PATH for user-installed gems
export PATH="$HOME/.local/share/gem/ruby/3.2.0/bin:$PATH"

# Navigate to the dice-ruby application
cd dice-ruby

echo "Starting Rails application with OpenTelemetry console exporter..."
echo "The application will show traces in the console output."
echo ""
echo "Available endpoints:"
echo "  - http://localhost:8080/rolldice - Roll a dice (1-6)"
echo "  - http://localhost:8080/up - Health check"
echo ""
echo "In another terminal, test the endpoints:"
echo "  curl http://localhost:8080/rolldice"
echo "  curl http://localhost:8080/up"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

# Run with console trace exporter
env OTEL_TRACES_EXPORTER=console rails server -p 8080
