import random
import string
import psycopg2
from pymongo import MongoClient
import redis
import json
import datetime
import time

def log_message(severity, db, query, result=None, error=None):
    """Logs a structured JSON message to stdout."""
    log_entry = {
        "timestamp": datetime.datetime.utcnow().isoformat() + "Z",
        "severity": severity,
        "database": db,
        "query": query,
        "result": result,
        "error": str(error) if error else None
    }
    print(json.dumps(log_entry))

def generate_tracking_number():
    """Generate a random UPS tracking number in the format 1Z999AA10123456784."""
    shipper_code = ''.join(random.choices(string.ascii_uppercase + string.digits, k=6))
    service_code = ''.join(random.choices(string.ascii_uppercase + string.digits, k=2))
    package_identifier = ''.join(random.choices(string.digits, k=8))
    return f"1Z{shipper_code}{service_code}{package_identifier}"

def write_to_postgres():
    """Write and delete a random entry in PostgreSQL."""
    tracking_number = generate_tracking_number()
    try:
        conn = psycopg2.connect(
            dbname="postgres",
            user="postgres",
            password="postgres",
            host="coraexp-postgres",
            port=5432
        )
        cur = conn.cursor()
        
        # Create table if it doesn't exist
        create_query = "CREATE TABLE IF NOT EXISTS test_data (id SERIAL PRIMARY KEY, data TEXT);"
        cur.execute(create_query)
        conn.commit()
        log_message("INFO", "PostgreSQL", create_query, "Table check/create successful.")
        
        # Insert the random data
        insert_query = "INSERT INTO test_data (data) VALUES (%s) RETURNING id;"
        cur.execute(insert_query, (tracking_number,))
        entry_id = cur.fetchone()[0]
        conn.commit()
        log_message("INFO", "PostgreSQL", insert_query, f"Inserted tracking number {tracking_number} with ID {entry_id}.")
        
        # Delete the entry
        delete_query = "DELETE FROM test_data WHERE id = %s;"
        cur.execute(delete_query, (entry_id,))
        conn.commit()
        log_message("INFO", "PostgreSQL", delete_query, f"Deleted tracking number {tracking_number} with ID {entry_id}.")
        
        cur.close()
        conn.close()
    except Exception as e:
        log_message("ERROR", "PostgreSQL", "N/A", error=e)

def write_to_mongo():
    """Write and delete a random entry in MongoDB."""
    tracking_number = generate_tracking_number()
    try:
        client = MongoClient("mongodb://coraexp-mongo:27017/")
        db = client.test_database
        collection = db.test_collection
        
        # Insert the random data
        result = collection.insert_one({"data": tracking_number})
        log_message("INFO", "MongoDB", "insert_one", f"Inserted tracking number {tracking_number} with ID {result.inserted_id}.")
        
        # Delete the entry
        collection.delete_one({"_id": result.inserted_id})
        log_message("INFO", "MongoDB", "delete_one", f"Deleted tracking number {tracking_number} with ID {result.inserted_id}.")
        
        client.close()
    except Exception as e:
        log_message("ERROR", "MongoDB", "N/A", error=e)

def write_to_redis():
    """Write and delete a random entry in Redis."""
    tracking_number = generate_tracking_number()
    try:
        r = redis.Redis(host="coraexp-redis", port=6379)
        
        # Set the random data with a key
        set_query = "SET test_key"
        r.set("test_key", tracking_number)
        log_message("INFO", "Redis", set_query, f"Inserted tracking number {tracking_number} with key 'test_key'.")
        
        # Delete the entry
        delete_query = "DEL test_key"
        r.delete("test_key")
        log_message("INFO", "Redis", delete_query, f"Deleted tracking number {tracking_number} with key 'test_key'.")
    except Exception as e:
        log_message("ERROR", "Redis", "N/A", error=e)

if __name__ == "__main__":
    while True:
        write_to_postgres()
        write_to_mongo()
        write_to_redis()
        time.sleep(2)