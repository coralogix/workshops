## Work In Process Host Example

### Set Up Otel Collector
Requires locally running otel collector correctly configured for Coralogix.  
- Download and install a Coralogix recommended release (currently 76.1) from here: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)
- Install using the debian / RPM method of your platform
- Back up `/etc/otelcol-contrig/config.yaml`:  
```
sudo cp /etc/otelcol-contrig/config.yaml /etc/otelcol-contrig/config-orig.yaml
```
- Replace and configure `/etc/otelcol-contrig/config.yaml` and make sure to update the `<variables` from template config: [./otelcol/config.yaml](./otelcol/config.yanl)

### Example host based client and server tested on Debian Linux  
#### Run Flask and Redis Server  
in `python` dir:
```
source setup.sh
source start-flask.sh
```

#### run Node client in new terminal
in `node` dir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```