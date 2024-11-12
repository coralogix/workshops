#!/bin/bash

# Start the subapp on port 8081 in the background
php -S localhost:8081 subapp.php &

# Wait a moment to ensure the subapp starts
sleep 2

# Start the main app on port 8080 in the background
php -S localhost:8080 mainapp.php &

# Wait another moment to ensure the main app starts
sleep 2

# Infinite loop to make requests to the main app every second
while true; do
  curl localhost:8080/rolldice
  echo ""  # Add a newline for readability
  sleep 1  # Wait for 1 second before the next request
done
