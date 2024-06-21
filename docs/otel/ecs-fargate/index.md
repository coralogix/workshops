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

### Step 3: Prep Requirements  
  
1 - Put the following file in an S3 bucket:  
https://github.com/coralogix/telemetry-shippers/blob/master/logs/fluent-bit/ecs-fargate/base_filters.conf  
  
2 - Put the following in a Systems Manager Parameter Store:  
https://github.com/coralogix/telemetry-shippers/blob/master/otel-ecs-fargate/config.yaml  

3 - Create a role called `ecsTaskExecutionRole`  
that contains the AWS managed `AmazonECSTaskExecutionRolePolicy`  
and two example policies from this workshop:  
`ecs-policy-s3-access.json`  
`ecs-policy-secrets-access.json`  

These two policies should be tuned for your own production security needs when going into production.  

### Step 4: Prep / Register Task   

Prep example Task: `aws-fargate-otel-demo.json` 

Update all 14 locations replacing contents bewteen `< >` and removing those brackets but leaving the quotes: adding task execution role, key, domain, S3 ARN, Systems Parameter etc...  

Once prepped the task can be registered and made into a service.  

### Step 5: Launch Task

Launch according to your standard procedure.  

The task creates the following containers:  
- two microservices (Client/Server)  
- FireLens log router  
- Otel collector  

The microservice containers create a Python HTTP client and server that will generate spans and logs and send to Coralogix.  


### Step 6 - Study results in Coralogix portal

The shell scripts show how a the microservices example.