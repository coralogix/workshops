#!/bin/bash

aws cloudformation deploy --template-file template.yaml --stack-name cx-otel \
    --region us-west-2 \
    --parameter-overrides \
        DefaultApplicationName=YOURAPPNAME \
        CDOTImageVersion=v0.4.1 \
        ClusterName=YOURCLUSTERNAME \
        CoralogixApiKey=YOURKEY \
        CoralogixRegion=YOURCXREGION \
        Metrics=enable
