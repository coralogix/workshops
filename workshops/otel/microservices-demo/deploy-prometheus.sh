helm upgrade otel-coralogix-integration coralogix/otel-integration --values yaml/override-prometheus.yaml 
kubectl apply -f yaml/deploy-prometheus.yaml