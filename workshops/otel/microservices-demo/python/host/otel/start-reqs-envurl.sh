export PYTHON_TEST_URLGOOD=GOOD
export PYTHON_TEST_URL="http://localhost:5001"
opentelemetry-instrument \
    --traces_exporter console \
    --metrics_exporter console \
    --logs_exporter console \
    --service_name cx-pythong-reqs \
    python3 ../../apps/python-reqs.py