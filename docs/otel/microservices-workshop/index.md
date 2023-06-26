# Microservices Observability Demo (In Development)

## Instructions

Requires a completely operational OpenTelemetry collector configured for Coralogix: https://coralogix.com/docs/opentelemetry-using-kubernetes/  
This example is for basic study only and is not documentation.  
For full documentation: https://coralogix.com/docs/  

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd workshops/otel/microservices-demo
```

### Step 3 - Deploy example
Deploy example to your k8s cluster- this will deploy to the default k8s namespace.  
If you want to change the namespace, edit `yaml/deploy-good.yaml`  
There will be three services spun up:  

`cx-client-py-reqs` - a requesting service initiating a transaction  
`cx-flask-server` - a server that is a bridge for a transaction to a database  
`cx-redis` - an instance of a redis database  

```
source deploy-all.sh
```

### Step 4 - Study results and simulate CI/CD scenarios
Study results in Coralogix portal

Simulate a "bad" deployment:  
```
source deploy-bad.sh
```

This deployment will cause severe sporadic problems in `cx-flask-server` such as 404s, a log key:value ` 'transaction': 'failed',` and latency in the service response.  
  
Roll back the bad deployment for the services to return to normal:  
```
source deploy-good.sh
```

Study how the example is built:  
- The Python apps that drive this example are in the `python/apps` dir  
- `.yaml` deployment files are in `python/yaml`  
- Dockerfiles for the containers show how the OpenTelemetry instrumentation works and are in the `/python` root level  

### Step 5 - Cleanup
To remove all the deployments/services/pods from the example from your k8s cluster:  
```
source delete-all.sh
```