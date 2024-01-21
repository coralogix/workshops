#!/bin/bash

#change all variables as needed

aws cloudformation deploy --template-file template-updated-parallel.yaml --stack-name cx-otel \
    --region us-east-2 \
    --parameter-overrides \
        DefaultApplicationName=cx-ecs-node \
        SubsystemName=cx-ecs-node \
        ClusterName=sym-ecs \
        CDOTImageVersion=v0.2.7 \
        CORALOGIX_DOMAIN=coralogix.us \
        PrivateKey=cxtp_hqeVmHgwCTSlC4y1sntQpLrILl0MbV \
        CoralogixRegion=US