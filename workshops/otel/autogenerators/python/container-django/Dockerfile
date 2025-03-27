FROM python:slim

# Create working directory
RUN mkdir -p /home/code
WORKDIR /home/code

# Install system utilities: curl and uuidgen (from uuid-runtime)
RUN apt-get update && \
    apt-get install -y curl uuid-runtime && \
    pip install --upgrade pip && \
    pip install requests "Flask<3" "Werkzeug<3" Django && \
    pip install opentelemetry-distro opentelemetry-instrumentation-django opentelemetry-exporter-otlp && \
    opentelemetry-bootstrap -a install && \
    apt-get -y autoremove && apt-get -y clean && rm -rf /var/lib/apt/lists/*

# Copy code and entrypoint
COPY . /home/code/
RUN chmod +x /home/code/entrypoint.sh

# Run the entrypoint
ENTRYPOINT ["/bin/sh", "/home/code/entrypoint.sh"]
