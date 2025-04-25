#!/bin/bash

# Run the Python application
# If an argument is provided, pass it to the script
if [ $# -eq 1 ]; then
    opentelemetry-instrument python main.py $1
else
    opentelemetry-instrument python main.py
fi
