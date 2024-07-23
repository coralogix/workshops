import pymysql
import os
import json
import logging
import time
import random
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import ConsoleSpanExporter, SimpleSpanProcessor
from opentelemetry.instrumentation.logging import LoggingInstrumentor

# Set up OpenTelemetry Tracer
trace.set_tracer_provider(TracerProvider())
tracer = trace.get_tracer(__name__)
trace.get_tracer_provider().add_span_processor(
    SimpleSpanProcessor(ConsoleSpanExporter())
)
LoggingInstrumentor().instrument()

# Configure JSON logging
class JSONFormatter(logging.Formatter):
    def format(self, record):
        trace_context = trace.get_current_span().get_span_context()
        log_record = {
            "time": record.created,
            "span_id": '{:016x}'.format(trace_context.span_id),
            "trace_id": '{:032x}'.format(trace_context.trace_id),
            "severity": record.levelname,
            "message": record.getMessage(),
        }
        if hasattr(record, 'sql'):
            log_record['sql_query'] = record.sql
        if hasattr(record, 'sql_result'):
            log_record['sql_result'] = record.sql_result
        return json.dumps(log_record)

# Set up logging
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)
ch = logging.StreamHandler()
ch.setFormatter(JSONFormatter())
logger.addHandler(ch)

# Environment variables for MySQL connection
MYSQL_HOST = os.getenv('MYSQL_HOST', 'cx-mysql-service')
MYSQL_USER = os.getenv('MYSQL_USER', 'exampleuser')
MYSQL_PASSWORD = os.getenv('MYSQL_PASSWORD', 'exampleuserpassword')
MYSQL_DB = os.getenv('MYSQL_DB', 'testdb')

def get_database_connection():
    return pymysql.connect(
        host=MYSQL_HOST,
        user=MYSQL_USER,
        password=MYSQL_PASSWORD,
        db=MYSQL_DB
    )

def perform_query():
    conn = get_database_connection()
    try:
        with conn.cursor() as cursor:
            sql = "SELECT number FROM random_numbers;"
            cursor.execute(sql)
            results = cursor.fetchall()  # Fetch all records
            if results:
                for result in results:
                    with tracer.start_as_current_span(f"Process number {result[0]}"):
                        message = f"Number: {result[0]}"
                        logger.info(message, extra={'sql': sql, 'sql_result': result})
                        time.sleep(random.uniform(0.15, 0.45))  # Sleep between 0.15 and 0.45 seconds
            else:
                with tracer.start_as_current_span("No entries found"):
                    logger.warning("No entries found in the database.", extra={'sql': sql, 'sql_result': 'None'})
    finally:
        conn.close()

# Perform the query
perform_query()