export OTEL_PYTHON_LOG_CORRELATION=true
export OTEL_PYTHON_LOG_LEVEL=INFO
opentelemetry-instrument \
    --traces_exporter console \
    --service_name cx-flask-server \
    python3 ../../apps/flask-server-redis.py