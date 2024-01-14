aws cloudformation create-stack \
    --stack-name cx-node-autogen-stack \
    --template-body file://cx-node-autogen.yaml \
    --parameters ParameterKey=ClusterName,ParameterValue=sl-ecs \
    --region us-east-2 \
    # --debug
