FROM ubuntu

# Create working directory
RUN mkdir -p /home/code

# Copy application and entrypoint script
COPY index.php /home/code/
COPY entrypoint.sh /home/code/
COPY php.ini /home/code/

# Set working directory
WORKDIR /home/code

# Update package lists
RUN apt-get update -y

# Install necessary packages and dependencies
RUN apt-get install -y gcc make autoconf php-pear php-dev php-cli unzip curl

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

# Install required Composer packages
RUN composer require \
    open-telemetry/sdk \
    open-telemetry/opentelemetry-auto-slim \
    open-telemetry/exporter-otlp \
    php-http/guzzle7-adapter \
    --no-interaction --quiet

# Clean up unnecessary packages and cache
RUN apt-get autoremove -y && \
    apt-get autoclean -y

# Ensure entrypoint.sh is executable
RUN chmod +x /home/code/entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
