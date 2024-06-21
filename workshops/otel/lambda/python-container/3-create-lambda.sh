aws lambda create-function \
  --function-name slerner-python-container \
  --package-type Image \
  --code ImageUri=104013952213.dkr.ecr.us-west-2.amazonaws.com/lambda-container-python:latest \
  --role arn:aws:iam::104013952213:role/service-role/slerner-lambda