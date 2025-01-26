composer init \
  --no-interaction \
  --require slim/slim:"^4" \
  --require slim/psr7:"^1"
composer update
composer require guzzlehttp/guzzle
# composer require open-telemetry/api open-telemetry/sdk open-telemetry/exporter-otlp
composer config allow-plugins.php-http/discovery false
composer require \
  open-telemetry/sdk \
  open-telemetry/opentelemetry-auto-slim \
  open-telemetry/exporter-otlp
composer show