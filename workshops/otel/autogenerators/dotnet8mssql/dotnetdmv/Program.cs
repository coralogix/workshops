using System;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;

namespace SqlRandomIntegersApp {
    public class TestConfiguration {
        // Database Configuration
        public const string SQL_SERVER = "localhost";
        public const string SQL_PORT = "1433";
        public const string SQL_USER = "sa";
        public const string SQL_PASSWORD = "Toortoor9#";
        public const string SQL_DATABASE = "master";
        public const int CONNECTION_TIMEOUT = 30;
        public const int COMMAND_TIMEOUT = 120;
        public const int MAX_SQL_PARAMETERS = 2100;  // SQL Server limit

        // Table Configuration
        public const int NUMBER_OF_TABLES = 20;
        public const string TABLE_PREFIX = "DataRecords";
        public static string GetTableName(int index) => $"{TABLE_PREFIX}_{index:D2}";
        public static string GetRandomTableName(Random random) => GetTableName(random.Next(1, NUMBER_OF_TABLES + 1));
        public static IEnumerable<string> GetAllTableNames() => 
            Enumerable.Range(1, NUMBER_OF_TABLES).Select(i => GetTableName(i));

        // Test Data Configuration
        public const int TOTAL_RECORDS = 1000;  // Reduced from 100000 for faster testing
        public const int LOG_BATCH_SIZE = 100;  // Reduced from 1000
        public const int QUERY_LIMIT = 50;      // Added to standardize query result limits
        public const int MAX_ITERATIONS = 1;   // Changed to 10 iterations
        public const bool MONITOR_DMVS = true;  // Added flag for DMV monitoring

        // Test Data Sets (Reduced for simpler testing)
        public static readonly string[] Categories = { 
            "Small", "Medium", "Large"
        };
        
        public static readonly string[] StatusCodes = { 
            "ACTIVE", "PENDING", "COMPLETED"
        };

        public static readonly (int min, int max)[] NumberRanges = {
            (1, 100), (100, 500), (500, 1000)
        };

        public static string GetConnectionString() => 
            $"Server={SQL_SERVER},{SQL_PORT};User Id={SQL_USER};Password={SQL_PASSWORD};" +
            $"Connection Timeout={CONNECTION_TIMEOUT};TrustServerCertificate=True;";
    }

    /// <summary>
    /// Enhanced SQL Server test application that demonstrates various query patterns and performance scenarios
    /// </summary>
    class Program {
        static async Task Main(string[] args) {
            int? durationMinutes = null;
            if (args.Length > 0 && int.TryParse(args[0], out int minutes)) {
                durationMinutes = minutes;
                Console.WriteLine($"Will run for {minutes} minutes");
            } else {
                Console.WriteLine("No duration specified - will run until interrupted");
            }

            DateTime? endTime = durationMinutes.HasValue 
                ? DateTime.UtcNow.AddMinutes(durationMinutes.Value) 
                : null;

            string connectionString = TestConfiguration.GetConnectionString();
            var logs = new List<LogEntry>();
            var totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            using var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true; // Prevent immediate termination
                Console.WriteLine("\nGracefully shutting down...");
                cancellationTokenSource.Cancel();
            };

            var token = cancellationTokenSource.Token;
            SqlConnection? connection = null;

            try {
                while (!token.IsCancellationRequested && (!endTime.HasValue || DateTime.UtcNow < endTime.Value)) {
                    Console.WriteLine($"\n=== Starting New Test Cycle at {DateTime.Now} ===\n");
                    
                    try {
                        // Wait for SQL Server to be ready
                        Console.WriteLine("Connecting to SQL Server...");
                        await WaitForSqlServer(connectionString, logs);

                        connection = new SqlConnection(connectionString);
                        await connection.OpenAsync();
                        LogAction(logs, "INFO", "Connection", "SQL Server connection established");

                        // Initial setup
                        Console.WriteLine("\nSetting up test database...");
                        try {
                            await SetupDatabase(connection, logs);
                        } catch (Exception e) {
                            Console.WriteLine($"\nWARNING: Database setup failed: {e.Message}");
                            Console.WriteLine("Will attempt to continue with existing database...");
                            await TryUseExistingDatabase(connection, logs);
                        }

                        // Run tests for specified number of iterations
                        int iteration = 1;
                        while (!token.IsCancellationRequested && 
                               (!endTime.HasValue || DateTime.UtcNow < endTime.Value) && 
                               iteration <= TestConfiguration.MAX_ITERATIONS) {
                            try {
                                await RunTestIteration(connection, logs, iteration);
                                iteration++;
                                
                                if (iteration <= TestConfiguration.MAX_ITERATIONS) {
                                    Console.WriteLine($"\nWaiting 2 seconds before starting iteration {iteration}...");
                                    await Task.Delay(2000, token);
                                }
                            } catch (Exception e) {
                                Console.WriteLine($"\nError in iteration {iteration}: {e.Message}");
                                LogAction(logs, "ERROR", "Iteration", $"Failed in iteration {iteration}", 0, e.Message);
                                
                                if (iteration <= TestConfiguration.MAX_ITERATIONS) {
                                    Console.WriteLine($"\nWaiting 5 seconds before retrying iteration {iteration}...");
                                    await Task.Delay(5000, token);
                                }
                            }
                        }

                        // Cleanup after iterations complete
                        if (connection.State == System.Data.ConnectionState.Open) {
                            try {
                                await CleanupDatabase(connection, logs);
                            } catch (Exception e) {
                                Console.WriteLine($"\nWARNING: Cleanup failed: {e.Message}");
                                LogAction(logs, "ERROR", "Cleanup", "Failed to cleanup database", 0, e.Message);
                            }
                        }

                        // Exit the main loop after completing iterations
                        break;

                    } catch (Exception e) {
                        Console.WriteLine($"\nERROR: {e.Message}");
                        LogAction(logs, "ERROR", "Global Error", e.Message);
                    }

                    // If we're not at the end time and not cancelled, wait a bit before next cycle
                    if (!token.IsCancellationRequested && (!endTime.HasValue || DateTime.UtcNow < endTime.Value)) {
                        Console.WriteLine("\nWaiting 5 seconds before starting next cycle...");
                        await Task.Delay(5000, token);
                    }
                }
            } catch (OperationCanceledException) {
                Console.WriteLine("\nOperation was cancelled");
            } catch (Exception e) {
                Console.WriteLine($"\nFatal error: {e.Message}");
                LogAction(logs, "ERROR", "Fatal Error", e.Message);
            } finally {
                totalStopwatch.Stop();

                // Display execution summary first
                Console.WriteLine("\n=== Test Execution Summary ===");
                DisplaySummary(logs, totalStopwatch.ElapsedMilliseconds);
                Console.WriteLine("\nApplication execution completed");
                Console.WriteLine(new string('=', 100));
                
                // Execute DMV queries as the final output
                if (TestConfiguration.MONITOR_DMVS && connection?.State == System.Data.ConnectionState.Open) {
                    try {
                        Console.WriteLine("\nGenerating final DMV report...");
                        await QueryDMVs(connection, logs);
                    } catch (Exception e) {
                        Console.WriteLine($"\nError querying DMVs: {e.Message}");
                        LogAction(logs, "ERROR", "DMV Monitoring", "Failed to query DMVs", 0, e.Message);
                    }
                }

                // Close connection if still open
                if (connection != null) {
                    connection.Dispose();
                }
            }
        }

        private static async Task TryUseExistingDatabase(SqlConnection connection, List<LogEntry> logs) {
            try {
                connection.ChangeDatabase("TestDB");
            } catch {
                await ExecuteSqlCommandAsync(logs, connection,
                    "IF DB_ID('TestDB') IS NULL CREATE DATABASE TestDB;",
                    "Create database simple", "Database");
                connection.ChangeDatabase("TestDB");
            }
        }

        private static async Task RunTestIteration(SqlConnection connection, List<LogEntry> logs, int iteration) {
            Console.WriteLine($"\n=== Starting Iteration {iteration} ===");
            
            // Define test scenarios with consistent delays between each
            var testScenarios = new List<(string name, Func<SqlConnection, List<LogEntry>, Task> action)> {
                ("Fast queries", RunFastQueries),
                ("Slow queries", RunSlowQueries),
                ("Parallel queries", RunParallelQueries),
                ("Temp table queries", RunTempTableQueries),
                ("Isolation level tests", RunIsolationLevelTests),
                ("Deadlock scenarios", RunDeadlockScenarios),
                ("Failed queries", RunFailedQueries),
                ("Problematic queries", RunProblematicQueries)
            };

            foreach (var (name, action) in testScenarios) {
                try {
                    Console.WriteLine($"\nRunning {name}...");
                    await action(connection, logs);
                    // Add a small delay between test scenarios
                    await Task.Delay(500);
                } catch (Exception e) {
                    Console.WriteLine($"Error in {name}: {e.Message}");
                    LogAction(logs, "ERROR", name, $"Failed in {name}", 0, e.Message);
                    // Continue with next scenario even if one fails
                }
            }

            Console.WriteLine($"\nCompleted Iteration {iteration}");
        }

