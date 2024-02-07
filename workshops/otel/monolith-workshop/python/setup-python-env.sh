export OTEL_RESOURCE_ATTRIBUTES=service.name=service-python,cx.application.name=app-python,cx.subsystem.name=subsystem-python
export OTEL_TRACES_EXPORTER=otlp
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp

export OTEL_PYTHON_LOG_CORRELATION=true
# export OTEL_PYTHON_LOG_FORMAT="%(msg)s [json.trace_id=%(span_id)s]"
export OTEL_PYTHON_LOG_LEVEL=DEBUG
export OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED=true
