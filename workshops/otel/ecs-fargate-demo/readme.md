# AWS CloudFormation Deployment

This section of the workshop provides an example CloudFormation template that creates a Task Definition with OpenTelemetry instrumentation - Metrics, Logs, Traces.

## Prerequisites

Before using running the template, make sure you have the following prerequisites in place:

1. **AWS CLI**: Ensure that you have the AWS Command Line Interface (CLI) installed and configured with the necessary credentials.

2. **AWS Region**: Specify the AWS region where you want to create or update the CloudFormation stack (`<aws region>` in the command).

3. **AWS IAM Capabilities**: The command requires the `CAPABILITY_NAMED_IAM` capability. Ensure that your AWS IAM permissions allow you to use this capability.

4. **Logging and Tracing Configuration**: For detailed instructions on setting up prereqs for logs, metrics, and traces, please refer to the [Coralogix AWS ECS Fargate Documentation](https://coralogix.com/docs/aws-ecs-fargate/).

   - Logs: You will need to have AWS Fluent Bit configurations in your S3 bucket.

   - Metrics & Traces: Utilize the Amazon Systems Manager Parameter Store for your OpenTelemetry (OTel) Collector configurations as described in the documentation.

5. **Application**: 

    - The .NET version of the template uses a tracegenerator container for .NET from our workshop [here](https://github.com/YonghuiCho/workshops/tree/master/workshops/otel/dotnet6-linux)
    - The Java version of the template uses a Java app from our [microservices workshop](https://github.com/YonghuiCho/workshops/tree/master/workshops/otel/microservices-demo/java).

## Usage

To deploy your CloudFormation stack, use the following command:

```bash
aws cloudformation deploy \
    --template-file ecs-fargate-<dotnet or java>.yaml \
    --stack-name <stack_name> \
    --region <aws region> \
    --capabilities "CAPABILITY_NAMED_IAM" \
    --parameter-overrides \
        PrivateKey=<your-private-key> \
        CoralogixRegion=<coralogix-region> \
        S3ConfigARN=<ARN of S3 log config>
```
## Parameters

Below is a table explaining each parameter used with the `aws cloudformation deploy` command:

| Parameter            | Description                                                                                                  |
|----------------------|--------------------------------------------------------------------------------------------------------------|
| `--template-file`    | The path to the CloudFormation template file (`ecs-fargate-cf.yaml` in this example).                      |
| `--stack-name`       | A unique name for your CloudFormation stack. This name will be used to identify your stack in AWS.         |
| `--region`           | The AWS region where you want to create or update the CloudFormation stack. (*eg.us-east-2*)                                 |
| `--capabilities`     | Set this to `"CAPABILITY_NAMED_IAM"` to acknowledge that the template may create AWS IAM resources.       |
| `--parameter-overrides` | Use this option to provide parameter values for your CloudFormation template.                               |
| `PrivateKey`         | Replace with your Coralogix private key.                                                                     |
| `CoralogixRegion`    | Replace with the Coralogix region you want to use. (*eg.US, Europe*)                                                        |
| `S3ConfigARN`        | Replace with the Amazon Resource Name (ARN) of your FluentBit S3 Config.                                            |

**Note**: Ensure that the parameters you provide match the parameter names defined in your CloudFormation template (`ecs-fargate-cf.yaml`).

## References
1. [Coralogix ECS Fargate integrations](https://github.com/coralogix/cloudformation-coralogix-aws/tree/master/aws-integrations/ecs-fargate)

2. [Coralogix AWS ECS Fargate Documentation](https://coralogix.com/docs/aws-ecs-fargate/)
