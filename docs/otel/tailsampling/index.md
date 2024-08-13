# Tail Sampling

## Instructions

Complete the Microservices Workshop or install the OpenTelemetry Collector on your k8s cluster: [Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Step 1 - Deploy Python Autogenerator

- In `/workshops/workshops/otel/autogenerators/python/container` directory in repo  
- Configure `.yaml` with your globals  
- Deploy the Python autogenerator  
```
source deploy-autogen-py.sh 
```   
### Step 2 - Configure and Deploy Tail Sampling Example

- In `/workshops/workshops/otel/microservices/tailsampling` directory in repo  
- Configure `.yaml` with your globals  
- Deploy the example  
```
source deploy-tailsampling.sh 
```   
### Step 3 - Study results in Coralogix  
- Notice that trace quantities have been reduced 90%  
- This example is based on Coralogix [official documentation for Tail Sampling](https://coralogix.com/docs/tail-sampling-with-opentelemetry-using-kubernetes/)