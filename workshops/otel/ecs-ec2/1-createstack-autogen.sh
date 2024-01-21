aws cloudformation create-stack \
    --stack-name cx-node-autogen-stack \
    --template-body file://node-autogen.yaml \
    --parameters ParameterKey=ClusterName,ParameterValue=sym-ecs \
    --region us-east-2