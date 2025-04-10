#!/bin/bash

# Clean up any previous virtual environment
rm -rf env

# Create and activate a new virtual environment using Python 3.12
python3 -m venv env
source env/bin/activate

# Upgrade pip and install required packages
pip install --upgrade pip
pip install requests "Flask<3" "Werkzeug<3" serverless-wsgi

# Deactivate the environment
deactivate
