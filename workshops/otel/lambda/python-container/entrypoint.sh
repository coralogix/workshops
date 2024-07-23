#!/bin/bash

# set -e  # Exit immediately if a command exits with a non-zero status
# set -x  # Print commands and their arguments as they are executed

# Run opentelemetry-instrument and then start the Lambda function handler
opentelemetry-instrument python -m awslambdaric lambda_function.lambda_handler