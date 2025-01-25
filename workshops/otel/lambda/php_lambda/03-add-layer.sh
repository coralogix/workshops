aws lambda update-function-configuration \
    --function-name slerner-myFunction \
    --layers arn:aws:lambda:us-west-2:625240141681:layer:coralogix-aws-lambda-telemetry-exporter-x86_64:34
aws lambda get-function-configuration --function-name slerner-myFunction --query "Layers"