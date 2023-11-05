aws cloudformation deploy --template-file ecs-fargate-cf.yaml \
    --stack-name slerner-fargate-java \
    --region US \
    --capabilities "CAPABILITY_NAMED_IAM" \
    --parameter-overrides \
        PrivateKey=7d6c036d-1bbb-7430-7827-cccfc339dcce \
        CoralogixRegion=coralogix.us \
        S3ConfigARN=s3://steve-lerner-logs/base_filters.conf