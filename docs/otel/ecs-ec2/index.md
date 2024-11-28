# OpenTelemetry for AWS ECS-EC2

## Instructions

This example is for basic study only and is not documentation.    
Full documentation: [https://coralogix.com/docs/](https://coralogix.com/docs/)  
Requirements:  
- AWS Account
- AWS CLI
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
  
1 - There are Cloudformation scripts in order used to deploy the examples- change all obvious variables like AWS regions and VARIABLESINCAPS related to Coralogix or your environment`  
2 - Identify Coralogix Distro for OpenTelemetry container version [here before starting](https://hub.docker.com/r/coralogixrepo/coralogix-otel-collector/tags)  
3 - Study official documentation here: [https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2](https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/opentelemetry/ecs-ec2)
  
These two policies should be tuned for your own production security needs when going into production.   
  
### Step 4 - Execute workshop

Deploy the steps in order- but note that there are two examples- a Python and Node example branches as 04 and 04a and 05 and 05a... pick one or the other- although you can do both too!

### Step 5 - Study results in Coralogix portal

### Cleanup  
  
Use the delete scripts to clean up.