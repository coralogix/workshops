helm upgrade otel-coralogix-integration coralogix/otel-integration --values override-prometheus.yaml 
kubectl apply -f deploy-prometheus.yaml