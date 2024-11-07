FROM ubuntu

# Create working directory
RUN mkdir -p /home/code

# Copy application and entrypoint script
COPY index.php /home/code/
COPY entrypoint.sh /home/code/
COPY php.ini /home/code/

# Set working directory
WORKDIR /home/code

# Install necessary packages and dependencies
RUN apt-get update -y && \
    apt-get install -y gcc make autoconf php-pear php-dev php-cli unzip curl && \
    pecl install opentelemetry && \
    echo "\nextension=opentelemetry.so" >> $(php -i | grep 'php.ini' | awk '{print $5}') && \
    php -r "copy('https://getcomposer.org/installer', 'composer-setup.php');" && \
    php composer-setup.php && \
    php -r "unlink('composer-setup.php');" && \
    mv composer.phar /usr/local/bin/composer && \
    composer require \
    open-telemetry/sdk \
    open-telemetry/opentelemetry-auto-slim \
    open-telemetry/exporter-otlp \
    php-http/guzzle7-adapter \
    --no-interaction --quiet && \
    apt-get autoremove -y && \
    apt-get autoclean -y

# Ensure entrypoint.sh is executable
RUN chmod +x /home/code/entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