        private static async Task CleanupAndGenerateReport(SqlConnection? connection, List<LogEntry> logs, long totalDuration) {
            if (connection != null) {
                try {
                    await CleanupDatabase(connection, logs);
                } catch (Exception e) {
                    Console.WriteLine($"\nWARNING: Cleanup failed: {e.Message}");
                    LogAction(logs, "ERROR", "Cleanup", "Failed to cleanup database", 0, e.Message);
                } finally {
                    connection.Dispose();
                }
            }

            // Generate and display summary
            DisplaySummary(logs, totalDuration);
        }

        private static void DisplaySummary(List<LogEntry> logs, long totalDuration) {
            Console.WriteLine($"Total Duration: {totalDuration/1000.0:F2} seconds");
            
            var categories = logs
                .GroupBy(l => l.Category)
                .OrderBy(g => g.Key);

            foreach (var category in categories) {
                var totalCategoryDuration = category.Sum(l => l.Duration);
                var errorCount = category.Count(l => l.Severity == "ERROR");
                var successCount = category.Count(l => l.Severity == "INFO");
                
                Console.WriteLine($"\n{category.Key}:");
                Console.WriteLine($"  Duration: {totalCategoryDuration/1000.0:F2} seconds");
                Console.WriteLine($"  Successful Operations: {successCount}");
                if (errorCount > 0) {
                    Console.WriteLine($"  Failed Operations: {errorCount}");
                    // Display error details
                    var errors = category.Where(l => l.Severity == "ERROR");
                    foreach (var error in errors) {
                        Console.WriteLine($"    - {error.Operation}: {error.Result}");
                    }
                }
            }

            var totalErrors = logs.Count(l => l.Severity == "ERROR");
            var totalSuccess = logs.Count(l => l.Severity == "INFO");
            Console.WriteLine($"\nTotal Successful Operations: {totalSuccess}");
            Console.WriteLine($"Total Failed Operations: {totalErrors}");
        }

        private static async Task WaitForSqlServer(string connectionString, List<LogEntry> logs) {
            int maxRetries = 10;
            int retryDelaySeconds = 5;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Create a sanitized connection string for logging (remove password)
            var builder = new SqlConnectionStringBuilder(connectionString);
            var sanitizedConnectionString = $"Server={builder.DataSource};Database={builder.InitialCatalog};User Id={builder.UserID};Timeout={builder.ConnectTimeout}";

            Console.WriteLine($"\nAttempting to connect to SQL Server with settings:");
            Console.WriteLine($"Server: {builder.DataSource}");
            Console.WriteLine($"Database: {builder.InitialCatalog ?? "default"}");
            Console.WriteLine($"User: {builder.UserID}");
            Console.WriteLine($"Timeout: {builder.ConnectTimeout} seconds");
            Console.WriteLine($"TrustServerCertificate: {builder.TrustServerCertificate}\n");

            for (int i = 1; i <= maxRetries; i++) {
                try {
                    Console.WriteLine($"Connection attempt {i}/{maxRetries}...");
                    using (var connection = new SqlConnection(connectionString)) {
                        await connection.OpenAsync();
                        stopwatch.Stop();
                        
                        // Get server version and state information
                        var serverInfo = await GetServerInfo(connection);
                        Console.WriteLine($"\nConnection successful!");
                        Console.WriteLine($"Server Version: {serverInfo.version}");
                        Console.WriteLine($"Server State: {connection.State}");
                        
                        LogAction(logs, "INFO", "Connection", 
                            $"SQL Server ready - Version: {serverInfo.version}, State: {connection.State}", 
                            stopwatch.ElapsedMilliseconds);

                        // Clean up any existing test databases
                        Console.WriteLine("\nChecking for and removing any existing test databases...");
                        await CleanupExistingDatabases(connection, logs);
                        return;
                    }
                } catch (SqlException e) {
                    var errorDetails = new StringBuilder();
                    errorDetails.AppendLine($"\nAttempt {i} failed with SQL error(s):");
                    
                    // Log each error in the collection
                    for (int j = 0; j < e.Errors.Count; j++) {
                        var error = e.Errors[j];
                        errorDetails.AppendLine($"Error {j + 1}:");
                        errorDetails.AppendLine($"  Message: {error.Message}");
                        errorDetails.AppendLine($"  Number: {error.Number}");
                        errorDetails.AppendLine($"  State: {error.State}");
                        errorDetails.AppendLine($"  Server: {error.Server}");
                        errorDetails.AppendLine($"  Procedure: {error.Procedure}");
                        errorDetails.AppendLine($"  LineNumber: {error.LineNumber}");
                    }

                    Console.WriteLine(errorDetails.ToString());

                    if (i == maxRetries) {
                        stopwatch.Stop();
                        LogAction(logs, "ERROR", "Connection", 
                            $"Failed to connect after {maxRetries} attempts. Connection string: {sanitizedConnectionString}. Last errors: {errorDetails}", 
                            stopwatch.ElapsedMilliseconds);
                        throw;
                    }

                    var remainingAttempts = maxRetries - i;
                    var waitTime = TimeSpan.FromSeconds(retryDelaySeconds);
                    Console.WriteLine($"Waiting {waitTime.TotalSeconds} seconds before next attempt... ({remainingAttempts} attempts remaining)");
                    await Task.Delay(waitTime);
                } catch (Exception e) {
                    Console.WriteLine($"\nAttempt {i} failed with unexpected error:");
                    Console.WriteLine($"Error Type: {e.GetType().Name}");
                    Console.WriteLine($"Message: {e.Message}");
                    if (e.InnerException != null) {
                        Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
                    }

                    if (i == maxRetries) {
                        stopwatch.Stop();
                        LogAction(logs, "ERROR", "Connection", 
                            $"Failed to connect after {maxRetries} attempts. Connection string: {sanitizedConnectionString}. Last error: {e.Message}", 
                            stopwatch.ElapsedMilliseconds);
                        throw;
                    }

                    var remainingAttempts = maxRetries - i;
                    var waitTime = TimeSpan.FromSeconds(retryDelaySeconds);
                    Console.WriteLine($"Waiting {waitTime.TotalSeconds} seconds before next attempt... ({remainingAttempts} attempts remaining)");
                    await Task.Delay(waitTime);
                }
            }
        }

        private static async Task<(string version, string state)> GetServerInfo(SqlConnection connection) {
            using (var cmd = new SqlCommand("SELECT @@VERSION", connection)) {
                var version = (await cmd.ExecuteScalarAsync())?.ToString() ?? "Unknown";
                return (version, connection.State.ToString());
            }
        }

