unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_METRICS_EXPORTER
unset OTEL_TRACES_EXPORTER
unset OTEL_NODE_RESOURCE_DETECTORS
unset OTEL_EXPORTER_OTLP_ENDPOINT
unset OTEL_SERVICE_NAME
export OTEL_RESOURCE_ATTRIBUTES=service.name=cx-python-client,cx.application.name=cx-python-client,cx.subsystem.name=cx-pytho
n-client
sudo rm -rf /var/log/cx
sudo mkdir -p /var/log/cx
sudo chown steve_lerner:steve_lerner /var/log/cx
sudo chmod 755 /var/log/cx