aws cloudformation create-stack \
    --stack-name node-autogen-stack \
    --template-body file://node-autogen-otel.yaml \
    --parameters \
        ParameterKey=ClusterName,ParameterValue=YOURCLUSTERNAME \
    --region us-west-2