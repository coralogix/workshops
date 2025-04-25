# SQL Server Performance Testing Tool (Python Version)

This is a Python implementation of a SQL Server performance testing tool that runs various query patterns and scenarios to test SQL Server performance and behavior.

## Prerequisites

- Python 3.7 or higher
- SQL Server instance running and accessible
- Ubuntu/Debian-based system (for automatic dependency installation)

## Quick Start

1. Install system dependencies:
```bash
chmod +x 1-install-dependencies.sh
./1-install-dependencies.sh
```

2. Set up Python environment:
```bash
chmod +x 2-setup-venv.sh
./2-setup-venv.sh
```

3. Run the application:
```bash
chmod +x 3-start-python.sh
./3-start-python.sh        # Run indefinitely
./3-start-python.sh 60     # Run for 60 minutes
```

## Manual Installation

If you prefer to install dependencies manually:

1. Install system dependencies:
```bash
# Install ODBC driver manager
sudo apt-get install unixodbc unixodbc-dev

# Install Microsoft ODBC driver for SQL Server
curl https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc
sudo curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list
sudo apt-get update
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18
```

2. Create virtual environment:
```bash
python3 -m venv env
source env/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
```

## Configuration

Edit the `config.py` file to configure:
- SQL Server connection details (server, port, credentials)
- Test parameters (number of tables, records, etc.)
- Test data characteristics

## Features

The tool runs several types of SQL Server tests:

1. Fast Queries:
   - Quick indexed lookups
   - Simple aggregations
   - Category-based filtering

2. Slow Queries:
   - Complex aggregations with window functions
   - String operations
   - Multi-table joins

3. Parallel Queries:
   - Large table scans with parallelism
   - Aggregations with MAXDOP hints

4. Temporary Table Operations:
   - Global temporary tables
   - Table variables
   - Intermediate result sets

5. Error Handling:
   - Arithmetic overflow
   - Type conversion errors
   - Invalid object references
   - Syntax errors

## Output

The tool provides:
- Real-time progress updates
- Error reporting
- Detailed execution summary
- Category-wise statistics
- Total duration and operation counts

## Troubleshooting

1. Connection Issues:
   - Verify SQL Server is running
   - Check connection string in config.py
   - Ensure ODBC driver is installed (`odbcinst -j` to check)
   - Verify network connectivity

2. Permission Issues:
   - Ensure SQL user has appropriate permissions
   - Check database creation rights
   - Verify temporary table creation rights

3. Performance Issues:
   - Adjust test parameters in config.py
   - Reduce number of concurrent operations
   - Increase delays between operations

4. ODBC Driver Issues:
   - Run `odbcinst -j` to check ODBC installation
   - Verify driver is installed: `ls /opt/microsoft/msodbcsql18/lib64/libmsodbcsql-18.*`
   - Check system architecture matches driver architecture

## License

This tool is provided as-is under the MIT license. 