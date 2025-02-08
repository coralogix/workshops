#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e  

echo "Installing OTEL gems"
gem install opentelemetry-sdk --user-install
gem install opentelemetry-exporter-otlp --user-install
gem install opentelemetry-logs-sdk --user-install
gem install opentelemetry-logs-api --user-install
gem install opentelemetry-exporter-otlp-logs --user-install
gem install opentelemetry-metrics-sdk --user-install
gem install opentelemetry-metrics-api --user-install
gem install opentelemetry-exporter-otlp-metrics --user-install


echo "Installing Bundler..."
gem install bundler --user-install

echo "Setting up gem environment..."
mkdir -p ~/.gem
export GEM_HOME="$HOME/.gem"
export PATH="$HOME/.gem/bin:$PATH"

echo "Configuring bundle..."
bundle config set --local path 'vendor/bundle'

echo "Installing dependencies..."
bundle install

echo "Setup complete!"