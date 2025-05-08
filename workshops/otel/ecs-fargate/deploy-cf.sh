aws cloudformation deploy --template-file fargate-new.yaml \
    --stack-name slerner-fargate-test2 \
    --region us-west-2 \
    --capabilities CAPABILITY_NAMED_IAM \
    --parameter-overrides \
        PrivateKey="YOURKEYHERE" \
        CoralogixRegion="US2"
