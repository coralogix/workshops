# OpenTelemetry for AWS ECS-Fargate

## Instructions

Additional Requirements:  
- AWS Account
- Intermediate to Advanced skill with ECS-Fargate  
- Sufficient permissions to configure AWS  

### Step 1 - Setup
Clone repo:
```
git clone https://github.com/coralogix/workshops
```  

### Step 2 - Change to workshop dir
Change to the proper directory for workshop example:  

```
cd ./workshops/workshops/otel/ecs-fargate
```  

### Step 3: Prep Requirements and deploy script
  
Study Coralogix [OpenTelemetry ECS Fargate Example](https://github.com/coralogix/telemetry-shippers/tree/master/otel-ecs-fargate)  
Update the `deploy-cf.sh` script with your API key and Coralogix Region  

### Step 4: Deploy Cloudformation to set up Fargate components 

```
source deploy-cf.sh
```

### Step 5: Launch Task

Launch according to your standard procedure.  
Ensure the security group can allow for port 5000 requests.  

The task creates the following containers:  
- Coralogix Python autogenerator  
- OpenTelemetry Collector  

The microservice container create a Python HTTP client and server that will generate spans and logs and send to Coralogix.  

### Step 6 - Study results in Coralogix portal  