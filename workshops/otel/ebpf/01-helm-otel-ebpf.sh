helm repo add coralogix https://cgx.jfrog.io/artifactory/coralogix-charts-virtual
helm repo update
helm upgrade --install otel-coralogix-integration coralogix/otel-integration \
 --render-subchart-notes \
 --set opentelemetry-ebpf-instrumentation.enabled=true \
 --set global.domain="cx498.coralogix.com" \
 --set global.clusterName="slerner-cluster"