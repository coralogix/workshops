aws ecs create-service \
  --cluster slerner-cluster \
  --task-definition Coralogix-observability:3 \
  --service-name coralogix-fargate-service \
  --desired-count 1 \
  --launch-type "FARGATE" \
  --network-configuration "awsvpcConfiguration={subnets=[subnet-037378fd5035994fd],securityGroups=[sg-032d218698f31d86f],assignPublicIp=ENABLED}" \
  --enable-execute-command