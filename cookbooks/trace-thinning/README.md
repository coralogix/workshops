To thin Otel Collector traces without sampling, here is a suggested example to remove specific keys that are not critical for observability:

Add to the `processor` section of the Otel Collector `config.yaml`  

```
transform:
  trace_statements:
    - context: span
      statements:
      - delete_key(attributes, "thread.name") 
      - delete_key(resource.attributes, "telemetry.sdk.version") 
      - delete_key(resource.attributes, "telemetry.sdk.name")
      - delete_key(resource.attributes, "otel.scope.name")
      - delete_key(resource.attributes, "otel.library.name") 
      - delete_key(resource.attributes, "otel.library.version") 
      - delete_key(resource.attributes, "k8s.pod.uid")
      - delete_key(resource.attributes, "host.id")
      - delete_key(resource.attributes, "k8s.pod.ip")
      - delete_key(resource.attributes, "cloud.platform")   
      - delete_key(resource.attributes, "os.type")
```  

Configure the service `pipelines` section of the Otel Collector `config.yaml` by adding the `transform` processor before the `batch` processor- this example will vary based on your config.

```         
service:
  pipelines:
    processors:
    - memory_limiter
    - spanmetrics
    - transform
    - batch
```