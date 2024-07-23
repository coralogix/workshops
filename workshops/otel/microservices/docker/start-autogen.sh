#!/bin/bash

# Define the image
IMAGE="public.ecr.aws/w3s4j9x9/microservices-demo:py-autogen"

# Pull the latest version of the image
echo "Pulling the latest version of $IMAGE..."
sudo docker pull $IMAGE

# Environment variables
ENV_VARS=(
    "-e OTEL_SERVICE_NAME=cx-py-autogen"
    "-e OTEL_RESOURCE_ATTRIBUTES=application.name=cx-docker-py-auto,\
    api.name=cx-docker-py-auto,\
    cx.application.name=cx-docker-py-auto,\
    cx.subsystem.name=cx-docker-py-auto"
)

# Ensure the log directory exists
mkdir -p /tmp/log

# Run the container with host networking, environment variables, custom entrypoint, redirect logs, in foreground, and with a custom name
echo "Running: otel-python-autogen ..."
sudo docker run --name otel-py-autogen \
--network host \
--entrypoint "/bin/sh" \
"${ENV_VARS[@]}" $IMAGE -c "opentelemetry-instrument --resource_attributes application.name=cx-docker-py-auto,api.name=cx-docker-py-auto,cx.application.name=cx-docker-py-auto,cx.subsystem.name=cx-docker-py-auto,service.name=cx-docker-py-auto python app.py" > /tmp/log/docker.log 2>&1