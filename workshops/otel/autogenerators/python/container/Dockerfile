FROM python:slim

# Create working directory
RUN mkdir /home/code

# Copy application
COPY app.py /home/code/

# Set working directory
WORKDIR /home/code

# Install necessary packages and dependencies
RUN apt -y update && \
    export PATH="$HOME/.local/bin:$PATH" && \
    pip install --upgrade pip && \
    pip install requests "Flask <3" "Werkzeug <3" && \
    pip install opentelemetry-distro opentelemetry-exporter-otlp && \
    opentelemetry-bootstrap -a install && \
    apt -y autoremove && apt-get -y autoclean

# Set entrypoint
ENTRYPOINT ["opentelemetry-instrument", "python3", "app.py"]
