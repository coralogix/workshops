## Work In Process Host Example

### Set Up Otel Collector On Linux Host 
- Download and install a Coralogix recommended release (currently 76.1) from here: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)
- Install using the debian / RPM method of your platform
- Back up `/etc/otelcol-contrig/config.yaml`:  
```
sudo cp /etc/otelcol-contrig/config.yaml /etc/otelcol-contrig/config-orig.yaml
```
- Replace and configure `/etc/otelcol-contrig/config.yaml` and make sure to update the `<variables>` from template config: [./otelcol/config.yaml](./otelcol/config.yanl)

### Example host based client and server tested on Debian Linux  

If you don't want ot configure Python and run a local Flask server, you can configure the destination URL in the Node client to be any public URL- just be careful because firewalls may eventually block this type of repetitive client request.  

in `python` dir:
```
source setup.sh
source start-flask.sh
```

#### run Node client in new terminal

See above- you can configure the node client to be any URL- the default is a local Flask server running as shown above.  

in `node` dir:
```
setup-node.sh
setup-node-env.sh
start-node.sh
```