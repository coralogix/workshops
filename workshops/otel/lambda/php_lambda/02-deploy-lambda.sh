# aws lambda delete-function --function-name slerner-myFunction
aws lambda create-function --function-name slerner-myFunction \
--runtime provided.al2023 \
--handler bootstrap \
--role arn:aws:iam::104013952213:role/service-role/slerner-lambda \
--zip-file fileb://myFunction.zip