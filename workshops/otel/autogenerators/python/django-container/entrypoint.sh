#!/bin/bash

# Start Django with OpenTelemetry instrumentation in the background
opentelemetry-instrument python3 manage.py runserver --noreload &
DJANGO_PID=$!

# Wait for server to start
sleep 2

# Curl loop
URL="http://localhost:8000"
echo "Starting curl loop to $URL every 0.5 seconds..."
echo "Press Ctrl+C to stop."

while true; do
    echo -e "\n[$(date)]"
    curl -s -i "$URL"
    sleep 0.5
done

# (Optional) kill the server when the loop is killed
# trap "kill $DJANGO_PID" EXIT