        private static async Task CleanupExistingDatabases(SqlConnection connection, List<LogEntry> logs) {
            try {
                // Get list of databases that might be left over from previous runs
                var cmd = new SqlCommand(@"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name IN ('TestDB')", connection);

                using (var reader = await cmd.ExecuteReaderAsync()) {
                    var databasesToDelete = new List<string>();
                    while (await reader.ReadAsync()) {
                        databasesToDelete.Add(reader.GetString(0));
                    }
                    reader.Close();

                    foreach (var dbName in databasesToDelete) {
                        try {
                            await ExecuteSqlCommandAsync(logs, connection,
                                $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}];",
                                $"Drop existing database '{dbName}'", "Cleanup");
                        } catch (Exception ex) {
                            LogAction(logs, "WARN", "Cleanup", $"Failed to drop database '{dbName}'", 0, ex.Message);
                        }
                    }

                    if (databasesToDelete.Count > 0) {
                        Console.WriteLine($"Cleaned up {databasesToDelete.Count} existing test database(s).");
                    } else {
                        Console.WriteLine("No existing test databases found.");
                    }
                }
            } catch (Exception ex) {
                LogAction(logs, "WARN", "Cleanup", "Database cleanup check failed", 0, ex.Message);
                Console.WriteLine("Warning: Could not perform initial database cleanup check.");
            }
        }

        private static async Task ExecuteSqlCommandAsync(List<LogEntry> logs, SqlConnection connection, string sql, string operation, string category, SqlTransaction? transaction = null) {
            var stopwatch = new Stopwatch();
            try {
                stopwatch.Start();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    if (transaction != null) {
                        command.Transaction = transaction;
                    }
                    command.CommandTimeout = TestConfiguration.COMMAND_TIMEOUT;
                    await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();
                    LogAction(logs, "INFO", category, operation, stopwatch.ElapsedMilliseconds);
                }
            } catch (Exception e) {
                stopwatch.Stop();
                LogAction(logs, "ERROR", category, operation, stopwatch.ElapsedMilliseconds, e.Message);
                throw;
            }
        }

        private static void LogAction(List<LogEntry> logs, string severity, string category, string operation, long duration = 0, string? result = null) {
            var log = new LogEntry {
                Timestamp = DateTime.UtcNow,
                Category = category,
                Operation = operation,
                Severity = severity,
                Duration = duration,
                Result = result
            };
            logs.Add(log);

            // Print progress to console
            if (severity == "ERROR") {
                Console.WriteLine($"  {operation} - Failed: {result}");
            } else if (duration > 0) {
                Console.WriteLine($"  {operation} - Completed in {duration/1000.0:F2}s");
            }
        }

        private static async Task SetupDatabase(SqlConnection connection, List<LogEntry> logs) {
            // Drop and recreate database
            await ExecuteSqlCommandAsync(logs, connection, @"
                IF DB_ID('TestDB') IS NOT NULL 
                BEGIN 
                    ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; 
                    DROP DATABASE TestDB; 
                END
                CREATE DATABASE TestDB;", 
                "Setup database", "Database");

            connection.ChangeDatabase("TestDB");

            // Create tables
            foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                await ExecuteSqlCommandAsync(logs, connection, $@"
                    CREATE TABLE {tableName} (
                        ID BIGINT PRIMARY KEY IDENTITY(1,1),
                        Number INT,
                        Description NVARCHAR(100),
                        Category NVARCHAR(50),
                        Status NVARCHAR(20),
                        CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
                    );

                    CREATE INDEX IX_{tableName}_Number ON {tableName}(Number);
                    CREATE INDEX IX_{tableName}_Category ON {tableName}(Category);
                    CREATE INDEX IX_{tableName}_Status ON {tableName}(Status);", 
                    $"Create schema for {tableName}", "Database");
            }

            // Insert test data across all tables
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var random = new Random();

            // Calculate records per table
            int recordsPerTable = TestConfiguration.TOTAL_RECORDS / TestConfiguration.NUMBER_OF_TABLES;

            foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                using (var cmd = new SqlCommand()) {
                    cmd.Connection = connection;
                    cmd.CommandText = $@"
                        INSERT INTO {tableName} (Number, Description, Category, Status)
                        VALUES (@num, @desc, @cat, @status)";

                    var numParam = cmd.Parameters.Add("@num", System.Data.SqlDbType.Int);
                    var descParam = cmd.Parameters.Add("@desc", System.Data.SqlDbType.NVarChar, 100);
                    var catParam = cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar, 50);
                    var statusParam = cmd.Parameters.Add("@status", System.Data.SqlDbType.NVarChar, 20);

                    for (int i = 0; i < recordsPerTable; i++) {
                        numParam.Value = random.Next(1, 1000000);
                        descParam.Value = $"Test record {i} in {tableName}";
                        catParam.Value = TestConfiguration.Categories[random.Next(TestConfiguration.Categories.Length)];
                        statusParam.Value = TestConfiguration.StatusCodes[random.Next(TestConfiguration.StatusCodes.Length)];
                        
                        await cmd.ExecuteNonQueryAsync();

                        if (i % TestConfiguration.LOG_BATCH_SIZE == 0) {
                            LogAction(logs, "INFO", "Database", $"Inserted {i} records into {tableName}", stopwatch.ElapsedMilliseconds);
                        }
                    }
                }
                
                LogAction(logs, "INFO", "Database", $"Completed inserting {recordsPerTable} records into {tableName}", stopwatch.ElapsedMilliseconds);
            }
            
            stopwatch.Stop();
            LogAction(logs, "INFO", "Database", "Data Generation Complete", stopwatch.ElapsedMilliseconds);
        }

        private static string GenerateRandomText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static async Task RunFastQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            
            // Add small random delay
            await Task.Delay(random.Next(100, 300));
            
            // Test 1: Quick indexed lookups with randomization
            var range = TestConfiguration.NumberRanges[random.Next(TestConfiguration.NumberRanges.Length)];
            var table1 = TestConfiguration.GetRandomTableName(random);
            var category = GetRandomWeighted(random, TestConfiguration.Categories);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"SELECT TOP {TestConfiguration.QUERY_LIMIT} * FROM {table1} WHERE Number BETWEEN {range.min} AND {range.max} AND Category = '{category}';", 
                $"Indexed range lookup on {table1}", "Query");

            // Test 2: Category and status aggregation with index
            var status = GetRandomWeighted(random, TestConfiguration.StatusCodes);
            var table2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"SELECT Category, Status, COUNT(*) as Count, MIN(Number) as MinNumber, MAX(Number) as MaxNumber FROM {table2} WHERE Status = '{status}' GROUP BY Category, Status HAVING COUNT(*) > 5;", 
                $"Category-status aggregation on {table2}", "Query");

            // Test 3: Recent records lookup with join between two random tables
            var table3 = TestConfiguration.GetRandomTableName(random);
            var table4 = TestConfiguration.GetRandomTableName(random);
            var category2 = GetRandomWeighted(random, TestConfiguration.Categories);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"SELECT TOP {TestConfiguration.QUERY_LIMIT} t1.ID, t1.Number, t1.Category, t1.Status, t1.CreatedAt, t2.Number as RelatedNumber FROM {table3} t1 LEFT JOIN {table4} t2 ON t1.Category = t2.Category WHERE t1.Category = '{category2}' AND t1.CreatedAt >= DATEADD(MINUTE, -5, GETUTCDATE()) ORDER BY t1.CreatedAt DESC;", 
                $"Recent records lookup joining {table3} and {table4}", "Query");
        }

        private static async Task RunSlowQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            
            // Add medium random delay
            await Task.Delay(random.Next(300, 500));

            // Test 1: Complex aggregation with string operations
            var randomTable1 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"WITH DataStats AS (SELECT TOP {TestConfiguration.QUERY_LIMIT} Category, Status, COUNT(*) as RecordCount, AVG(CAST(Number as FLOAT)) as AvgNumber, STRING_AGG(CAST(ID as VARCHAR(20)), ',') as RecordIDs FROM {randomTable1} GROUP BY Category, Status) SELECT Category, Status, RecordCount, AvgNumber, COUNT(*) OVER (PARTITION BY Category) as CategoryTotal FROM DataStats WHERE RecordCount > 5 ORDER BY CategoryTotal DESC, AvgNumber DESC;", 
                "Complex aggregation", "Query");

            // Test 2: Cross apply with string operations
            var searchCategory = GetRandomWeighted(random, TestConfiguration.Categories);
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"SELECT TOP {TestConfiguration.QUERY_LIMIT} dr1.Category, dr1.Status, Matches.MatchCount, Matches.AvgNumber FROM {randomTable2} dr1 CROSS APPLY (SELECT COUNT(*) as MatchCount, AVG(CAST(dr2.Number as FLOAT)) as AvgNumber FROM {randomTable2} dr2 WHERE dr2.Category = dr1.Category AND dr2.Status = dr1.Status AND dr2.Number > dr1.Number) Matches WHERE dr1.Category = '{searchCategory}' AND Matches.MatchCount > 0 ORDER BY Matches.AvgNumber DESC;",
                "Cross apply with processing time", "Query");
        }

        private static async Task RunParallelQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            
            // Add random delay between parallel operations
            await Task.Delay(random.Next(500, 1500));

            // Test 1: Parallel full table scan with computation
            var randomTable1 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"SELECT Category, COUNT(*) as TotalCount, AVG(CAST(Number as FLOAT)) as AvgNumber, MIN(Number) as MinNumber, MAX(Number) as MaxNumber, SUM(CASE WHEN Number % 2 = 0 THEN 1 ELSE 0 END) as EvenCount, STRING_AGG(CAST(Number as VARCHAR(20)), ',') WITHIN GROUP (ORDER BY Number) as NumberList FROM {randomTable1} WHERE Number BETWEEN 1000 AND 900000 GROUP BY Category OPTION (MAXDOP 4);", 
                "Complex parallel aggregation", "Query");

            // Add random delay between parallel operations
            await Task.Delay(random.Next(1000, 2000));

            // Test 2: Parallel join operations
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"WITH NumberRanges AS (SELECT Number, Category, NTILE(100) OVER (ORDER BY Number) as Range FROM {randomTable2}) SELECT r1.Range, COUNT(*) as Combinations, AVG(ABS(r1.Number - r2.Number)) as AvgDifference, MAX(r1.Number) as MaxNumber, MIN(r2.Number) as MinNumber FROM NumberRanges r1 JOIN NumberRanges r2 ON r1.Range = r2.Range AND r1.Number <> r2.Number GROUP BY r1.Range OPTION (MAXDOP 4);",
                "Parallel join operations", "Query");

            // Test 3: Parallel data analysis with multiple operations
            var randomTable3 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, 
                $"WITH ProcessingMetrics AS (SELECT Category, Status, AVG(CAST(Number as FLOAT)) as AvgNumber, COUNT(*) as RecordCount FROM {randomTable3} GROUP BY Category, Status) SELECT Category, Status, AvgNumber, RecordCount, CAST(RecordCount as FLOAT) / NULLIF(SUM(RecordCount) OVER (PARTITION BY Category), 0) as CategoryRatio, RANK() OVER (PARTITION BY Category ORDER BY AvgNumber DESC) as ProcessingTimeRank FROM ProcessingMetrics WHERE RecordCount > 100 ORDER BY Category, ProcessingTimeRank OPTION (MAXDOP 4);",
                "Parallel metrics analysis", "Query");
        }

        private static async Task RunTempTableQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            // Reset isolation level to default
            await ExecuteSqlCommandAsync(logs, connection,
                "SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Reset isolation level", "Configuration");

            // Test 1: Global temporary table with indexes
            var randomTable1 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                BEGIN TRANSACTION;

                CREATE TABLE ##GlobalTempNumbers (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    Number INT,
                    Category NVARCHAR(50),
                    ProcessedAt DATETIME2 DEFAULT GETUTCDATE()
                );

                CREATE INDEX IX_GTN_Number ON ##GlobalTempNumbers(Number);
                
                INSERT INTO ##GlobalTempNumbers (Number, Category)
                SELECT TOP 100 Number, Category 
                FROM {randomTable1}
                WHERE Category = 'Large';
                
                SELECT 
                    DATEPART(SECOND, ProcessedAt) as ProcessedSecond,
                    COUNT(*) as NumberCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber
                FROM ##GlobalTempNumbers
                GROUP BY DATEPART(SECOND, ProcessedAt);
                
                DROP TABLE ##GlobalTempNumbers;

                COMMIT TRANSACTION;",
                "Global temp table operations", "Database");

            // Test 2: Table variables for intermediate results
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                BEGIN TRANSACTION;

                DECLARE @CategoryStats TABLE (
                    Category NVARCHAR(50) PRIMARY KEY,
                    MinNumber INT,
                    MaxNumber INT,
                    AvgNumber FLOAT,
                    NumberCount INT
                );

                INSERT INTO @CategoryStats
                SELECT 
                    Category,
                    MIN(Number) as MinNumber,
                    MAX(Number) as MaxNumber,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    COUNT(*) as NumberCount
                FROM {randomTable2}
                GROUP BY Category;

                SELECT * FROM @CategoryStats
                WHERE NumberCount > (SELECT AVG(NumberCount) FROM @CategoryStats);

                COMMIT TRANSACTION;",
                "Table variable operations", "Database");
        }

        private static async Task RunIsolationLevelTests(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            // Reset to default isolation level before starting tests
            await ExecuteSqlCommandAsync(logs, connection,
                "SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Reset isolation level", "Configuration");

            // Test 1: Read Committed with nolock hint
            var randomTable1 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
                BEGIN TRANSACTION;
                
                SELECT TOP 50 
                    dr.Number,
                    dr.Category,
                    (
                        SELECT COUNT(*) 
                        FROM {randomTable1} WITH (NOLOCK) 
                        WHERE Number > dr.Number
                    ) as LargerNumbers
                FROM {randomTable1} dr
                ORDER BY dr.Number;
                
                COMMIT TRANSACTION;

                -- Reset isolation level
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Read Committed with NOLOCK", "Query");

            // Test 2: Snapshot isolation
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                -- Enable snapshot isolation for TestDB
                ALTER DATABASE TestDB SET ALLOW_SNAPSHOT_ISOLATION ON;
                
                SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                BEGIN TRANSACTION;
                
                SELECT Category, COUNT(*) as NumberCount
                FROM {randomTable2}
                GROUP BY Category;
                
                -- Simulate some updates in a separate transaction
                UPDATE TOP (10) {randomTable2}
                SET Number = Number + 1
                WHERE Category = 'Large';
                
                -- Read again in the same transaction
                SELECT Category, COUNT(*) as NumberCount
                FROM {randomTable2}
                GROUP BY Category;
                
                COMMIT TRANSACTION;

                -- Reset isolation level
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Snapshot isolation", "Query");
        }

        private static async Task RunDeadlockScenarios(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            
            // Create additional tables for deadlock testing
            await ExecuteSqlCommandAsync(logs, connection, @"
                IF OBJECT_ID('NumberCategories', 'U') IS NOT NULL DROP TABLE NumberCategories;
                IF OBJECT_ID('NumberRanges', 'U') IS NOT NULL DROP TABLE NumberRanges;
                IF OBJECT_ID('ProcessingQueue', 'U') IS NOT NULL DROP TABLE ProcessingQueue;

                CREATE TABLE NumberCategories (
                    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
                    CategoryName NVARCHAR(50) UNIQUE,
                    LastUpdated DATETIME2 DEFAULT GETUTCDATE()
                );

                CREATE TABLE NumberRanges (
                    RangeID INT IDENTITY(1,1) PRIMARY KEY,
                    MinValue INT,
                    MaxValue INT,
                    CategoryID INT REFERENCES NumberCategories(CategoryID),
                    LastUpdated DATETIME2 DEFAULT GETUTCDATE()
                );

                CREATE TABLE ProcessingQueue (
                    QueueID INT IDENTITY(1,1) PRIMARY KEY,
                    ItemValue INT,
                    CategoryID INT REFERENCES NumberCategories(CategoryID),
                    Status NVARCHAR(20),
                    LastUpdated DATETIME2 DEFAULT GETUTCDATE()
                );", 
                "Create deadlock test tables", "Database");

            var randomTable = TestConfiguration.GetRandomTableName(random);
            
            // Populate test tables
            await ExecuteSqlCommandAsync(logs, connection, $@"
                INSERT INTO NumberCategories (CategoryName)
                SELECT DISTINCT Category FROM {randomTable};

                INSERT INTO NumberRanges (MinValue, MaxValue, CategoryID)
                SELECT 
                    MIN(Number),
                    MAX(Number),
                    nc.CategoryID
                FROM {randomTable} t
                JOIN NumberCategories nc ON t.Category = nc.CategoryName
                GROUP BY nc.CategoryID;

                INSERT INTO ProcessingQueue (ItemValue, CategoryID, Status)
                SELECT TOP 1000
                    Number,
                    nc.CategoryID,
                    'PENDING'
                FROM {randomTable} t
                JOIN NumberCategories nc ON t.Category = nc.CategoryName
                ORDER BY NEWID();",
                "Populate deadlock test tables", "Database");

            // Test 1: Classic deadlock with different lock order
            var stopwatch = new Stopwatch();
            try {
                Console.WriteLine("\nExecuting Classic Deadlock Scenario...");
                stopwatch.Start();

                var task1 = Task.Run(async () => {
                    using var conn1 = new SqlConnection(connection.ConnectionString);
                    await conn1.OpenAsync();
                    using var transaction = conn1.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        // First update NumberCategories
                        await ExecuteSqlCommandAsync(logs, conn1, @"
                            UPDATE NumberCategories 
                            SET LastUpdated = GETUTCDATE()
                            WHERE CategoryName = 'Large';
                            WAITFOR DELAY '00:00:02';
                            UPDATE NumberRanges
                            SET LastUpdated = GETUTCDATE()
                            WHERE CategoryID IN (SELECT CategoryID FROM NumberCategories WHERE CategoryName = 'Large');",
                            "Transaction 1 - Classic Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                var task2 = Task.Run(async () => {
                    using var conn2 = new SqlConnection(connection.ConnectionString);
                    await conn2.OpenAsync();
                    using var transaction = conn2.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        // First update NumberRanges
                        await ExecuteSqlCommandAsync(logs, conn2, @"
                            UPDATE NumberRanges
                            SET LastUpdated = GETUTCDATE()
                            WHERE CategoryID IN (SELECT CategoryID FROM NumberCategories WHERE CategoryName = 'Large');
                            WAITFOR DELAY '00:00:02';
                            UPDATE NumberCategories
                            SET LastUpdated = GETUTCDATE()
                            WHERE CategoryName = 'Large';",
                            "Transaction 2 - Classic Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                await Task.WhenAll(task1, task2);
            } catch (Exception ex) {
                LogAction(logs, "INFO", "Deadlock", "Classic Deadlock Scenario", stopwatch.ElapsedMilliseconds, ex.Message);
            }

            // Test 2: Multiple Row Updates Deadlock
            try {
                Console.WriteLine("\nExecuting Multiple Row Updates Deadlock Scenario...");
                var task3 = Task.Run(async () => {
                    using var conn3 = new SqlConnection(connection.ConnectionString);
                    await conn3.OpenAsync();
                    using var transaction = conn3.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        await ExecuteSqlCommandAsync(logs, conn3, @"
                            UPDATE ProcessingQueue
                            SET Status = 'PROCESSING'
                            WHERE QueueID IN (
                                SELECT TOP 50 QueueID
                                FROM ProcessingQueue
                                WHERE Status = 'PENDING'
                                ORDER BY QueueID
                            );
                            WAITFOR DELAY '00:00:01';
                            UPDATE ProcessingQueue
                            SET Status = 'COMPLETED'
                            WHERE QueueID IN (
                                SELECT TOP 50 QueueID
                                FROM ProcessingQueue
                                WHERE Status = 'PENDING'
                                ORDER BY QueueID DESC
                            );",
                            "Transaction 3 - Row Updates Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                var task4 = Task.Run(async () => {
                    using var conn4 = new SqlConnection(connection.ConnectionString);
                    await conn4.OpenAsync();
                    using var transaction = conn4.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        await ExecuteSqlCommandAsync(logs, conn4, @"
                            UPDATE ProcessingQueue
                            SET Status = 'PROCESSING'
                            WHERE QueueID IN (
                                SELECT TOP 50 QueueID
                                FROM ProcessingQueue
                                WHERE Status = 'PENDING'
                                ORDER BY QueueID DESC
                            );
                            WAITFOR DELAY '00:00:01';
                            UPDATE ProcessingQueue
                            SET Status = 'COMPLETED'
                            WHERE QueueID IN (
                                SELECT TOP 50 QueueID
                                FROM ProcessingQueue
                                WHERE Status = 'PENDING'
                                ORDER BY QueueID
                            );",
                            "Transaction 4 - Row Updates Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                await Task.WhenAll(task3, task4);
            } catch (Exception ex) {
                LogAction(logs, "INFO", "Deadlock", "Multiple Row Updates Deadlock Scenario", stopwatch.ElapsedMilliseconds, ex.Message);
            }

            // Test 3: Key Lookup Deadlock
            try {
                Console.WriteLine("\nExecuting Key Lookup Deadlock Scenario...");
                var task5 = Task.Run(async () => {
                    using var conn5 = new SqlConnection(connection.ConnectionString);
                    await conn5.OpenAsync();
                    using var transaction = conn5.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        await ExecuteSqlCommandAsync(logs, conn5, @"
                            UPDATE nc
                            SET LastUpdated = GETUTCDATE()
                            FROM NumberCategories nc
                            JOIN NumberRanges nr ON nc.CategoryID = nr.CategoryID
                            WHERE nr.MinValue < 500;
                            WAITFOR DELAY '00:00:01';
                            UPDATE NumberRanges
                            SET LastUpdated = GETUTCDATE()
                            WHERE MinValue < 500;",
                            "Transaction 5 - Key Lookup Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                var task6 = Task.Run(async () => {
                    using var conn6 = new SqlConnection(connection.ConnectionString);
                    await conn6.OpenAsync();
                    using var transaction = conn6.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
                    try {
                        await ExecuteSqlCommandAsync(logs, conn6, @"
                            UPDATE NumberRanges
                            SET LastUpdated = GETUTCDATE()
                            WHERE MinValue < 500;
                            WAITFOR DELAY '00:00:01';
                            UPDATE nc
                            SET LastUpdated = GETUTCDATE()
                            FROM NumberCategories nc
                            JOIN NumberRanges nr ON nc.CategoryID = nr.CategoryID
                            WHERE nr.MinValue < 500;",
                            "Transaction 6 - Key Lookup Deadlock", "Deadlock", transaction);
                        
                        await transaction.CommitAsync();
                    } catch (Exception) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                await Task.WhenAll(task5, task6);
            } catch (Exception ex) {
                LogAction(logs, "INFO", "Deadlock", "Key Lookup Deadlock Scenario", stopwatch.ElapsedMilliseconds, ex.Message);
            }

            stopwatch.Stop();

            // Cleanup deadlock test objects
            await ExecuteSqlCommandAsync(logs, connection, @"
                DROP TABLE ProcessingQueue;
                DROP TABLE NumberRanges;
                DROP TABLE NumberCategories;",
                "Cleanup deadlock test", "Database");
        }

        private static async Task RunFailedQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();

            Console.WriteLine("\n=== Testing Error Handling Scenarios ===");

            // Test 1: Arithmetic and conversion errors
            try {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                var randomTable = TestConfiguration.GetRandomTableName(random);
                await ExecuteSqlCommandAsync(logs, connection, 
                    $@"SELECT 
                        CASE WHEN '{errorType}' = 'OVERFLOW' THEN CAST(2147483647 + Number as INT)
                             WHEN '{errorType}' = 'CONVERSION' THEN CAST('invalid' as INT)
                             ELSE 1/0 
                        END as ErrorResult
                    FROM {randomTable} 
                    WHERE ID = 1;", 
                    "Expected arithmetic error test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Arithmetic error handling test", 0, e.Message);
            }

            // Test 2: Constraint violations
            try {
                var randomTable = TestConfiguration.GetRandomTableName(random);
                await ExecuteSqlCommandAsync(logs, connection, $@"
                    -- Attempting to violate identity constraint (expected to fail)
                    INSERT INTO {randomTable} (ID, Number, Description, Category, Status)
                    SELECT TOP 1 ID, Number, Description, Category, Status 
                    FROM {randomTable};",
                    "Expected constraint violation test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Constraint violation handling test", 0, e.Message);
            }

            // Test 3: Invalid object references
            try {
                var invalidObject = $"NonExistent_{random.Next(1000)}";
                await ExecuteSqlCommandAsync(logs, connection, 
                    $"SELECT * FROM {invalidObject};",
                    "Expected invalid object test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Invalid object handling test", 0, e.Message);
            }

            // Test 4: Syntax errors with varying complexity
            try {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                var randomTable = TestConfiguration.GetRandomTableName(random);
                var errorQuery = errorType switch {
                    "SYNTAX" => $"SELEC * FORM {randomTable}",
                    "JOIN" => $"SELECT * FROM {randomTable} INNER JOIN",
                    "GROUP" => $"SELECT Category, COUNT(*) {randomTable} GROUP Category",
                    _ => $"SELECT * FROM {randomTable} WHERE;"
                };
                await ExecuteSqlCommandAsync(logs, connection, errorQuery, "Expected syntax error test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Syntax error handling test", 0, e.Message);
            }

            // Test 5: Lock timeout simulation (this one might occasionally succeed)
            try {
                var randomTable1 = TestConfiguration.GetRandomTableName(random);
                var randomTable2 = TestConfiguration.GetRandomTableName(random);
                await ExecuteSqlCommandAsync(logs, connection, $@"
                    BEGIN TRANSACTION;
                    
                    -- Set a short lock timeout
                    SET LOCK_TIMEOUT 1000;
                    
                    -- Try to update records that might be locked
                    UPDATE TOP (5) {randomTable1} WITH (UPDLOCK)
                    SET Description = Description + ' - Updated'
                    WHERE Category IN (
                        SELECT TOP 1 Category 
                        FROM {randomTable2} WITH (UPDLOCK)
                        ORDER BY NEWID()
                    );
                    
                    COMMIT TRANSACTION;",
                    "Lock timeout test", "Query");
                LogAction(logs, "INFO", "Query", "Lock timeout test completed without errors", 0);
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Lock timeout handling test", 0, e.Message);
            }

            Console.WriteLine("=== Error Handling Tests Complete ===\n");
        }

        private static async Task RunProblematicQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();

            // Test 1: Cartesian product (cross join) - very expensive
            var table1 = TestConfiguration.GetRandomTableName(random);
            var table2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT a.*, b.* 
                FROM {table1} a, {table2} b 
                WHERE a.Number > b.Number
                ORDER BY a.Number;", 
                $"Expensive cross join between {table1} and {table2}", "Query");

            // Test 2: Nested loop with large result set
            var table3 = TestConfiguration.GetRandomTableName(random);
            var table4 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT a.Number, a.Category,
                    (SELECT COUNT(*) 
                     FROM {table4} b 
                     WHERE b.Number > a.Number) as LargerNumbers
                FROM {table3} a;",
                $"Nested loop query between {table3} and {table4}", "Query");

            // Test 3: Multiple subqueries causing high CPU
            var table5 = TestConfiguration.GetRandomTableName(random);
            var table6 = TestConfiguration.GetRandomTableName(random);
            var table7 = TestConfiguration.GetRandomTableName(random);
            var table8 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT 
                    Number,
                    Category,
                    (SELECT AVG(CAST(Number as FLOAT)) 
                     FROM {table6} b 
                     WHERE b.Category = a.Category) as CategoryAvg,
                    (SELECT MAX(Number) 
                     FROM {table7} c 
                     WHERE c.Status = a.Status) as StatusMax,
                    (SELECT COUNT(*) 
                     FROM {table8} d 
                     WHERE d.Number BETWEEN a.Number - 1000 AND a.Number + 1000) as NumberRange
                FROM {table5} a
                WHERE Number > 500;",
                $"Multiple subqueries across {table5}, {table6}, {table7}, and {table8}", "Query");

            // Test 4: Non-indexed column search with large table scan
            var table9 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT * 
                FROM {table9}
                WHERE CAST(Number AS VARCHAR) LIKE '5%'
                ORDER BY Description;",
                $"Non-indexed search on {table9}", "Query");

            // Test 5: Large result set with string operations
            var table10 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT 
                    Number,
                    UPPER(Category) as UpperCategory,
                    LOWER(Status) as LowerStatus,
                    SUBSTRING(Description, 1, 50) as TruncDesc,
                    REPLICATE(Category, 3) as RepeatedCategory
                FROM {table10}
                WHERE LEN(Description) > 10;",
                $"String operations on {table10}", "Query");

            // Test 6: Lock-inducing update with select
            var table11 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                BEGIN TRANSACTION;
                
                UPDATE {table11}
                SET Description = Description + ' - Updated'
                WHERE Number BETWEEN 1 AND 100;

                SELECT * 
                FROM {table11} WITH (HOLDLOCK)
                WHERE Number BETWEEN 1 AND 200;

                COMMIT TRANSACTION;",
                $"Lock-inducing update and select on {table11}", "Query");

            // Test 7: Parallel query with sort
            var table12 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                SELECT 
                    Category,
                    Status,
                    COUNT(*) as Count,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    STRING_AGG(CAST(Number as VARCHAR), ',') as Numbers
                FROM {table12}
                GROUP BY Category, Status
                ORDER BY COUNT(*) DESC
                OPTION (MAXDOP 4);",
                $"Parallel query with sort on {table12}", "Query");

            // Test 8: Memory-intensive operation
            var table13 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WITH NumberSequence AS (
                    SELECT Number, Category, Status,
                           ROW_NUMBER() OVER (ORDER BY Number) as RowNum
                    FROM {table13}
                )
                SELECT 
                    a.Number,
                    a.Category,
                    COUNT(*) OVER (PARTITION BY a.Category) as CategoryCount,
                    AVG(CAST(a.Number as FLOAT)) OVER (PARTITION BY a.Status) as StatusAvg,
                    MAX(a.Number) OVER (ORDER BY a.Number ROWS BETWEEN 1000 PRECEDING AND 1000 FOLLOWING) as MovingMax
                FROM NumberSequence a
                ORDER BY a.RowNum;",
                $"Memory-intensive window functions on {table13}", "Query");

            // Test 9: Blocking insert with select
            var table14 = TestConfiguration.GetRandomTableName(random);
            var table15 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                BEGIN TRANSACTION;
                
                INSERT INTO {table14} (Number, Description, Category, Status)
                SELECT 
                    Number + 1000000,
                    'Copied Record - ' + CAST(Number as VARCHAR),
                    Category,
                    Status
                FROM {table15}
                WHERE Number BETWEEN 1 AND 1000;

                -- This select will be blocked by the insert
                SELECT COUNT(*) 
                FROM {table14} WITH (TABLOCKX)
                WHERE Number > 1000000;

                COMMIT TRANSACTION;",
                $"Blocking insert with select between {table14} and {table15}", "Query");

            // Test 10: Resource-intensive batch insert with proper parameterization
            var table16 = TestConfiguration.GetRandomTableName(random);
            using (var cmd = new SqlCommand()) {
                cmd.Connection = connection;
                var sql = new StringBuilder($"INSERT INTO {table16} (Number, Description, Category, Status) VALUES ");
                var values = new List<string>();
                
                // Calculate max batch size based on parameters per record (4) and SQL Server limit
                int maxBatchSize = TestConfiguration.MAX_SQL_PARAMETERS / 4;
                int batchSize = Math.Min(500, maxBatchSize);  // Use smaller of 500 or max allowed
                
                for (int i = 0; i < batchSize; i++) {
                    string paramPrefix = $"p{i}_";
                    values.Add($"(@{paramPrefix}num, @{paramPrefix}desc, @{paramPrefix}cat, @{paramPrefix}status)");
                    
                    cmd.Parameters.AddWithValue($"@{paramPrefix}num", random.Next(1, 1000000));
                    cmd.Parameters.AddWithValue($"@{paramPrefix}desc", $"Batch Insert {i}");
                    cmd.Parameters.AddWithValue($"@{paramPrefix}cat", $"Category{i % 5}");
                    cmd.Parameters.AddWithValue($"@{paramPrefix}status", $"Status{i % 3}");
                }
                
                cmd.CommandText = sql.Append(string.Join(",", values)).ToString() + $"; SELECT COUNT(*) FROM {table16};";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task CleanupDatabase(SqlConnection connection, List<LogEntry> logs) {
            try {
                // Drop each table first to avoid any locking issues
                foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                    try {
                        await ExecuteSqlCommandAsync(logs, connection,
                            $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName};",
                            $"Drop table {tableName}", "Database");
                    } catch (Exception ex) {
                        LogAction(logs, "WARN", "Cleanup", $"Failed to drop table {tableName}", 0, ex.Message);
                    }
                }
            } catch (Exception ex) {
                LogAction(logs, "WARN", "Cleanup", "Table cleanup failed", 0, ex.Message);
            }

            // Switch to master and drop the test database
            connection.ChangeDatabase("master");
            await ExecuteSqlCommandAsync(logs, connection, 
                "ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB;",
                "Drop database", "Database");
        }

        private static async Task QueryDMVs(SqlConnection connection, List<LogEntry> logs) {
            // Remove console headers and create StringBuilder for any necessary logging
            var debugLog = new StringBuilder();
            debugLog.AppendLine("\nExecuting DMV queries...");

            // Create a log entry wrapper
            var logEntry = new Dictionary<string, object> {
                {"timestamp", DateTime.UtcNow.ToString("o")},
                {"version", "1.0"},
                {"source", "SQL_Server_DMV_Monitor"},
                {"logs", new List<Dictionary<string, object>>()}
            };

            var logsList = (List<Dictionary<string, object>>)logEntry["logs"];

            // Check permissions first
            try {
                debugLog.AppendLine("Checking DMV access permissions...");
                var permissionChecks = new Dictionary<string, string> {
                    {"Server State Permission", "SELECT CAST(HAS_PERMS_BY_NAME(NULL, NULL, 'VIEW SERVER STATE') AS bit) as has_permission"},
                    {"Database State Permission", "SELECT CAST(HAS_PERMS_BY_NAME(DB_NAME(), NULL, 'VIEW DATABASE STATE') AS bit) as has_permission"},
                    {"Server Performance Permission", "SELECT CAST(HAS_PERMS_BY_NAME(NULL, NULL, 'VIEW SERVER PERFORMANCE STATE') AS bit) as has_permission"},
                    {"Database Performance Permission", "SELECT CAST(HAS_PERMS_BY_NAME(DB_NAME(), NULL, 'VIEW DATABASE PERFORMANCE STATE') AS bit) as has_permission"}
                };

                var permissions = new Dictionary<string, bool>();
                foreach (var check in permissionChecks) {
                    using var cmd = new SqlCommand(check.Value, connection);
                    var hasPermission = Convert.ToBoolean(await cmd.ExecuteScalarAsync());
                    permissions[check.Key] = hasPermission;
                    debugLog.AppendLine($"  {check.Key}: {hasPermission}");
                }

                logsList.Add(new Dictionary<string, object> {
                    {"timestamp", DateTime.UtcNow.ToString("o")},
                    {"severity", "INFO"},
                    {"severity_number", 0},
                    {"category", "Permissions"},
                    {"message", "Permission check completed"},
                    {"data", permissions}
                });
            }
            catch (Exception ex) {
                debugLog.AppendLine($"Permission check failed: {ex.Message}");
                logsList.Add(new Dictionary<string, object> {
                    {"timestamp", DateTime.UtcNow.ToString("o")},
                    {"severity", "ERROR"},
                    {"severity_number", 2},
                    {"category", "Permissions"},
                    {"message", "Permission check failed"},
                    {"error", new Dictionary<string, object> {
                        {"type", ex.GetType().Name},
                        {"message", ex.Message},
                        {"details", ex.ToString()}
                    }}
                });
            }

            // Define DMV queries
            var dmvQueries = new Dictionary<string, string> {
                {"Server Information", @"
                    SELECT 
                        SERVERPROPERTY('ServerName') as ServerName,
                        SERVERPROPERTY('Edition') as Edition,
                        SERVERPROPERTY('ProductVersion') as Version,
                        SERVERPROPERTY('ProductLevel') as ServicePack"},

                {"Database Information", @"
                    SELECT 
                        name,
                        state_desc,
                        recovery_model_desc,
                        compatibility_level,
                        is_read_only,
                        is_auto_close_on,
                        page_verify_option_desc
                    FROM sys.databases"},

                {"Memory Usage Details", @"
                    SELECT TOP 10
                        type,
                        name,
                        pages_kb/1024.0 as pages_mb,
                        virtual_memory_reserved_kb/1024.0 as virtual_memory_reserved_mb,
                        virtual_memory_committed_kb/1024.0 as virtual_memory_committed_mb
                    FROM sys.dm_os_memory_clerks
                    ORDER BY pages_kb DESC"},

                {"CPU Usage by Database", @"
                    SELECT TOP 10
                        DB_NAME(dbid) as database_name,
                        COUNT(*) as query_count,
                        SUM(total_worker_time)/1000.0 as total_cpu_time_ms,
                        SUM(total_elapsed_time)/1000.0 as total_elapsed_time_ms,
                        SUM(total_logical_reads) as total_logical_reads
                    FROM sys.dm_exec_query_stats qs
                    CROSS APPLY sys.dm_exec_sql_text(sql_handle) st
                    WHERE dbid is not null
                    GROUP BY dbid
                    ORDER BY total_cpu_time_ms DESC"},

                {"Wait Statistics", @"
                    SELECT TOP 10
                        wait_type,
                        waiting_tasks_count,
                        wait_time_ms,
                        max_wait_time_ms,
                        signal_wait_time_ms
                    FROM sys.dm_os_wait_stats
                    WHERE wait_time_ms > 0
                    ORDER BY wait_time_ms DESC"},

                {"IO Statistics by Database", @"
                    SELECT
                        DB_NAME(database_id) as database_name,
                        file_id,
                        num_of_reads,
                        num_of_writes,
                        io_stall_read_ms,
                        io_stall_write_ms,
                        size_on_disk_bytes/1024/1024.0 as size_mb
                    FROM sys.dm_io_virtual_file_stats(NULL, NULL)"},

                {"Connection Statistics", @"
                    SELECT 
                        COUNT(*) as connection_count,
                        login_name,
                        host_name,
                        program_name,
                        status
                    FROM sys.dm_exec_sessions
                    WHERE is_user_process = 1
                    GROUP BY login_name, host_name, program_name, status"},

                {"Transaction Information", @"
                    SELECT TOP 10
                        at.transaction_id,
                        DB_NAME(dt.database_id) as database_name,
                        dt.database_transaction_begin_time,
                        dt.database_transaction_type,
                        dt.database_transaction_state,
                        es.login_name,
                        es.host_name
                    FROM sys.dm_tran_active_transactions at
                    JOIN sys.dm_tran_database_transactions dt 
                        ON at.transaction_id = dt.transaction_id
                    JOIN sys.dm_exec_sessions es 
                        ON dt.transaction_id = es.transaction_id
                    WHERE es.is_user_process = 1
                    ORDER BY dt.database_transaction_begin_time DESC"},

                {"Index Usage Statistics", @"
                    SELECT TOP 10
                        OBJECT_NAME(s.object_id) as table_name,
                        i.name as index_name,
                        s.user_seeks + s.user_scans + s.user_lookups as user_reads,
                        s.user_updates,
                        s.last_user_seek,
                        s.last_user_scan
                    FROM sys.dm_db_index_usage_stats s
                    JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
                    WHERE database_id = DB_ID()
                    ORDER BY user_reads DESC"},

                {"Query Execution Statistics", @"
                    SELECT TOP 10
                        qs.execution_count,
                        qs.total_logical_reads/execution_count as avg_logical_reads,
                        qs.total_worker_time/execution_count as avg_cpu_time,
                        qs.total_elapsed_time/execution_count as avg_elapsed_time,
                        SUBSTRING(qt.text, (qs.statement_start_offset/2)+1,
                            ((CASE qs.statement_end_offset
                                WHEN -1 THEN DATALENGTH(qt.text)
                                ELSE qs.statement_end_offset
                                END - qs.statement_start_offset)/2) + 1) as query_text
                    FROM sys.dm_exec_query_stats qs
                    CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
                    ORDER BY qs.total_worker_time/execution_count DESC"},

                {"Stored Procedure Statistics", @"
                    SELECT TOP 50
                        OBJECT_NAME(ps.object_id) AS ProcedureName,
                        ps.database_id,
                        ps.execution_count,
                        ps.total_worker_time AS CPU_Time_Ms,
                        ps.total_elapsed_time AS Total_Duration_Ms,
                        ps.total_logical_reads,
                        ps.total_logical_writes,
                        ps.total_physical_reads,
                        ps.cached_time,
                        ps.last_execution_time,
                        OBJECT_DEFINITION(ps.object_id) AS ProcedureDefinition
                    FROM sys.dm_exec_procedure_stats ps
                    ORDER BY ps.total_worker_time DESC"},

                {"Blocked Queries", @"
                    SELECT 
                        blocking.session_id AS Blocking_Session_ID,
                        blocked.session_id AS Blocked_Session_ID,
                        waitstats.wait_duration_ms,
                        waitstats.wait_type,
                        blocked_query.text AS Blocked_Query_Text,
                        blocking_query.text AS Blocking_Query_Text,
                        blocking_proc.program_name AS Blocking_Application,
                        blocked_proc.program_name AS Blocked_Application,
                        blocking.last_transaction_started,
                        blocked.last_transaction_started,
                        blocking_proc.login_time AS Blocking_Login_Time,
                        blocked_proc.login_time AS Blocked_Login_Time
                    FROM sys.dm_exec_requests blocked
                    JOIN sys.dm_exec_requests blocking 
                        ON blocking.session_id = blocked.blocking_session_id
                    JOIN sys.dm_exec_sessions blocked_proc 
                        ON blocked.session_id = blocked_proc.session_id
                    JOIN sys.dm_exec_sessions blocking_proc 
                        ON blocking.session_id = blocking_proc.session_id
                    JOIN sys.dm_os_waiting_tasks waitstats 
                        ON waitstats.waiting_task_address = blocked.task_address
                    CROSS APPLY sys.dm_exec_sql_text(blocked.sql_handle) blocked_query
                    CROSS APPLY sys.dm_exec_sql_text(blocking.sql_handle) blocking_query"},

                {"Lock Information", @"
                    SELECT 
                        dl.request_session_id,
                        dl.resource_database_id,
                        DB_NAME(dl.resource_database_id) AS DatabaseName,
                        OBJECT_NAME(dl.resource_associated_entity_id) AS ObjectName,
                        dl.resource_type,
                        dl.resource_description,
                        dl.request_mode,
                        dl.request_status,
                        es.login_name,
                        es.host_name,
                        es.program_name,
                        est.text AS QueryText,
                        eqp.query_plan AS QueryPlan
                    FROM sys.dm_tran_locks dl
                    LEFT JOIN sys.dm_exec_sessions es 
                        ON dl.request_session_id = es.session_id
                    LEFT JOIN sys.dm_exec_requests er 
                        ON dl.request_session_id = er.session_id
                    OUTER APPLY sys.dm_exec_sql_text(er.sql_handle) est
                    OUTER APPLY sys.dm_exec_query_plan(er.plan_handle) eqp
                    WHERE dl.resource_type != 'DATABASE'
                    ORDER BY dl.request_session_id"},

                {"Currently Executing Queries", @"
                    SELECT 
                        er.session_id,
                        er.start_time,
                        er.status,
                        er.command,
                        er.cpu_time,
                        er.total_elapsed_time,
                        er.reads,
                        er.writes,
                        er.logical_reads,
                        est.text AS QueryText,
                        eqp.query_plan AS QueryPlan,
                        es.login_name,
                        es.host_name,
                        es.program_name,
                        DB_NAME(er.database_id) AS DatabaseName
                    FROM sys.dm_exec_requests er
                    JOIN sys.dm_exec_sessions es 
                        ON er.session_id = es.session_id
                    OUTER APPLY sys.dm_exec_sql_text(er.sql_handle) est
                    OUTER APPLY sys.dm_exec_query_plan(er.plan_handle) eqp
                    WHERE es.is_user_process = 1
                    ORDER BY er.total_elapsed_time DESC"},

                {"Lock Escalation Statistics", @"
                    SELECT 
                        ddios.database_id,
                        DB_NAME(ddios.database_id) AS DatabaseName,
                        ddios.object_id,
                        OBJECT_NAME(ddios.object_id) AS ObjectName,
                        ddios.index_id,
                        ddios.partition_number,
                        ddios.page_lock_count,
                        ddios.page_lock_wait_count,
                        ddios.page_lock_wait_in_ms,
                        ddios.row_lock_count,
                        ddios.row_lock_wait_count,
                        ddios.row_lock_wait_in_ms,
                        ddios.index_lock_promotion_attempt_count,
                        ddios.index_lock_promotion_count,
                        ddios.page_latch_wait_count,
                        ddios.page_latch_wait_in_ms,
                        ddios.page_io_latch_wait_count,
                        ddios.page_io_latch_wait_in_ms
                    FROM sys.dm_db_index_operational_stats(NULL, NULL, NULL, NULL) ddios
                    WHERE (page_lock_wait_in_ms > 0 
                        OR row_lock_wait_in_ms > 0 
                        OR page_latch_wait_in_ms > 0 
                        OR page_io_latch_wait_in_ms > 0)
                    ORDER BY 
                        (page_lock_wait_in_ms + row_lock_wait_in_ms + 
                         page_latch_wait_in_ms + page_io_latch_wait_in_ms) DESC"},

                {"Detailed Query Introspection", @"
                    WITH QueryStats AS (
                        SELECT TOP 50
                            qs.sql_handle,
                            qs.plan_handle,
                            qs.execution_count,
                            qs.total_worker_time,
                            qs.total_elapsed_time,
                            qs.total_logical_reads,
                            qs.total_physical_reads,
                            qs.total_logical_writes,
                            qs.creation_time,
                            qs.last_execution_time,
                            qs.statement_start_offset,
                            qs.statement_end_offset,
                            qs.min_worker_time,
                            qs.max_worker_time,
                            qs.min_elapsed_time,
                            qs.max_elapsed_time,
                            qs.min_logical_reads,
                            qs.max_logical_reads,
                            qs.plan_generation_num,
                            qs.total_rows,
                            qs.last_rows,
                            qs.min_rows,
                            qs.max_rows,
                            qs.total_dop,
                            qs.last_dop,
                            qs.min_dop,
                            qs.max_dop,
                            qs.total_grant_kb,
                            qs.last_grant_kb,
                            qs.min_grant_kb,
                            qs.max_grant_kb,
                            qs.total_used_grant_kb,
                            qs.last_used_grant_kb,
                            qs.min_used_grant_kb,
                            qs.max_used_grant_kb,
                            qs.total_ideal_grant_kb,
                            qs.last_ideal_grant_kb,
                            qs.min_ideal_grant_kb,
                            qs.max_ideal_grant_kb
                        FROM sys.dm_exec_query_stats qs
                        ORDER BY qs.total_worker_time DESC
                    )
                    SELECT 
                        qs.*,
                        SUBSTRING(st.text, 
                            (qs.statement_start_offset/2)+1,
                            ((CASE qs.statement_end_offset
                                WHEN -1 THEN DATALENGTH(st.text)
                                ELSE qs.statement_end_offset
                                END - qs.statement_start_offset)/2) + 1) as QueryText,
                        qp.query_plan as QueryPlan,
                        DB_NAME(st.dbid) as DatabaseName,
                        OBJECT_NAME(st.objectid, st.dbid) as ObjectName,
                        -- Performance Metrics
                        CAST(qs.total_worker_time / 1000000.0 as decimal(18,2)) as total_cpu_seconds,
                        CAST(qs.total_elapsed_time / 1000000.0 as decimal(18,2)) as total_duration_seconds,
                        CAST(qs.total_worker_time * 1.0 / qs.execution_count / 1000000.0 as decimal(18,2)) as avg_cpu_seconds,
                        CAST(qs.total_elapsed_time * 1.0 / qs.execution_count / 1000000.0 as decimal(18,2)) as avg_duration_seconds,
                        CAST(qs.total_logical_reads * 1.0 / qs.execution_count as decimal(18,2)) as avg_logical_reads,
                        CAST(qs.total_physical_reads * 1.0 / qs.execution_count as decimal(18,2)) as avg_physical_reads,
                        CAST(qs.total_logical_writes * 1.0 / qs.execution_count as decimal(18,2)) as avg_logical_writes,
                        -- Memory Metrics
                        CAST(qs.total_grant_kb * 1.0 / qs.execution_count / 1024.0 as decimal(18,2)) as avg_memory_grant_mb,
                        CAST(qs.total_used_grant_kb * 1.0 / qs.execution_count / 1024.0 as decimal(18,2)) as avg_memory_used_mb,
                        CAST(qs.total_ideal_grant_kb * 1.0 / qs.execution_count / 1024.0 as decimal(18,2)) as avg_memory_ideal_mb,
                        -- Parallelism Metrics
                        CAST(qs.total_dop * 1.0 / qs.execution_count as decimal(18,2)) as avg_dop,
                        -- Row Metrics
                        CAST(qs.total_rows * 1.0 / qs.execution_count as decimal(18,2)) as avg_rows,
                        -- Execution Pattern
                        DATEDIFF(MINUTE, qs.creation_time, qs.last_execution_time) as minutes_since_creation,
                        DATEDIFF(MINUTE, qs.last_execution_time, GETUTCDATE()) as minutes_since_last_execution,
                        CAST(qs.execution_count * 1.0 / 
                            CASE WHEN DATEDIFF(MINUTE, qs.creation_time, GETUTCDATE()) = 0 
                                THEN 1 
                                ELSE DATEDIFF(MINUTE, qs.creation_time, GETUTCDATE()) 
                            END as decimal(18,2)) as executions_per_minute
                    FROM QueryStats qs
                    CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
                    OUTER APPLY sys.dm_exec_query_plan(qs.plan_handle) qp"},
            };

            // Execute DMV queries
            foreach (var dmvQuery in dmvQueries) {
                try {
                    debugLog.AppendLine($"\nExecuting query: {dmvQuery.Key}");
                    using var command = new SqlCommand(dmvQuery.Value, connection);
                    command.CommandTimeout = TestConfiguration.COMMAND_TIMEOUT;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    using var reader = await command.ExecuteReaderAsync();
                    var data = new List<Dictionary<string, object>>();
                    
                    while (await reader.ReadAsync()) {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++) {
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i) ?? $"Column{i}";
                            row[columnName] = value == DBNull.Value ? null : value;
                        }
                        data.Add(row);
                    }

                    stopwatch.Stop();
                    debugLog.AppendLine($"  Retrieved {data.Count} rows in {stopwatch.ElapsedMilliseconds}ms");

                    logsList.Add(new Dictionary<string, object> {
                        {"timestamp", DateTime.UtcNow.ToString("o")},
                        {"severity", "INFO"},
                        {"severity_number", 0},
                        {"category", dmvQuery.Key},
                        {"message", $"Successfully retrieved DMV data for {dmvQuery.Key}"},
                        {"metadata", new Dictionary<string, object> {
                            {"execution_time_ms", stopwatch.ElapsedMilliseconds},
                            {"row_count", data.Count}
                        }},
                        {"data", data}
                    });

                    // Update internal logs but don't print to console
                    LogAction(logs, "INFO", "DMV Query", dmvQuery.Key, stopwatch.ElapsedMilliseconds, 
                        $"Successfully retrieved {data.Count} rows");
                }
                catch (SqlException ex) {
                    debugLog.AppendLine($"  Error executing query: {ex.Message}");
                    logsList.Add(new Dictionary<string, object> {
                        {"timestamp", DateTime.UtcNow.ToString("o")},
                        {"severity", "ERROR"},
                        {"severity_number", 2},
                        {"category", dmvQuery.Key},
                        {"message", $"SQL error occurred while querying {dmvQuery.Key}"},
                        {"error", new Dictionary<string, object> {
                            {"type", "SqlException"},
                            {"number", ex.Number},
                            {"state", ex.State},
                            {"class", ex.Class},
                            {"line_number", ex.LineNumber},
                            {"procedure", ex.Procedure ?? "N/A"},
                            {"message", ex.Message},
                            {"inner_exception", ex.InnerException?.Message ?? "None"}
                        }}
                    });

                    LogAction(logs, "ERROR", "DMV Query", dmvQuery.Key, 0, 
                        $"SQL Error {ex.Number}: {ex.Message}");
                }
                catch (Exception ex) {
                    debugLog.AppendLine($"  Error executing query: {ex.Message}");
                    logsList.Add(new Dictionary<string, object> {
                        {"timestamp", DateTime.UtcNow.ToString("o")},
                        {"severity", "ERROR"},
                        {"severity_number", 2},
                        {"category", dmvQuery.Key},
                        {"message", $"Error occurred while querying {dmvQuery.Key}"},
                        {"error", new Dictionary<string, object> {
                            {"type", ex.GetType().Name},
                            {"message", ex.Message},
                            {"inner_exception", ex.InnerException?.Message ?? "None"},
                            {"stack_trace", ex.StackTrace ?? "No stack trace available"}
                        }}
                    });

                    LogAction(logs, "ERROR", "DMV Query", dmvQuery.Key, 0, 
                        $"{ex.GetType().Name}: {ex.Message}");
                }
            }

            // Add summary information
            logsList.Add(new Dictionary<string, object> {
                {"timestamp", DateTime.UtcNow.ToString("o")},
                {"severity", "INFO"},
                {"severity_number", 0},
                {"category", "Summary"},
                {"message", "DMV query execution completed"},
                {"metadata", new Dictionary<string, object> {
                    {"total_queries", dmvQueries.Count},
                    {"successful_queries", logsList.Count(l => l["severity"].ToString() == "INFO" && l["category"].ToString() != "Summary")},
                    {"failed_queries", logsList.Count(l => l["severity"].ToString() == "ERROR")},
                    {"total_execution_time_ms", logsList
                        .Where(l => l.ContainsKey("metadata"))
                        .Sum(l => Convert.ToInt64(((Dictionary<string, object>)l["metadata"])
                            .GetValueOrDefault("execution_time_ms", 0)))}
                }}
            });

            // Write debug log to file if needed
            var debugLogFileName = $"dmv_debug_log_{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            await File.WriteAllTextAsync(debugLogFileName, debugLog.ToString());

            // Write the complete JSON log to file
            var jsonLogFileName = $"dmv_query_log_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            await File.WriteAllTextAsync(jsonLogFileName, JsonConvert.SerializeObject(logEntry, Formatting.Indented));

            // Clear the console and output only the final JSON report
            Console.Clear();
            Console.WriteLine(JsonConvert.SerializeObject(logEntry, Formatting.Indented));
        }

        // Helper method to get random test data with weighted probabilities
        private static T GetRandomWeighted<T>(Random random, T[] items, double errorProbability = 0.1) {
            if (random.NextDouble() < errorProbability) {
                // Return items from the latter half of the array (usually error/edge cases)
                return items[random.Next(items.Length / 2, items.Length)];
            }
            // Return items from the first half (usually normal cases)
            return items[random.Next(0, items.Length / 2)];
        }

        // Added more test variations
        private static readonly string[] ErrorTypes = new[] {
            "TIMEOUT", "DEADLOCK", "CONSTRAINT", "OVERFLOW", "CONVERSION",
            "PERMISSION", "MEMORY", "IO", "NETWORK", "VALIDATION"
        };
    }

    class LogEntry {
        public DateTime Timestamp { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public long Duration { get; set; }
        public string? Result { get; set; }
    }
}
