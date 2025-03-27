#!/bin/bash

URL="http://localhost:8000"

echo "Starting curl loop to $URL every 0.5 seconds..."
echo "Press Ctrl+C to stop."

while true; do
    echo -e "\n[$(date)]"
    curl -s -i "$URL"
    sleep 0.5
done
