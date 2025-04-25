import sys
import time
import random
import signal
import pyodbc
import datetime
import argparse
from typing import List, Optional, Dict, Any
from dataclasses import dataclass
from config import TestConfiguration

@dataclass
class LogEntry:
    timestamp: datetime.datetime
    category: str
    operation: str
    severity: str
    duration: float
    result: Optional[str] = None

class SQLServerTester:
    def __init__(self):
        self.logs: List[LogEntry] = []
        self.running = True
        signal.signal(signal.SIGINT, self.handle_interrupt)

    def handle_interrupt(self, signum, frame):
        print("\nGracefully shutting down...")
        self.running = False

    def log_action(self, severity: str, category: str, operation: str, duration: float = 0, result: Optional[str] = None):
        log = LogEntry(
            timestamp=datetime.datetime.utcnow(),
            category=category,
            operation=operation,
            severity=severity,
            duration=duration,
            result=result
        )
        self.logs.append(log)

        if severity == "ERROR":
            print(f"  {operation} - Failed: {result}")
        elif duration > 0:
            print(f"  {operation} - Completed in {duration/1000:.2f}s")

    def wait_for_sql_server(self, conn_str: str, max_retries: int = 10, retry_delay: int = 5):
        print("\nAttempting to connect to SQL Server...")
        
        for attempt in range(1, max_retries + 1):
            try:
                print(f"Connection attempt {attempt}/{max_retries}...")
                conn = pyodbc.connect(conn_str, timeout=TestConfiguration.CONNECTION_TIMEOUT)
                cursor = conn.cursor()
                version = cursor.execute("SELECT @@VERSION").fetchone()[0]
                print(f"\nConnection successful!")
                print(f"Server Version: {version}")
                cursor.close()
                conn.close()
                return True
            except pyodbc.Error as e:
                print(f"\nAttempt {attempt} failed with SQL error:")
                print(f"Error: {str(e)}")
                
                if attempt == max_retries:
                    self.log_action("ERROR", "Connection", 
                                  f"Failed to connect after {max_retries} attempts",
                                  result=str(e))
                    raise
                
                print(f"Waiting {retry_delay} seconds before next attempt... ({max_retries - attempt} attempts remaining)")
                time.sleep(retry_delay)

    def setup_database(self, conn):
        start_time = time.time()
        
        try:
            # Drop and recreate database - must be done outside transaction
            print("Creating TestDB database...")
            
            # Create a new connection for database operations to ensure no active transaction
            admin_conn = pyodbc.connect(TestConfiguration.get_connection_string(), 
                                      timeout=TestConfiguration.CONNECTION_TIMEOUT,
                                      autocommit=True)  # Important: autocommit mode
            admin_cursor = admin_conn.cursor()
            
            try:
                # First check if database exists
                admin_cursor.execute("SELECT database_id FROM sys.databases WHERE Name = 'TestDB'")
                if admin_cursor.fetchone():
                    print("Database exists, dropping it...")
                    # Force close existing connections
                    admin_cursor.execute("""
                        ALTER DATABASE TestDB SET SINGLE_USER 
                        WITH ROLLBACK IMMEDIATE
                    """)
                    admin_cursor.execute("DROP DATABASE TestDB")
                
                print("Creating new database...")
                admin_cursor.execute("CREATE DATABASE TestDB")
                
            finally:
                admin_cursor.close()
                admin_conn.close()
            
            # Switch to TestDB with a new connection
            print("Connecting to TestDB...")
            conn.close()  # Close the old connection
            db_conn = pyodbc.connect(
                TestConfiguration.get_connection_string(),
                timeout=TestConfiguration.CONNECTION_TIMEOUT
            )
            db_cursor = db_conn.cursor()
            db_cursor.execute("USE TestDB")
            
            # Create tables
            print("\nCreating tables...")
            for table_name in TestConfiguration.get_all_table_names():
                print(f"Creating table {table_name}...")
                try:
                    # Create table and indexes in separate statements
                    db_cursor.execute(f"""
                        CREATE TABLE {table_name} (
                            ID BIGINT PRIMARY KEY IDENTITY(1,1),
                            Number INT,
                            Description NVARCHAR(100),
                            Category NVARCHAR(50),
                            Status NVARCHAR(20),
                            CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
                        )
                    """)
                    db_conn.commit()
                    
                    print(f"Creating indexes for {table_name}...")
                    db_cursor.execute(f"CREATE INDEX IX_{table_name}_Number ON {table_name}(Number)")
                    db_cursor.execute(f"CREATE INDEX IX_{table_name}_Category ON {table_name}(Category)")
                    db_cursor.execute(f"CREATE INDEX IX_{table_name}_Status ON {table_name}(Status)")
                    db_conn.commit()
                    
                    self.log_action("INFO", "Database", f"Created schema for {table_name}")
                except Exception as e:
                    print(f"Error creating table {table_name}: {str(e)}")
                    raise

            # Insert test data
            records_per_table = TestConfiguration.TOTAL_RECORDS // TestConfiguration.NUMBER_OF_TABLES
            print(f"\nInserting {records_per_table} records per table...")
            
            for table_name in TestConfiguration.get_all_table_names():
                print(f"\nPopulating table {table_name}...")
                
                # Prepare batch insert
                batch = []
                batch_size = min(1000, TestConfiguration.MAX_SQL_PARAMETERS // 4)  # 4 parameters per record
                insert_sql = f"""
                    INSERT INTO {table_name} (Number, Description, Category, Status)
                    VALUES (?, ?, ?, ?)
                """
                
                for i in range(records_per_table):
                    number = random.randint(1, 1000000)
                    description = f"Test record {i} in {table_name}"
                    category = random.choice(TestConfiguration.CATEGORIES)
                    status = random.choice(TestConfiguration.STATUS_CODES)
                    
                    batch.append((number, description, category, status))
                    
                    if len(batch) >= batch_size or i == records_per_table - 1:
                        try:
                            db_cursor.fast_executemany = True
                            db_cursor.executemany(insert_sql, batch)
                            db_conn.commit()
                            self.log_action("INFO", "Database", 
                                          f"Inserted {len(batch)} records into {table_name}")
                            batch = []
                        except Exception as e:
                            print(f"Error inserting batch into {table_name}: {str(e)}")
                            raise
                
                print(f"Completed populating {table_name}")
            
            duration = (time.time() - start_time) * 1000
            self.log_action("INFO", "Database", "Database setup complete", duration)
            return db_conn
            
        except Exception as e:
            duration = (time.time() - start_time) * 1000
            self.log_action("ERROR", "Database", "Database setup failed", duration, str(e))
            raise

    def verify_database_setup(self, conn):
        """Verify that all tables exist and have data"""
        cursor = conn.cursor()
        try:
            cursor.execute("USE TestDB")
            
            for table_name in TestConfiguration.get_all_table_names():
                # Check if table exists
                result = cursor.execute(f"""
                    SELECT OBJECT_ID('{table_name}', 'U') as TableID,
                           (SELECT COUNT(*) FROM {table_name}) as RecordCount
                """).fetchone()
                
                if not result or not result.TableID:
                    raise Exception(f"Table {table_name} does not exist")
                if result.RecordCount == 0:
                    raise Exception(f"Table {table_name} has no records")
                
            return True
        except Exception as e:
            self.log_action("ERROR", "Verification", "Database verification failed", result=str(e))
            return False

    def run_test_iteration(self, conn, iteration: int):
        cursor = conn.cursor()
        print(f"\n=== Starting Iteration {iteration} ===")
        
        # Define test scenarios
        test_scenarios = [
            ("Fast queries", self.run_fast_queries),
            ("Slow queries", self.run_slow_queries),
            ("Parallel queries", self.run_parallel_queries),
            ("Temp table queries", self.run_temp_table_queries),
            ("Failed queries", self.run_failed_queries)
        ]
        
        for name, func in test_scenarios:
            try:
                print(f"\nRunning {name}...")
                func(conn)
                time.sleep(0.5)  # Small delay between scenarios
            except Exception as e:
                print(f"Error in {name}: {str(e)}")
                self.log_action("ERROR", name, f"Failed in {name}", result=str(e))

        print(f"\nCompleted Iteration {iteration}")

    def run_fast_queries(self, conn):
        cursor = conn.cursor()
        time.sleep(random.uniform(0.1, 0.3))  # Random delay
        
        # Test 1: Quick indexed lookups
        range_min, range_max = random.choice(TestConfiguration.NUMBER_RANGES)
        table = random.choice(TestConfiguration.get_all_table_names())
        category = random.choice(TestConfiguration.CATEGORIES)
        
        cursor.execute(f"""
            SELECT TOP {TestConfiguration.QUERY_LIMIT} * 
            FROM {table} 
            WHERE Number BETWEEN ? AND ? AND Category = ?
        """, (range_min, range_max, category))
        self.log_action("INFO", "Query", f"Indexed range lookup on {table}")

    def run_slow_queries(self, conn):
        cursor = conn.cursor()
        time.sleep(random.uniform(0.3, 0.5))  # Medium delay
        
        # Complex aggregation
        table = random.choice(TestConfiguration.get_all_table_names())
        cursor.execute(f"""
            WITH DataStats AS (
                SELECT TOP {TestConfiguration.QUERY_LIMIT} 
                    Category, Status, 
                    COUNT(*) as RecordCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    STRING_AGG(CAST(ID as VARCHAR(20)), ',') as RecordIDs
                FROM {table}
                GROUP BY Category, Status
            )
            SELECT 
                Category, Status, RecordCount, AvgNumber,
                COUNT(*) OVER (PARTITION BY Category) as CategoryTotal
            FROM DataStats
            WHERE RecordCount > 5
            ORDER BY CategoryTotal DESC, AvgNumber DESC
        """)
        self.log_action("INFO", "Query", "Complex aggregation")

    def run_parallel_queries(self, conn):
        cursor = conn.cursor()
        time.sleep(random.uniform(0.5, 1.5))  # Longer delay
        
        table = random.choice(TestConfiguration.get_all_table_names())
        cursor.execute(f"""
            SELECT 
                Category,
                COUNT(*) as TotalCount,
                AVG(CAST(Number as FLOAT)) as AvgNumber,
                MIN(Number) as MinNumber,
                MAX(Number) as MaxNumber
            FROM {table}
            WHERE Number BETWEEN 1000 AND 900000
            GROUP BY Category
            OPTION (MAXDOP 4)
        """)
        self.log_action("INFO", "Query", "Parallel aggregation")

    def run_temp_table_queries(self, conn):
        cursor = conn.cursor()
        
        # Global temporary table test
        table = random.choice(TestConfiguration.get_all_table_names())
        cursor.execute("""
            CREATE TABLE ##GlobalTempNumbers (
                ID INT IDENTITY(1,1) PRIMARY KEY,
                Number INT,
                Category NVARCHAR(50),
                ProcessedAt DATETIME2 DEFAULT GETUTCDATE()
            )
        """)
        
        cursor.execute(f"""
            INSERT INTO ##GlobalTempNumbers (Number, Category)
            SELECT TOP 100 Number, Category 
            FROM {table}
            WHERE Category = 'Large'
        """)
        
        cursor.execute("""
            SELECT 
                DATEPART(SECOND, ProcessedAt) as ProcessedSecond,
                COUNT(*) as NumberCount,
                AVG(CAST(Number as FLOAT)) as AvgNumber
            FROM ##GlobalTempNumbers
            GROUP BY DATEPART(SECOND, ProcessedAt)
        """)
        
        cursor.execute("DROP TABLE ##GlobalTempNumbers")
        self.log_action("INFO", "Query", "Temp table operations")

    def run_failed_queries(self, conn):
        cursor = conn.cursor()
        
        # Test various error scenarios
        error_types = ["OVERFLOW", "CONVERSION", "INVALID_OBJECT", "SYNTAX"]
        error_type = random.choice(error_types)
        
        try:
            if error_type == "OVERFLOW":
                cursor.execute("SELECT CAST(2147483647 + 1 as INT)")
            elif error_type == "CONVERSION":
                cursor.execute("SELECT CAST('invalid' as INT)")
            elif error_type == "INVALID_OBJECT":
                cursor.execute("SELECT * FROM NonExistentTable")
            else:  # SYNTAX
                cursor.execute("SELEC * FROM")
        except Exception as e:
            self.log_action("EXPECTED_ERROR", "Query", f"Expected {error_type} error test", result=str(e))

    def cleanup_database(self, conn):
        try:
            # First drop all tables using the normal connection
            cursor = conn.cursor()
            cursor.execute("USE TestDB")
            
            for table_name in TestConfiguration.get_all_table_names():
                try:
                    print(f"Dropping table {table_name}...")
                    cursor.execute(f"DROP TABLE IF EXISTS {table_name}")
                    conn.commit()
                    self.log_action("INFO", "Cleanup", f"Dropped table {table_name}")
                except Exception as e:
                    self.log_action("WARN", "Cleanup", f"Failed to drop table {table_name}", result=str(e))
            
            # Close the normal connection
            cursor.close()
            conn.close()
            
            # Create a new connection in autocommit mode for database operations
            print("\nDropping TestDB database...")
            admin_conn = pyodbc.connect(TestConfiguration.get_connection_string(), 
                                      timeout=TestConfiguration.CONNECTION_TIMEOUT,
                                      autocommit=True)
            try:
                admin_cursor = admin_conn.cursor()
                
                # Switch to master before dropping TestDB
                admin_cursor.execute("USE master")
                
                # Check if TestDB exists
                admin_cursor.execute("SELECT database_id FROM sys.databases WHERE Name = 'TestDB'")
                if admin_cursor.fetchone():
                    # Force close existing connections and drop database
                    admin_cursor.execute("ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE")
                    admin_cursor.execute("DROP DATABASE TestDB")
                    print("TestDB database dropped successfully")
                else:
                    print("TestDB database does not exist")
                
                self.log_action("INFO", "Cleanup", "Dropped database")
            finally:
                admin_conn.close()
                
        except Exception as e:
            self.log_action("ERROR", "Cleanup", "Failed to cleanup database", result=str(e))
            raise

    def display_summary(self, total_duration: float):
        print("\n=== Test Execution Summary ===")
        print(f"Total Duration: {total_duration/1000:.2f} seconds")
        
        categories = {}
        for log in self.logs:
            if log.category not in categories:
                categories[log.category] = {"success": 0, "error": 0, "duration": 0}
            
            stats = categories[log.category]
            stats["duration"] += log.duration
            if log.severity == "ERROR":
                stats["error"] += 1
            elif log.severity == "INFO":
                stats["success"] += 1
        
        for category, stats in categories.items():
            print(f"\n{category}:")
            print(f"  Duration: {stats['duration']/1000:.2f} seconds")
            print(f"  Successful Operations: {stats['success']}")
            if stats["error"] > 0:
                print(f"  Failed Operations: {stats['error']}")
                errors = [log for log in self.logs if log.category == category and log.severity == "ERROR"]
                for error in errors:
                    print(f"    - {error.operation}: {error.result}")
        
        total_errors = sum(stats["error"] for stats in categories.values())
        total_success = sum(stats["success"] for stats in categories.values())
        print(f"\nTotal Successful Operations: {total_success}")
        print(f"Total Failed Operations: {total_errors}")

def main():
    parser = argparse.ArgumentParser(description='SQL Server Performance Testing Tool')
    parser.add_argument('duration', nargs='?', type=int, help='Duration to run in minutes (optional)')
    args = parser.parse_args()

    if args.duration:
        print(f"Will run for {args.duration} minutes")
        end_time = datetime.datetime.utcnow() + datetime.timedelta(minutes=args.duration)
    else:
        print("No duration specified - will run until interrupted")
        end_time = None

    tester = SQLServerTester()
    start_time = time.time()

    try:
        conn_str = TestConfiguration.get_connection_string()
        
        while tester.running and (not end_time or datetime.datetime.utcnow() < end_time):
            print(f"\n=== Starting New Test Cycle at {datetime.datetime.now()} ===\n")
            
            try:
                # Wait for SQL Server to be ready
                if not tester.wait_for_sql_server(conn_str):
                    print("\nFailed to connect to SQL Server, retrying in 10 seconds...")
                    time.sleep(10)
                    continue
                
                # Create connection
                conn = pyodbc.connect(conn_str)
                tester.log_action("INFO", "Connection", "SQL Server connection established")
                
                # Setup database
                print("\nSetting up test database...")
                try:
                    conn = tester.setup_database(conn)
                    if not tester.verify_database_setup(conn):
                        raise Exception("Database verification failed")
                except Exception as e:
                    print(f"\nERROR: Database setup failed: {str(e)}")
                    print("Will retry in 10 seconds...")
                    time.sleep(10)
                    continue
                
                # Run test iterations
                iteration = 1
                while (tester.running and 
                       (not end_time or datetime.datetime.utcnow() < end_time) and 
                       iteration <= TestConfiguration.MAX_ITERATIONS):
                    try:
                        tester.run_test_iteration(conn, iteration)
                        iteration += 1
                        
                        if iteration <= TestConfiguration.MAX_ITERATIONS:
                            print(f"\nWaiting 2 seconds before starting iteration {iteration}...")
                            time.sleep(2)
                    except Exception as e:
                        print(f"\nError in iteration {iteration}: {str(e)}")
                        tester.log_action("ERROR", "Iteration", f"Failed in iteration {iteration}", result=str(e))
                        
                        if iteration <= TestConfiguration.MAX_ITERATIONS:
                            print(f"\nWaiting 5 seconds before retrying iteration {iteration}...")
                            time.sleep(5)
                
                # Cleanup
                try:
                    tester.cleanup_database(conn)
                except Exception as e:
                    print(f"\nWARNING: Cleanup failed: {str(e)}")
                    tester.log_action("ERROR", "Cleanup", "Failed to cleanup database", result=str(e))
                
                conn.close()
                
                # Wait before next cycle if not done
                if tester.running and (not end_time or datetime.datetime.utcnow() < end_time):
                    print("\nWaiting 5 seconds before starting next cycle...")
                    time.sleep(5)
            
            except Exception as e:
                print(f"\nERROR: {str(e)}")
                tester.log_action("ERROR", "Global Error", str(e))
                
                # Wait before retry
                if tester.running and (not end_time or datetime.datetime.utcnow() < end_time):
                    print("\nWaiting 10 seconds before retry...")
                    time.sleep(10)
    
    except KeyboardInterrupt:
        print("\nOperation was cancelled")
    except Exception as e:
        print(f"\nFatal error: {str(e)}")
        tester.log_action("ERROR", "Fatal Error", str(e))
    finally:
        total_duration = (time.time() - start_time) * 1000  # Convert to milliseconds
        tester.display_summary(total_duration)
        print("\nApplication has completed execution")

if __name__ == "__main__":
    main() 