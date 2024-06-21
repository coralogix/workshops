# OpenTelemetry for AWS ECS-EC2

## Instructions

This example is for basic study only and is not documentation.    
Full documentation: [https://coralogix.com/docs/](https://coralogix.com/docs/)  
Requirements:  
- AWS Account
- Intermediate to Advanced skill with ECS-EC2
- Sufficient permissions to configure AWS  
- Proper IDE i.e. Visual Studio Code 

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  
  
### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  
  
```
cd ./workshops/workshops/otel/ecs-ec2
```  
  
### Step 3 - Prep requirements  
  
1 - Example ECS EC2 setup scripts to bring up and down an ECS cluster are included: `ecs-cli-up.sh` and `ecs-cli-down.sh`  
2 - Identify Coralogix Distro for OpenTelemetry container version [here before starting](https://hub.docker.com/r/coralogixrepo/coralogix-otel-collector/tags)  
3 - REQUIRED! Review all scripts and update variables IN CAPS 
4 - Study official documentation here: [https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2](https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2)
  
These two policies should be tuned for your own production security needs when going into production.   
  
### Step 4 - Execute workshop

- Deploy Coralogix Otel Collector stack in ECS EC2 Cluster
```
source 1-ecs-deploy-otel.sh
```
- Deploy example Python application stack that will generate traces and logs  
- The `template.yaml` included in this repo from the [source repo](https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2) may change over time and if there are issues use the `template.yaml` from source repo instead.

```
source 2-ecs-createstack-autogen-otel.sh
```  

### Step 5 - Study results in Coralogix portal

### Cleanup  
  
- Delete Python application stack   
```
source 3-ecs-deletestack-autogen-otel.sh  
``` 
- Delete Coralogix Otel Collector stack in ECS EC2 Cluster  
```
source 4-ecs-deletestack-otel.sh
```