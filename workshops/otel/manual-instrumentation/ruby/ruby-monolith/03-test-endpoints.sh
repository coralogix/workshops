#!/bin/bash

# Test script for Ruby OpenTelemetry Workshop
# Run this in a separate terminal while the Rails app is running

echo "=== Testing Ruby OpenTelemetry Workshop Endpoints ==="
echo ""

# Check which ports are available
CONSOLE_PORT=8080
COLLECTOR_PORT=8081

echo "Checking for running workshop instances..."
if curl -s http://localhost:$CONSOLE_PORT/up > /dev/null 2>&1; then
    echo "Found console workshop running on port $CONSOLE_PORT"
    PORT=$CONSOLE_PORT
elif curl -s http://localhost:$COLLECTOR_PORT/up > /dev/null 2>&1; then
    echo "Found collector workshop running on port $COLLECTOR_PORT"
    PORT=$COLLECTOR_PORT
else
    echo "No workshop instance found running. Please start one of:"
    echo "  ./debug-run-workshop-console.sh (port 8080)"
    echo "  ./02-run-workshop-collector.sh (port 8081)"
    exit 1
fi

echo ""
echo "Testing health check endpoint..."
curl -s http://localhost:$PORT/up
echo ""
echo ""

echo "Testing dice rolling endpoint (5 times)..."
for i in {1..5}; do
    echo "Roll $i: $(curl -s http://localhost:$PORT/rolldice)"
    sleep 1
done

echo ""
echo "Check the Rails server console to see the OpenTelemetry traces!"
echo "Each request should generate trace spans showing:"
echo "  - HTTP request details"
echo "  - Controller action execution"
echo "  - View rendering (if applicable)"
echo "  - Database queries (if any)"
