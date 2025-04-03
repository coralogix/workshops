FROM python:slim

# Create working directory
WORKDIR /home/code

# Copy application code
COPY . /home/code/

# Install system dependencies and Python packages
RUN apt-get update && \
    apt-get install -y curl gcc build-essential && \
    pip install --upgrade pip && \
    pip install \
        openai>=1.0.0 \
        python-dotenv \
        tiktoken \
        opentelemetry-distro \
        opentelemetry-exporter-otlp && \
    opentelemetry-bootstrap -a install && \
    apt-get remove -y gcc build-essential && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set default entrypoint to use OpenTelemetry instrumentation
ENTRYPOINT ["opentelemetry-instrument", "python3", "main.py"]