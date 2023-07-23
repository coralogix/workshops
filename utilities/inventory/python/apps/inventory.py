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

# Configure the logging module to display the inventory results on the console
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def get_aws_inventory(access_key, secret_key, region):
    # Create a boto3 session to interact with AWS services using the specified credentials and region
    session = boto3.Session(
        aws_access_key_id=access_key,
        aws_secret_access_key=secret_key,
        region_name=region
    )

    # Initialize a dictionary to store the inventory
    inventory = {}

    # Retrieve AWS resource information using the describe functions for relevant services

    # EC2 Instances
    ec2_client = session.client('ec2')
    ec2_response = ec2_client.describe_instances()
    inventory['ec2_instances'] = ec2_response['Reservations']

    # S3 Buckets
    s3_client = session.client('s3')
    s3_response = s3_client.list_buckets()
    inventory['s3_buckets'] = s3_response['Buckets']

    # RDS Instances
    rds_client = session.client('rds')
    rds_response = rds_client.describe_db_instances()
    inventory['rds_instances'] = rds_response['DBInstances']

    # Lambda Functions
    lambda_client = session.client('lambda')
    lambda_response = lambda_client.list_functions()
    inventory['lambda_functions'] = lambda_response['Functions']

    # VPCs
    ec2_resource = session.resource('ec2')
    vpcs = list(ec2_resource.vpcs.all())
    inventory['vpcs'] = [{'VpcId': vpc.id, 'CidrBlock': vpc.cidr_block} for vpc in vpcs]

    # ECR Repositories
    ecr_client = session.client('ecr')
    ecr_response = ecr_client.describe_repositories()
    inventory['ecr_repositories'] = ecr_response['repositories']

    # ECS Clusters
    ecs_client = session.client('ecs')
    ecs_response = ecs_client.list_clusters()
    inventory['ecs_clusters'] = ecs_response['clusterArns']

    # EKS Clusters
    eks_client = session.client('eks')
    eks_response = eks_client.list_clusters()
    inventory['eks_clusters'] = eks_response['clusters']

    # Add other AWS resources here using the describe methods of their respective services

    return inventory

def write_inventory_to_console(inventory):
    # Convert the inventory dictionary to JSON format using the custom JSON encoder
    inventory_json = json.dumps(inventory, indent=2, cls=CustomJsonEncoder)

    # Write the JSON result to the console using the logging module
    logging.info(f"Inventory Results:\n{inventory_json}")

if __name__ == "__main__":
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

                # Remove "Environment" data from the inventory (if present)
                for resource_type in inventory:
                    resources = inventory[resource_type]
                    for resource in resources:
                        resource.pop('Environment', None)
                
                # Write the inventory to the console
                write_inventory_to_console(inventory)

            # Wait for the specified interval before running again
            time.sleep(inventory_interval)
        except KeyboardInterrupt:
            # Stop the script when Ctrl+C is pressed
            break
        except Exception as e:
            # Catch any exceptions and log the error
            logging.error(f"Error occurred: {str(e)}")