#!/bin/bash

# Create virtual environment if it doesn't exist
if [ ! -d "env" ]; then
    python3 -m venv env
fi

# Activate virtual environment
source env/bin/activate

# Upgrade pip
pip install --upgrade pip

# Install Python dependencies
pip install -r requirements.txt
opentelemetry-bootstrap -a install
echo "Python environment setup complete!" 