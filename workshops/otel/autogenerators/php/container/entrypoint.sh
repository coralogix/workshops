#!/bin/bash

# Start PHP built-in server in the background
php -S localhost:8080 &

# Wait a moment to ensure the server starts
sleep 2

# Infinite loop to make requests every second
while true; do
  curl localhost:8080/rolldice
  echo ""  # Add a newline for readability
  sleep 1  # Wait for 1 second before the next request
done