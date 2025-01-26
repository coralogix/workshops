#!/bin/bash

# Remove existing Composer files to reset
rm -rf composer.json composer.lock vendor/

# Reinitialize Composer and install the base dependencies
composer init \
  --no-interaction \
  --type="project" \
  --license="MIT" \
  --require slim/slim:"^4" \
  --require slim/psr7:"^1"

# Update dependencies
composer update --no-interaction

# Install additional required packages
composer require --no-interaction guzzlehttp/guzzle

# Disable PHP-HTTP Discovery plugin for compatibility
composer config --no-interaction allow-plugins.php-http/discovery false

# Install OpenTelemetry-related dependencies
composer require --no-interaction \
  open-telemetry/api \
  open-telemetry/sdk \
  open-telemetry/opentelemetry-auto-slim \
  open-telemetry/exporter-otlp

# Display installed packages for verification
composer show
