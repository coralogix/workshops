# JVM Metrics

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  

## JVM Metrics in OpenTelemetry

OpenTelemetry can gather JVM metrics in multiple ways:
- OpenTelemetry Collector Receiver for JMX Metrics
- Direct receipt of metrics from OpenTelemetry Java tracing instrumentation

In either case, a metrics dashboard provided in this workshop will be used to visualize these metrics.

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/autogenerators/java/monolith
```  

### Step 3 - Set up Otel Collector on a Linux host     
- update Coralogix exporter with region and key `config-demo-jmx.yaml`  
- download current release of otel collector: [https://github.com/open-telemetry/opentelemetry-collector-releases/releases](https://github.com/open-telemetry/opentelemetry-collector-releases/releases)
- update install script with downloaded version of otel collector and run script
```
source jmx-otel-config-setup.sh
```

### Step 4 - Add jvm dashboard to Custom Dashbords
```
jvm_metrics.json
```  

### Step 5 - Add jvm dashboard to Custom Dashbords
Run the included Java app example  

```
source 4-monolith-install-otel.sh
source 5-monolith-run-app.sh
```
