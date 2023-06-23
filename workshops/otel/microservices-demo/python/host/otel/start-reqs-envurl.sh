export PYTHON_TEST_URLGOOD=GOOD
export PYTHON_TEST_URL="http://localhost:5000"
export OTEL_PYTHON_LOG_CORRELATION=true
export OTEL_PYTHON_LOG_LEVEL=debug
opentelemetry-instrument \
    --traces_exporter console \
    --service_name cx-python-reqs \
    python3 ../../apps/python-reqs.py