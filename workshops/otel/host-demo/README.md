## Work In Process Host Example

### How to set up Otel Collector on a Linux host 
- Download and install a Coralogix recommended release (currently 76.1) from here: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)
- Install using the Debian / RPM method of your platform
- Back up `/etc/otelcol-contrig/config.yaml`:  
```
sudo cp /etc/otelcol-contrig/config.yaml /etc/otelcol-contrig/config-orig.yaml
```
- Replace and configure `/etc/otelcol-contrig/config.yaml` and make sure to update the variable values from template `config.yaml` here: [./otelcol](./otelcol)
- If not using a contrib version of the collector, comment out the Coralogix exporter and uncomment the `otlp` exporter and configure its variables, and then replace the pipelines with the `otlp` exporter instead of `coralogix`.

### Tracing Example: Host based client and server tested on Debian Linux  

This example sets up a Python Flask Server and a Node requests client both which generate traces sent ot the Otel Collector.  
If you don't want to configure Python and run a local Flask server, you can configure the destination URL in the Node client to be any public URL- just be careful because firewalls may eventually block this type of repetitive client request.  

#### Flask server in new terminal

in [./python](./python) subdir:
```
source setup.sh
source start-flask.sh
```

#### Run Node client in new terminal

See above- you can configure the node client to be any URL- the default is a local Flask server running as shown above.  
*Make sure you do this in a new terminal and leave above Flask server running*

in [./node](./node) subdir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```