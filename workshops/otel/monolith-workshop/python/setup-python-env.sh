export OTEL_RESOURCE_ATTRIBUTES=service.name=service-python,application.name=app-python,api.name=subsystem-python,cx.application.name=app-python,cx.subsystem.name=subsystem-python
export OTEL_TRACES_EXPORTER=otlp
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp
export OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED=true
export OTEL_PYTHON_LOG_CORRELATION=true
