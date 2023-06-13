# Microservices Observability Demo (In Development)

## Instructions

Requires a completely operational OpenTelemetry collector configured for Coralogix: https://coralogix.com/docs/opentelemetry-using-kubernetes/  
This example is for basic study only and is not documentation.  
For full documentation: https://coralogix.com/docs/  

### Step 1  
Clone repo:
```
git clone https://github.com/coralogix/workshops
```

### Step 2
Change to the proper directory for workshop example:  

```
cd workshops/otel/microservices-demo
```

### Step 3  
Deploy example to your k8s cluster- this will deploy to the default k8s namespace.  
If you want to change the namespace, edit `yaml/deploy-good.yaml`  
There will be three services spun up:  

cx-client-py-reqs - a requesting service initiating a transaction  
cx-flask-server - a server that is a bridge for a transaction to a database  
cx-redis - an instance of a redis database  


```
source deploy-all.sh
```

### Step 4
Study results in Coralogix portal

### Step 5
To remove all the deployments/services/pods from the example from your k8s cluster:  
```
source delete-all.sh
```