aws lambda invoke \
    --function-name slerner-myFunction \
    --cli-binary-format raw-in-base64-out \
    --payload '{"httpMethod":"GET","path":"/rolldice","queryStringParameters":{},"headers":{"Content-Type":"application/json"}}' \
    /dev/stdout
