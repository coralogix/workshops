FROM debian:trixie-slim

# Create working directory
RUN mkdir -p /home/code

# Copy application files and entrypoint scripts
COPY index.php /home/code/
COPY entrypoint.sh /home/code/

# Set working directory
WORKDIR /home/code

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
    open-telemetry/exporter-otlp \
    php-http/guzzle7-adapter \
    --no-interaction --quiet

# Clean up unnecessary packages and cache
RUN apt-get autoremove -y && \
    apt-get autoclean -y && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Ensure entrypoint scripts are executable
RUN chmod +x /home/code/*.sh

# Set default entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
