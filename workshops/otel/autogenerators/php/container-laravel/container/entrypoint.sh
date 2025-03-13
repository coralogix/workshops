#!/bin/bash

# Navigate to Laravel project
cd /home/code/laravel-app

# Start Laravel server in the background
php artisan serve --host=0.0.0.0 --port=8000 &

# Wait for the server to start
sleep 5

# Infinite loop to make requests every second
while true; do
  curl http://localhost:8000
  echo ""  # Add a newline for readability
  sleep 1  # Wait for 1 second before the next request
done
