DRAFT WORK IN PROGRESS

## Step 1: Prep Requirements  
Requirements:
The following file in an S3 bucket:  
https://github.com/coralogix/telemetry-shippers/blob/master/logs/fluent-bit/ecs-fargate/base_filters.conf  
  
The following in a Systems Manager Parameter Store:  
https://github.com/coralogix/telemetry-shippers/blob/master/otel-ecs-fargate/config.yaml  

Creation of a role called `ecsTaskExecutionRole`  
that contains the AWS managed `AmazonECSTaskExecutionRolePolicy`  
and two example policies from this workshop:  
`ecs-policy-s3-access.json`  
`ecs-policy-secrets-access.json`  

These two policies should be tuned for your own production security needs when going into production.  

## Step 2: Prep Task   

Prep example Task: `aws-fargate-otel-demo.json:  

Update all 14 locations replacing contents bewteen `< >` and removing those brackets but leaving the quotes: adding task execution role, key, domain, S3 ARN, Systems Parameter etc...  

Once prepped the task can be registered and made into a service.  

 The task has two microservice containers, the FireLens log container, and the Otel collector.   
 It creates Python HTTP client and server that will generate spans and logs and send to Coralogix.  