#!/bin/bash

# Set the Lambda function name and AWS region
FUNCTION_NAME="slerner-python"
AWS_REGION="us-west-2"

# Create the test payload
TEST_PAYLOAD=$(cat <<EOF
{
  "httpMethod": "GET",
  "path": "/",
  "headers": {
    "X-Request-ID": "test-id"
  },
  "queryStringParameters": {},
  "body": ""
}
EOF
)

# Base64 encode the payload
ENCODED_PAYLOAD=$(echo "$TEST_PAYLOAD" | base64)

# Function to invoke Lambda function
invoke_lambda() {
    echo "Invoking Lambda function: $FUNCTION_NAME"
    aws lambda invoke \
        --function-name "$FUNCTION_NAME" \
        --region "$AWS_REGION" \
        --payload "$ENCODED_PAYLOAD" \
        response.json

    # Check if the invocation was successful
    if [ $? -eq 0 ]; then
        echo "Lambda function invoked successfully. Response:"
        cat response.json
        rm response.json
    else
        echo "Failed to invoke Lambda function."
    fi
}

# Run the test 10 times
for i in {1..10}
do
    echo "Test #$i"
    invoke_lambda
    echo "---------------------------------"
done

echo "All tests completed."
