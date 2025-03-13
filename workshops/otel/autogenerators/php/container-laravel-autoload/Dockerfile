FROM debian:trixie-slim

# Create working directory
RUN mkdir -p /home/code

# Set working directory
WORKDIR /home/code

# Update package lists and install necessary packages and dependencies
RUN apt-get update -y && \
    apt-get install -y \
    gcc make autoconf php-pear php-dev php-cli php-curl unzip curl git php-mbstring php-xml php-bcmath php-tokenizer php-zip php-sqlite3

# Install the OpenTelemetry PHP extension
RUN pecl install opentelemetry && \
    php_ini_path=$(php -i | grep "Loaded Configuration File" | awk '{print $NF}') && \
    echo -e "\nextension=opentelemetry.so" >> "$php_ini_path"

# Download and install Composer
RUN php -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    php composer-setup.php && \
    php -r "unlink('composer-setup.php');" && \
    mv composer.phar /usr/local/bin/composer

# Install Laravel globally
RUN composer global require laravel/installer

# Install Laravel application
RUN composer create-project --prefer-dist laravel/laravel laravel-app && \
    chmod -R 777 /home/code/laravel-app/storage /home/code/laravel-app/bootstrap/cache

# Install required Composer packages, including OpenTelemetry
WORKDIR /home/code/laravel-app
RUN composer require \
    symfony/http-client \
    guzzlehttp/guzzle \
    nyholm/psr7 \
    http-interop/http-factory-guzzle \
    open-telemetry/sdk \
    open-telemetry/opentelemetry-auto-slim \
    open-telemetry/exporter-otlp \
    open-telemetry/opentelemetry-auto-laravel \
    php-http/guzzle7-adapter \
    --no-interaction --quiet

# Copy standalone test script
COPY scripts/test.php /home/code/laravel-app/scripts/test.php

# Expose Laravel's default port
EXPOSE 8000

# Copy entrypoint script
COPY entrypoint.sh /home/code/entrypoint.sh
RUN chmod +x /home/code/entrypoint.sh

# Set default entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
