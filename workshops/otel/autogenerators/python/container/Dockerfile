FROM python:slim

# Create working directory
RUN mkdir /home/code

# Copy application and entrypoint script
COPY app.py /home/code/
COPY entrypoint.sh /home/code/

# Set working directory
WORKDIR /home/code

# Install necessary packages and dependencies
RUN apt -y update && \
    export PATH="$HOME/.local/bin:$PATH" && \
    pip install --upgrade pip && \
    pip install requests "Flask <3" "Werkzeug <3" && \
    pip install opentelemetry-distro opentelemetry-exporter-otlp && \
    # pip install opentelemetry-distro==0.41b0 opentelemetry-exporter-otlp==1.20.0 && \
    opentelemetry-bootstrap -a install && \
    apt -y autoremove && apt-get -y autoclean

# Ensure entrypoint.sh is executable
RUN chmod +x /home/code/entrypoint.sh

# Set entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
