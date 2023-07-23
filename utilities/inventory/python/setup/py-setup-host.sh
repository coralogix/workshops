export PATH="$HOME/.local/bin:$PATH" 
pip install --upgrade pip
python3 -m pip install -r requirements.txt \
    opentelemetry-distro \
    opentelemetry-instrumentation \
    opentelemetry-exporter-otlp \
    opentelemetry-instrumentation-logging && \
    opentelemetry-bootstrap