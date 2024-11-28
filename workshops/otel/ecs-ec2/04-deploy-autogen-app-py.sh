aws cloudformation create-stack \
    --stack-name py-autogen-stack \
    --template-body file://py-autogen-otel.yaml \
    --parameters \
        ParameterKey=ClusterName,ParameterValue=YOURCLUSTERNAME \
    --region us-west-2