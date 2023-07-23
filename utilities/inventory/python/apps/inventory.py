import os
import boto3
import logging
import json
import time
from datetime import datetime

# Custom JSON encoder to handle datetime objects
class CustomJsonEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, datetime):
            return obj.isoformat()
        return json.JSONEncoder.default(self, obj)

# Function to log resources individually
def log_resource(resource_type, resource):
    # Convert the resource to JSON format using the custom JSON encoder
    resource_json = json.dumps(resource, indent=2, cls=CustomJsonEncoder)

    # Create a dictionary for the log message
    log_message = {
        'timestamp': datetime.now(),
        'log_level': 'INFO',
        'body': {
            'resource_type': resource_type,
            'resource': resource,
        },
    }
    # Convert the log message dictionary to a JSON string
    log_json = json.dumps(log_message, indent=2, cls=CustomJsonEncoder)

    # Log the JSON message as an info message
    logging.info(log_json)

def get_ec2_instances(session):
    ec2_client = session.client('ec2')
    ec2_response = ec2_client.describe_instances()
    return ec2_response['Reservations']

def get_s3_buckets(session):
    s3_client = session.client('s3')
    s3_response = s3_client.list_buckets()
    return s3_response['Buckets']

def get_rds_instances(session):
    rds_client = session.client('rds')
    rds_response = rds_client.describe_db_instances()
    return rds_response['DBInstances']

def get_lambda_functions(session):
    lambda_client = session.client('lambda')
    lambda_response = lambda_client.list_functions()
    return lambda_response['Functions']

def get_vpcs(session):
    ec2_resource = session.resource('ec2')
    vpcs = list(ec2_resource.vpcs.all())
    return [{'VpcId': vpc.id, 'CidrBlock': vpc.cidr_block} for vpc in vpcs]

def get_ecr_repositories(session):
    ecr_client = session.client('ecr')
    ecr_response = ecr_client.describe_repositories()
    return ecr_response['repositories']

def get_ecs_clusters(session):
    ecs_client = session.client('ecs')
    ecs_response = ecs_client.list_clusters()
    return ecs_response['clusterArns']

def get_eks_clusters(session):
    eks_client = session.client('eks')
    eks_response = eks_client.list_clusters()
    return eks_response['clusters']

# Dictionary to map AWS service names to their corresponding resource retrieval functions
resource_functions = {
    'ec2_instances': get_ec2_instances,
    's3_buckets': get_s3_buckets,
    'rds_instances': get_rds_instances,
    'lambda_functions': get_lambda_functions,
    'vpcs': get_vpcs,
    'ecr_repositories': get_ecr_repositories,
    'ecs_clusters': get_ecs_clusters,
    'eks_clusters': get_eks_clusters,
}

def get_aws_inventory(access_key, secret_key, region):
    # Create a boto3 session to interact with AWS services using the specified credentials and region
    session = boto3.Session(
        aws_access_key_id=access_key,
        aws_secret_access_key=secret_key,
        region_name=region
    )

    # Retrieve AWS resource information using the resource retrieval functions for relevant services
    inventory = {}
    for resource_type, resource_function in resource_functions.items():
        inventory[resource_type] = resource_function(session)

    return inventory

if __name__ == "__main__":
    # Configure the logging module to display the inventory results as JSON format
    logging.basicConfig(level=logging.INFO, format='%(message)s')

    # Retrieve the environment variable containing AWS configurations (keys and regions)
    aws_configurations = os.environ.get('AWS_CONFIGURATIONS', '').split(',')

    if not aws_configurations:
        logging.warning("No AWS configurations found in the environment variable 'AWS_CONFIGURATIONS'")

    # Retrieve the environment variable specifying the interval in seconds (as a string)
    inventory_interval_str = os.environ.get('INVENTORY_INTERVAL', '10')  # Default to 10 seconds

    try:
        # Convert the inventory interval string to an integer
        inventory_interval = int(inventory_interval_str)
    except ValueError:
        # Handle the case when the environment variable value is not a valid integer
        logging.error("Invalid value provided for INVENTORY_INTERVAL. Defaulting to 10 seconds.")
        inventory_interval = 10  # Default to 10 seconds

    while True:
        try:
            for configuration in aws_configurations:
                access_key, secret_key, region = configuration.split(':')

                # Get the AWS inventory for the current configuration
                inventory = get_aws_inventory(access_key.strip(), secret_key.strip(), region.strip())

                # Remove "Environment" data from the inventory (if present) and log each resource individually
                for resource_type, resources in inventory.items():
                    for resource in resources:
                        resource.pop('Environment', None)
                        log_resource(resource_type, resource)

            # Wait for the specified interval before running again
            time.sleep(inventory_interval)
        except KeyboardInterrupt:
            # Stop the script when Ctrl+C is pressed
            break