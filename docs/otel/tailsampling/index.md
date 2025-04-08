# OpenTelemetry Tail Sampling

## Instructions

OpenTelemetry Tail Sampling is based on a processor described here: [https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/processor/tailsamplingprocessor/README.md](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/processor/)  
  
Coralogix Docs: [Tail Sampling](https://coralogix.com/docs/tail-sampling-with-opentelemetry-using-kubernetes/)  
    
### Step 0 - Configure and Deploy Tail Sampling Example  
- Install the OpenTelemetry Collector on your k8s cluster: [Coralogix instructions are here](https://coralogix.com/docs/otel-collector-for-k8s/)  
- Change directory to `/workshops/workshops/otel/microservices/tailsampling` directory in repo  
- Configure all of the `.yaml` with your globals: cluster and region  

### Step 1 - Deploy the two Python example trace generating apps  
```
source 01-deploy-autogen-py.sh 
```   

### Step 2 - Try sampling both apps at 10%  
- Study `tailsampling10.yaml` to see how this is configured  
- Once deployed notice in Coralogix **Explore->Tracing** how the sample rate has dropped  
```
source 02-deploy-tailsampling-10percent.sh
```   
### Step 3 - Try sampling both apps at 10%  
- Study `tailsampling-apps-diff.yaml` to see how this is configured  
- The example will sample `cx-py-1` at 80% and `cx-py-2` at 20%
- In order to have multiple conditions for different apps, the tailsampling processor uses **SubPolicies** that specify a condtion of a span i.e. `service.name` and the sample policy for that service  
- Study the official docs to see all configuraiton options and examples [https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/processor/tailsamplingprocessor/README.md](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/processor/tailsamplingprocessor/README.md)
- Once deployed notice in Coralogix **Explore->Tracing** how the sample rate has dropped  
```
source 03-deploy-tailsampling-app1-app2-diff.sh
```   

### Step 4 - Cleanup
- A helm rollback of `otel-coralogix-integration` to the initial release before this workshop can restore original values  