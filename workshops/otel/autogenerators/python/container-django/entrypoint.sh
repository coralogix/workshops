#!/bin/sh

# Start Django server in the background
echo "Starting Django server..."
opentelemetry-instrument python3 django_app/manage.py runserver 0.0.0.0:5000 &

# Wait a moment for the server to become available
sleep 2

# Loop and send requests to the Django server
echo "Starting client loop to send requests to Django..."
while true; do
  REQUEST_ID=$(uuidgen)
  echo "Sending request with ID: $REQUEST_ID"
  curl -s -H "X-Request-ID: $REQUEST_ID" http://127.0.0.1:5000 > /dev/null
  sleep 0.3
done
