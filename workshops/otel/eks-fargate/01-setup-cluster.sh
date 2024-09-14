eksctl create cluster \
  --name slerner-eksfargate \
  --region us-west-2 \
  --vpc-private-subnets subnet-00774971f1ec66408,subnet-06bcb06f01f102470,subnet-0fa652d167425a457 \
  --fargate