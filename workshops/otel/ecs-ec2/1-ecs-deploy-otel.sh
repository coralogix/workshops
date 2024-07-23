#!/bin/bash

aws cloudformation deploy --template-file template.yaml --stack-name cx-otel \
    --region us-east-2 \
    --parameter-overrides \
        DefaultApplicationName=YOURAPPNAMEHERE \
        CDOTImageVersion=vCHECKVERSION \
        ClusterName=YOURCLUSTERNAMEHERE \
        CoralogixApiKey=YOURKEYHERE \
        CoralogixRegion=YOURREGIONHERE \
        Metrics=enable