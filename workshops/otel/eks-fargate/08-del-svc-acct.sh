eksctl delete iamserviceaccount \
  --cluster=slerner-eks-fargate \
  --region=us-west-2 \
  --name=cx-otel-collector \
  --namespace=default

eksctl delete iamserviceaccount \
  --cluster=slerner-eks-fargate \
  --region=us-west-2 \
  --name=cx-otel-collector \
  --namespace=cx-eks-fargate-otel