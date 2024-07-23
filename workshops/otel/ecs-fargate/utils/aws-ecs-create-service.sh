aws ecs create-service --cluster slerner-cluster \
 --task-definition fargate-demo:9 \
 --service-name fargate-demo \
 --desired-count 1 --launch-type "FARGATE" \
 --network-configuration "awsvpcConfiguration={subnets=[subnet-0517e5c4de64acc2b], securityGroups=[sg-0d178e26b0da1164d], assignPublicIp=ENABLED}" \
 --enable-execute-command