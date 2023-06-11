    pip3 install --upgrade pip && \
    pip3 install -r ../setup/requirements.txt && \
    pip3 install opentelemetry-distro opentelemetry-instrumentation opentelemetry-exporter-otlp pip install opentelemetry-instrumentation-logging && \
    opentelemetry-bootstrap -a install