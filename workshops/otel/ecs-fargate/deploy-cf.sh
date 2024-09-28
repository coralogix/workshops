aws cloudformation deploy --template-file ecs-fargate-cf-autogen.yaml \
    --stack-name slerner-fargate-test \
    --region us-west-2 \
    --capabilities CAPABILITY_NAMED_IAM \
    --parameter-overrides \
        PrivateKey="" \
        CoralogixRegion="US2"
