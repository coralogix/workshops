DRAFT

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

These should be tuned for your own production security needs after trying a demo.  

Prep example Task: `aws-fargate-otel-demo`:  

Update all 14 locations replacing contents bewteen `< >` and removing those bracks: adding task execution role, key, domain, S3 ARN, Systems Parameter etc...  

Once prepped it can be registered and made into a service that creates Python HTTP client and server that will generate spans and logs and send to Coralogix.  