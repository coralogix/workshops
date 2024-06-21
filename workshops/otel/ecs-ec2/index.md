### ECS EC2 Workshop  
  
#### Prep  
   
- Example ECS EC2 setup scripts to bring up and down an ECS cluster are included: `ecs-cli-up.sh` and `ecs-cli-down.sh`  
- Identify Coralogix Distro for OpenTelemetry container version [here before starting](https://hub.docker.com/r/coralogixrepo/coralogix-otel-collector/tags)  
- REQUIRED! Review all scripts and update variables IN CAPS 
- Study official documentation here: [https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2](https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2)
  
#### Workshop  
  
- Deploy Coralogix Otel Collector stack in ECS EC2 Cluster
```
source 1-ecs-deploy-otel.sh
```
- Deploy example Python application stack that will generate traces and logs  
```
source 2-ecs-createstack-autogen-otel.sh
```  
- Study logs, traces, and metrics in Coralogix portal  
 
#### Cleanup  
  
- Delete Python application stack   
```
source 3-ecs-deletestack-autogen-otel.sh  
``` 
- Delete Coralogix Otel Collector stack in ECS EC2 Cluster  
```
source 4-ecs-deletestack-otel.sh
```