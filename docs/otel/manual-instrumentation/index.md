# Manual Instrumentation

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  

## OpenTelemetry Manual Instrumentation 

OpenTelemetry has SDKs and APIs that allow for the manual creation export of metrics, logs, and traces.  
(Full documentation here here: https://opentelemetry.io/docs/languages/)[https://opentelemetry.io/docs/languages/]  

## Coralogix Manual Instrumentaiton Examples    
 
Each example contains the following:  

**All Examples**  
- Application source code  
- Various scripts useful in building, deploying, and running the app. These vary but are labelled clearly i.e. `deploy.sh` and `delete.sh`.  
- Scripts sometimes in order to accomplish a goal i.e. `1-configure.sh` and `2-build.sh`  
- Each script needs to be checked for variables that need to be changed in your environment  
- Notes about each one ***on this page*** or see the README.md with each example   
  
**Container Examples**  
- `buildcontainer.sh` script for building a container  
- `Dockerfile` to demonstrate containerization  
- .yaml deployment example for Kubernetes  
- Although the container examples are for Kubernetes, you can port them to any container style environment so long as required ports are open and there are no prohibitions for the deployment making requests of itself.  
  
**Host Examples**  
- Env variable and other setup/run scripts  
- Otel collector `config.yaml` if needed  

**Serverless**  
- OpenTelemetry on serverless runs as a virtual OpenTelemetry collector 
- Set export destination to `localhost` and appropriate port for telemetry  
  
These examples are under frequent revision so please open an issue with any bug reports.  

## The Examples 

### Step 1 - Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Step 2 - Change to Examples Directory
```
cd workshops/workshops/otel/manual-instrumentation
```

`python`  
- Generates metrics and exports directly to an OpenTelemetry collector    
- Example is for host deployment- see the other examples for how to containerize   

`go-logs-metrics`  
- Golang app  
- Generates metrics and logs directly to an OpenTelemetry collector
- Can be run on host and has k8s deployment example as well

`ruby-logs-metrics`  
- Golang app  
- Generates metrics and logs directly to an OpenTelemetry collector
- Can be run on host and has k8s deployment example as well