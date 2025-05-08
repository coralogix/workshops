aws ecs create-service \
  --cluster slerner-ecs-cluster \
  --task-definition slerner-fargate-java:1 \
  --service-name slerner-fargate-java-service \
  --desired-count 1 \
  --launch-type "FARGATE" \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-037378fd5035994fd],securityGroups=[sg-032d218698f31d86f],assignPublicIp=ENABLED}" \
  --enable-execute-command