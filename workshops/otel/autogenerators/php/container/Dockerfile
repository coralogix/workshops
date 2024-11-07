FROM ubuntu
# Create working directory
RUN mkdir /home/code
# Copy application and entrypoint script
COPY index.php /home/code/
COPY entrypoint.sh /home/code/
# Set working directory
WORKDIR /home/code
# Install necessary packages and dependencies
RUN apt -y update && \
    export PATH="$HOME/.local/bin:$PATH" && \
    apt-get install -y gcc make autoconf php-pear php-dev php-cli unzip curl && \
    # curl -sS https://getcomposer.org/installer -o composer-setup.php && \
    # composer require \
    # open-telemetry/sdk \
    # open-telemetry/opentelemetry-auto-slim \
    # open-telemetry/exporter-otlp \
    # php-http/guzzle7-adapter \
    # --no-interaction --quiet && \
    apt -y autoremove && apt-get -y autoclean
# Ensure entrypoint.sh is executable
RUN chmod +x /home/code/entrypoint.sh
# Set entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]