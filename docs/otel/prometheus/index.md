# Prometheus for OpenTelemetry Collector

## Instructions

Complete the Microservices Workshop or install the OpenTelemetry Collector on your k8s cluster  
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Step 1 - Deploy the Prometheus example metric generators

From `/workshops/workshops/otel/microservices/prometheus` directory in repo:  
```
source deploy-prom-app.sh 
```  
Two deployments appear each generating random number gauges with metric names: `prom1` and `prom2`  
  
### Step 2 - Prometheus Simple Receiver

- ([Prometheus Simple Receiver Docs](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/simpleprometheusreceiver)) declares explicit targts for scraping metrics  
- Study the override file called `override-prometheus-simple.yaml` to understand how it works  
- Update the `Cluster` name and `Domain` to match your stack and deploy:    
```
source deploy-prom-simple.sh
```  
- You can now see metrics in Coralogix portal called `prom1` and `prom2`  
- To restore original OpenTelemetry Collector config:  
```
helm rollback otel-coralogix-integration 1 (or your previous helm version)
```  
### Step 3 - Prometheus Autodiscovery ReceiverCreator

- ([Prometheus Auto ReceiverCreator Docs](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver/receivercreator)) uses various features of OpenTelmetry collector to create an autodiscovery set of receivers based on watching for changes in deployments. This will find all self declared Prometheus endpoints in deployments.    
- Study the override file called `override-prometheus-auto.yaml` to understand how it works  
- Update the `Cluster` name and `Domain` to match your stack and deploy:    
```
source deploy-prom-auto.sh
```  
- You can now see metrics in Coralogix portal called `prom1` and `prom2`  
- To restore original OpenTelemetry Collector config:  
```
helm rollback otel-coralogix-integration 1 (or your previous helm version)
``` 
- To delete the Prometheus metrics deployment:
```
source delete-prom-app.sh
```  
  
## Cleanup

Roll back to original Collector config:  
```
helm rollback otel-coralogix-integration 1 (or your previous helm version)
``` 

From: `/workshops/workshops/otel/microservices/prometheus`  
```
source delete-all.sh
```