WORK IN PROCESS  

PHP Laravel Opentelemetry Container Instrumentation Demo  

Requires OpenTelemetry Collector installed on k8s configured for Coralogix  

1. Build container using `buildcontainer-php.sh` updated for your repo.  
2. Deploy using`kubectl apply -f deply-autogen-php.yaml` and make sure to update the container image for your repo.  
3. `kubectl exec it CONTAINERNAME -- sh` to shell into the container and then `curl localhost:8000` exercise laravel framework and traces should be sent to the local OpenTelemetry collector and on to the Coralogix portal.  

Note this is using the `http/json` OTLP exporter protocol and not GRPC.