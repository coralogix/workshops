import logging
import json
import requests
import boto3

# Custom JSON formatter for logging
class JsonFormatter(logging.Formatter):
    def format(self, record):
        # Construct a JSON log record using standard and custom fields
        log_record = {
            "time": self.formatTime(record, self.datefmt),
            "name": record.name,
            "level": record.levelname,
            "message": record.getMessage(),
            "status_code": getattr(record, "statusCode", "N/A"),
            "url": getattr(record, "url", "N/A"),
        }
        if record.exc_info:
            log_record["exception"] = self.formatException(record.exc_info)
        return json.dumps(log_record)

# Set up logging with JSON format
def setup_logging():
    # Get the root logger
    logger = logging.getLogger()
    # Set logging level to DEBUG
    logger.setLevel(logging.DEBUG)
    
    # Create a stream handler for outputting logs
    ch = logging.StreamHandler()
    ch.setLevel(logging.DEBUG)
    
    # Use the custom JSON formatter
    formatter = JsonFormatter()
    ch.setFormatter(formatter)
    
    # Add the handler to the logger
    logger.addHandler(ch)

# Function to make a request to api.github.com
def make_github_request():
    response = requests.get("https://api.github.com")
    return response

# Function to list S3 buckets
def list_s3_buckets():
    s3 = boto3.client('s3')
    response = s3.list_buckets()
    return [bucket['Name'] for bucket in response['Buckets']]

# Lambda handler function
def lambda_handler(event, context):
    setup_logging()
    logger = logging.getLogger(__name__)
    
    responses = []
    s3_bucket_lists = []
    try:
        for _ in range(10):
            response = make_github_request()
            responses.append({"statusCode": response.status_code, "url": response.url})
            logger.info(
                "Successfully made a request to GitHub API",
                extra={"statusCode": response.status_code, "url": response.url}
            )
        
        # List S3 buckets 5 times
        for _ in range(5):
            buckets = list_s3_buckets()
            s3_bucket_lists.append(buckets)
            logger.info("List of S3 buckets", extra={"buckets": buckets})
            print(json.dumps(buckets, indent=2))

        return {
            'statusCode': 200,
            'body': json.dumps({
                "requests": responses,
                "buckets": s3_bucket_lists
            })
        }
    except Exception as e:
        logger.error("Error making requests to GitHub API or listing S3 buckets", exc_info=True)
        return {
            'statusCode': 500,
            'body': json.dumps({'error': str(e)})
        }

if __name__ == '__main__':
    # Configure logging
    setup_logging()
    logger = logging.getLogger(__name__)

    responses = []
    s3_bucket_lists = []
    try:
        for _ in range(10):
            response = make_github_request()
            responses.append({"statusCode": response.status_code, "url": response.url})
            logger.info(
                "Successfully made a request to GitHub API",
                extra={"statusCode": response.status_code, "url": response.url}
            )
            print(json.dumps({"statusCode": response.status_code, "url": response.url}, indent=2))
        
        # List S3 buckets 5 times
        for _ in range(5):
            buckets = list_s3_buckets()
            s3_bucket_lists.append(buckets)
            logger.info("List of S3 buckets", extra={"buckets": buckets})
            print(json.dumps(buckets, indent=2))
    except Exception as e:
        logger.error("Error making requests to GitHub API or listing S3 buckets", exc_info=True)
        print({'error': str(e)})
