    sudo apt install -y redis
    export PATH="$HOME/.local/bin:$PATH" && \
    pip3 install --upgrade pip && \
    pip3 install -r requirements.txt && \
    pip3 install opentelemetry-distro opentelemetry-instrumentation opentelemetry-exporter-otlp opentelemetry-instrumentation-logging && \
    opentelemetry-bootstrap -a install