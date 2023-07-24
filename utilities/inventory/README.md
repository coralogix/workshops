## AWS Inventory Logger Container

Inventory container executes a Python app using AWS BOTO3 that runs a loop of describing a set of inventory on AWS accounts. 
It requires the following env variables- and can handle as many accounts as you'd like.  

```
export AWS_CONFIGURATIONS="YOUR_ACCESS_KEY_1:YOUR_SECRET_KEY_1:us-east-1,YOUR_ACCESS_KEY_2:YOUR_SECRET_KEY_2:eu-west-1"  
export INVENTORY_INTERVAL=30
``````

or translated properly to YAML for a k8s deployment- you can see an example here [./yaml](./yaml)

It will describe as many AWS accounts as one adds.  
It will not retrieve enviroment variables to prevent exposure of tokens etc...

The following services are included in the inventory- more can be added upon request:
- `EC2`
- `S3`
- `RDS`
- `Lambda`
- `VPCs`
- `ECR`
- `ECS`
- `EKS`