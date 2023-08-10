## Work In Process Host Example

### How to set up Otel Collector on a Linux host 
- Download and install latest release version
from here: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

Collector must be configured with Coralogix Exporter.  
See the `config.yaml` example in [./otelcol](./otelcol)

#### Run Node client in new terminal

This will make 250 requests of `https://api.github.com` and then exit.


in [./node](./node) subdir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```