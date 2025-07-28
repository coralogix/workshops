# Profiling

## Requirements  
Prerequisites [here](https://coralogix.github.io/workshops/prereqs/)  
eBPF in this context only runs on Kubernetes with modern Linux kernel based containers.  
Note that eBPF integration w/ profiling is currently in a beta state and is subject to change.  

## Official Documentation [here](https://coralogix.com/docs/user-guides/continuous-profiling/setup/)  
Please read the [official documentation](https://coralogix.com/docs/user-guides/continuous-profiling/setup/) thoroughly before proceeding.

## Instructions  
 
This workshop is designed to add the eBPF agent to an existing installation of the Coralogix OpenTelemetry helm chart and then deploy simple applications in Java to demonstrate the spans and APM / profiling capabilities available with eBPF.

### Install the OpenTelemetry Collector on your k8s cluster
   
[Easy Coralogix instructions for Complete Observability are here](https://coralogix.com/docs/otel-collector-for-k8s/)  

### Clone Repo
```
git clone https://github.com/coralogix/workshops
```

### Change to Profiling Directory  
```
cd workshops/workshops/otel/profiling
```  

### Step 1 - Override the OpenTelemetry Collector with Continuous Profiling
Edit `override-otel.yaml` and update the globals for your domain and cluster name then:  
```
source 1-deploy-override.sh
```  

### Step 2 - Deploy Example Applications  
We will reuse a zero-code instrumentaiton example from the `autoinjection` workshop.   
  
This application has a client and server in the deployment, and the server has random slowdowns to study with profiling.  
Consult the [README](https://github.com/coralogix/workshops/blob/master/workshops/otel/autogenerators/java/microservices/README.md) (also found here workshops/workshops/otel/autogenerators/java/microservices/README.md) for more details on the architecture and frameworks used. 
```
cd workshops/workshops/otel/autogenerators/java/microservices
source deploy-java-autogen.sh
```

### Study The Results  
  
Coralogix->APM shows the working service catalog.  
  
Click on the `cx-java-server` service and select Profiles to see the profiles. Consult the [official documentation](https://coralogix.com/docs/user-guides/continuous-profiling/setup/) for more details.  

### Step 3 - Cleanup  

Delete the app examples:  
```
source delete-java-autogen.sh
```  
Return to the profiling workshop and roll back  the otel collector
```
cd workshops/workshops/otel/profiling
source 2-rollback-otel-collector.sh
```