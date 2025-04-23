#!/bin/bash
# Install Python virtual environment and netcat
sudo apt install -y python3.12-venv

# Create and activate virtual environment
python3 -m venv env
source env/bin/activate

# Install dependencies
pip3 install openai mcp[cli] requests llm-tracekit 

# pip3 install opentelemetry-distro opentelemetry-exporter-otlp
# opentelemetry-bootstrap -a install