FROM debian:trixie-slim

# Create working directory
RUN mkdir -p /app

# Set working directory
WORKDIR /app

# Update package lists and install necessary packages and dependencies
RUN apt-get update -y && \
    apt-get install -y gcc make autoconf php-pear php-dev php-cli php-curl unzip curl

# Install the OpenTelemetry PHP extension
RUN pecl install opentelemetry

# Append the OpenTelemetry extension to the active php.ini file
RUN php_ini_path=$(php -i | grep "Loaded Configuration File" | awk '{print $NF}') && \
    echo -e "\nextension=opentelemetry.so" >> "$php_ini_path"

# Download and install Composer
RUN php -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    php composer-setup.php && \
    php -r "unlink('composer-setup.php');" && \
    mv composer.phar /usr/local/bin/composer

# Install required Composer packages, including Guzzle
RUN composer require \
    symfony/http-client \
    guzzlehttp/guzzle \
    nyholm/psr7 \
    http-interop/http-factory-guzzle \
    open-telemetry/sdk \
    open-telemetry/opentelemetry-auto-slim \
    php-http/guzzle7-adapter \
    --no-interaction --quiet

# Create Laravel project
RUN composer create-project --quiet laravel/laravel otel-php-laravel-basic-example

# Clean up unnecessary files
RUN apt-get autoremove -y && \
    apt-get autoclean -y && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Set working directory to Laravel project
WORKDIR /app/otel-php-laravel-basic-example