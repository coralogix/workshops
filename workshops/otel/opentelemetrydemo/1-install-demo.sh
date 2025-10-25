helm repo add open-telemetry https://open-telemetry.github.io/opentelemetry-helm-charts
helm repo update
helm install my-otel-demo open-telemetry/opentelemetry-demo \
    --set opentelemetry-collector.enabled=false \
    --set jaeger.enabled=false \
    --set prometheus.enabled=false \
    --set grafana.enabled=false \
    --set opensearch.enabled=false \
    --set-json 'default.envOverrides=[{"name":"OTEL_COLLECTOR_NAME","valueFrom":{"fieldRef":{"fieldPath":"status.hostIP"}}}]'