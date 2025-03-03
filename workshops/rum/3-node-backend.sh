unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_METRICS_EXPORTER
unset OTEL_TRACES_EXPORTER
unset OTEL_NODE_RESOURCE_DETECTORS
unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_SERVICE_NAME
export OTEL_TRACES_EXPORTER="otlp"
export OTEL_METRICS_EXPORTER="otlp"
export OTEL_EXPORTER_OTLP_ENDPOINT="http://127.0.0.1:4318"
export OTEL_NODE_RESOURCE_DETECTORS="env,host,os"
export OTEL_RESOURCE_ATTRIBUTES="cx.application.name=cx-node-gateway,cx.subsystem.name=cx-node-gateway" 
export OTEL_SERVICE_NAME="cx-node-gateway"
export NODE_OPTIONS="--require @opentelemetry/auto-instrumentations-node/register"
node service.js