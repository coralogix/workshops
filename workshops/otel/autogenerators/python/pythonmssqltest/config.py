class TestConfiguration:
    # Database Configuration
    SQL_SERVER = "localhost"
    SQL_PORT = "1433"
    SQL_USER = "sa"
    SQL_PASSWORD = "Toortoor9#"
    SQL_DATABASE = "master"
    CONNECTION_TIMEOUT = 30
    COMMAND_TIMEOUT = 120
    MAX_SQL_PARAMETERS = 2100  # SQL Server limit

    # Table Configuration
    NUMBER_OF_TABLES = 20
    TABLE_PREFIX = "DataRecords"
    
    # Test Data Configuration
    TOTAL_RECORDS = 1000  # Reduced from 100000 for faster testing
    LOG_BATCH_SIZE = 100  # Reduced from 1000
    QUERY_LIMIT = 50      # Added to standardize query result limits
    MAX_ITERATIONS = 5    # Maximum number of iterations to run

    # Test Data Sets
    CATEGORIES = ["Small", "Medium", "Large"]
    STATUS_CODES = ["ACTIVE", "PENDING", "COMPLETED"]
    NUMBER_RANGES = [(1, 100), (100, 500), (500, 1000)]

    @classmethod
    def get_table_name(cls, index):
        return f"{cls.TABLE_PREFIX}_{index:02d}"

    @classmethod
    def get_all_table_names(cls):
        return [cls.get_table_name(i) for i in range(1, cls.NUMBER_OF_TABLES + 1)]

    @classmethod
    def get_connection_string(cls):
        return (f"DRIVER={{ODBC Driver 18 for SQL Server}};"
                f"SERVER={cls.SQL_SERVER},{cls.SQL_PORT};"
                f"DATABASE={cls.SQL_DATABASE};"
                f"UID={cls.SQL_USER};"
                f"PWD={cls.SQL_PASSWORD};"
                f"TrustServerCertificate=yes;"
                f"Connection Timeout={cls.CONNECTION_TIMEOUT}") 