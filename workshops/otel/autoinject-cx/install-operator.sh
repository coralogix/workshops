helm install opentelemetry-operator open-telemetry/opentelemetry-operator \
  --namespace coralogix \
  --set "manager.collectorImage.repository=otel/opentelemetry-collector-k8s"