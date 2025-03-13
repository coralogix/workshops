#!/bin/bash

# Navigate to Laravel project
cd /home/code/laravel-app

# Ensure dependencies are installed
if [ ! -f "vendor/autoload.php" ]; then
  echo "Composer dependencies not found. Running composer install..."
  composer install --no-interaction --quiet
fi

# Run standalone test script
echo "Running standalone Laravel autoload test..."
php scripts/test.php || echo "Autoload test failed."

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
