## Work In Process Host Example

### How to set up Otel Collector on a Linux host 
- Download and install latest CONTRIB release version
from here: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)  

Collector must be configured with Coralogix Exporter.  
See the `config.yaml` example in [./otelcol](./otelcol)

You can check Collector status with these status URLS:  
http://localhost:55679/debug/tracez  
http://localhost:55679/debug/pipelinez  
For more info: [zpages docs](https://github.com/open-telemetry/opentelemetry-collector/blob/main/extension/zpagesextension/README.md)  


#### Run Node client in new terminal

This will make 250 requests of `https://api.github.com` and then exit.


in [./node](./node) subdir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```
