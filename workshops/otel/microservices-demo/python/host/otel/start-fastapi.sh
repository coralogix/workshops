opentelemetry-instrument \
    --traces_exporter console \
    --metrics_exporter console \
    --logs_exporter console \
    --service_name cx-fastapi-server \
    python3 ../../apps/storage/fastapi-server.py