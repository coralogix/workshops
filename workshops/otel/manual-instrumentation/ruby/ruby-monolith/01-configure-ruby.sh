#!/bin/bash

# Ruby OpenTelemetry Workshop Setup
# Following: https://opentelemetry.io/docs/languages/ruby/getting-started/

set -e

echo "=== Ruby OpenTelemetry Workshop Setup ==="

# Set up PATH for user-installed gems
export PATH="$HOME/.local/share/gem/ruby/3.2.0/bin:$PATH"

# Install Rails with user permissions
echo "Installing Rails..."
gem install rails --user-install

# Install bundler if not present
echo "Installing bundler..."
gem install bundler --user-install

echo "Ruby version: $(ruby --version)"
echo "Rails version: $(rails --version)"
echo "Bundler version: $(bundle --version)"

echo ""
if [ -d "dice-ruby" ]; then
    echo "dice-ruby application already exists. Skipping creation."
    echo "To recreate from scratch, delete the dice-ruby folder first."
else
    echo "Creating dice-ruby application with OpenTelemetry..."
    rails new --api dice-ruby
    cd dice-ruby

    echo "Installing OpenTelemetry dependencies..."
    bundle add opentelemetry-sdk opentelemetry-instrumentation-all opentelemetry-exporter-otlp
    
    echo "Setting up dice controller and routes..."
    # Add controller and routes setup here if needed
fi

echo ""
echo "Setup complete! Rails application with OpenTelemetry is ready."
echo "Make sure to run: export PATH=\"\$HOME/.local/share/gem/ruby/3.2.0/bin:\$PATH\""
