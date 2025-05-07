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
        public const int MAX_ITERATIONS = 5;    // Maximum number of iterations to run

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

            try {
                while (!token.IsCancellationRequested && (!endTime.HasValue || DateTime.UtcNow < endTime.Value)) {
                    Console.WriteLine($"\n=== Starting New Test Cycle at {DateTime.Now} ===\n");
                    
                    SqlConnection? connection = null;
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

                    } catch (Exception e) {
                        Console.WriteLine($"\nERROR: {e.Message}");
                        LogAction(logs, "ERROR", "Global Error", e.Message);
                    } finally {
                        if (connection != null) {
                            connection.Dispose();
                        }
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
                DisplaySummary(logs, totalStopwatch.ElapsedMilliseconds);
                Console.WriteLine("\nApplication has completed execution");
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
            // Direct query scenarios
            var directScenarios = new List<(string name, Func<SqlConnection, List<LogEntry>, Task> action)> {
                ("Fast queries", RunFastQueries),
                ("Slow queries", RunSlowQueries),
                ("Parallel queries", RunParallelQueries),
                ("Temp table queries", RunTempTableQueries),
                ("Isolation level tests", RunIsolationLevelTests),
                ("Deadlock scenarios", RunDeadlockScenarios),
                ("Failed queries", RunFailedQueries),
                ("Problematic queries", RunProblematicQueries)
            };
            // Stored procedure scenarios
            var spScenarios = new List<(string name, Func<SqlConnection, List<LogEntry>, Task> action)> {
                ("Fast queries (SP)", RunFastQueriesSP),
                ("Slow queries (SP)", RunSlowQueriesSP),
                ("Parallel queries (SP)", RunParallelQueriesSP),
                ("Temp table queries (SP)", RunTempTableQueriesSP),
                ("Isolation level tests (SP)", RunIsolationLevelTestsSP),
                ("Deadlock scenarios (SP)", RunDeadlockScenariosSP),
                ("Failed queries (SP)", RunFailedQueriesSP),
                ("Problematic queries (SP)", RunProblematicQueriesSP)
            };
            foreach (var (name, action) in directScenarios) {
                try {
                    Console.WriteLine($"\nRunning {name}...");
                    await action(connection, logs);
                    await Task.Delay(500);
                } catch (Exception e) {
                    Console.WriteLine($"Error in {name}: {e.Message}");
                    LogAction(logs, "ERROR", name, $"Failed in {name}", 0, e.Message);
                }
            }
            foreach (var (name, action) in spScenarios) {
                try {
                    Console.WriteLine($"\nRunning {name}...");
                    await action(connection, logs);
                    await Task.Delay(500);
                } catch (Exception e) {
                    Console.WriteLine($"Error in {name}: {e.Message}");
                    LogAction(logs, "ERROR", name, $"Failed in {name}", 0, e.Message);
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
            Console.WriteLine("\n=== Test Execution Summary ===");
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
            // Only create database if it does not exist
            await ExecuteSqlCommandAsync(logs, connection, "IF DB_ID('TestDB') IS NULL CREATE DATABASE TestDB;", "Setup database", "Database");

            connection.ChangeDatabase("TestDB");

            // Create tables if they do not exist
            foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                await ExecuteSqlCommandAsync(logs, connection, $"IF OBJECT_ID('{tableName}', 'U') IS NULL BEGIN CREATE TABLE {tableName} (ID BIGINT PRIMARY KEY IDENTITY(1,1), Number INT, Description NVARCHAR(100), Category NVARCHAR(50), Status NVARCHAR(20), CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()); CREATE INDEX IX_{tableName}_Number ON {tableName}(Number); CREATE INDEX IX_{tableName}_Category ON {tableName}(Category); CREATE INDEX IX_{tableName}_Status ON {tableName}(Status); END", $"Create schema for {tableName}", "Database");
            }

            // Insert test data across all tables (skip if table already has data)
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var random = new Random();
            int recordsPerTable = TestConfiguration.TOTAL_RECORDS / TestConfiguration.NUMBER_OF_TABLES;
            foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                // Check if table already has data
                using (var checkCmd = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", connection)) {
                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0) continue;
                }
                using (var cmd = new SqlCommand()) {
                    cmd.Connection = connection;
                    cmd.CommandText = $"INSERT INTO {tableName} (Number, Description, Category, Status) VALUES (@num, @desc, @cat, @status)";
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

            // Create or alter stored procedures (no drops)
            var procedureDefinitions = new[]
            {
                "CREATE OR ALTER PROCEDURE usp_ComplexAggregation AS BEGIN SET NOCOUNT ON; SELECT Category, Status, COUNT(*) AS RecordCount, AVG(Number) AS AvgNumber FROM DataRecords_01 GROUP BY Category, Status; END;",
                "CREATE OR ALTER PROCEDURE usp_JoinAndTempTable AS BEGIN SET NOCOUNT ON; SELECT t1.ID, t1.Number, t2.Description INTO #TempJoin FROM DataRecords_01 t1 INNER JOIN DataRecords_02 t2 ON t1.Category = t2.Category; SELECT COUNT(*) AS TempCount FROM #TempJoin; DROP TABLE #TempJoin; END;",
                "CREATE OR ALTER PROCEDURE usp_ErrorHandlingDemo @InputNumber INT AS BEGIN SET NOCOUNT ON; BEGIN TRY IF @InputNumber < 0 THROW 50001, 'Negative numbers not allowed', 1; SELECT TOP 1 * FROM DataRecords_01 WHERE Number = @InputNumber; END TRY BEGIN CATCH SELECT ERROR_MESSAGE() AS ErrorMessage; END CATCH END;",
                "CREATE OR ALTER PROCEDURE usp_FastQueries AS BEGIN SET NOCOUNT ON; SELECT TOP 50 * FROM DataRecords_01 WHERE Number BETWEEN 1 AND 100 AND Category = 'Small'; SELECT Category, Status, COUNT(*) as Count, MIN(Number) as MinNumber, MAX(Number) as MaxNumber FROM DataRecords_02 WHERE Status = 'ACTIVE' GROUP BY Category, Status HAVING COUNT(*) > 5; SELECT TOP 50 t1.ID, t1.Number, t1.Category, t1.Status, t1.CreatedAt, t2.Number as RelatedNumber FROM DataRecords_03 t1 LEFT JOIN DataRecords_04 t2 ON t1.Category = t2.Category WHERE t1.Category = 'Medium' AND t1.CreatedAt >= DATEADD(MINUTE, -5, GETUTCDATE()) ORDER BY t1.CreatedAt DESC; END;",
                "CREATE OR ALTER PROCEDURE usp_SlowQueries AS BEGIN SET NOCOUNT ON; WITH DataStats AS (SELECT TOP 50 Category, Status, COUNT(*) as RecordCount, AVG(CAST(Number as FLOAT)) as AvgNumber, STRING_AGG(CAST(ID as VARCHAR(20)), ',') as RecordIDs FROM DataRecords_05 GROUP BY Category, Status) SELECT Category, Status, RecordCount, AvgNumber, COUNT(*) OVER (PARTITION BY Category) as CategoryTotal FROM DataStats WHERE RecordCount > 5 ORDER BY CategoryTotal DESC, AvgNumber DESC; SELECT TOP 50 dr1.Category, dr1.Status, Matches.MatchCount, Matches.AvgNumber FROM DataRecords_06 dr1 CROSS APPLY (SELECT COUNT(*) as MatchCount, AVG(CAST(dr2.Number as FLOAT)) as AvgNumber FROM DataRecords_06 dr2 WHERE dr2.Category = dr1.Category AND dr2.Status = dr1.Status AND dr2.Number > dr1.Number) Matches WHERE dr1.Category = 'Large' AND Matches.MatchCount > 0 ORDER BY Matches.AvgNumber DESC; END;",
                "CREATE OR ALTER PROCEDURE usp_ParallelQueries AS BEGIN SET NOCOUNT ON; SELECT Category, COUNT(*) as TotalCount, AVG(CAST(Number as FLOAT)) as AvgNumber, MIN(Number) as MinNumber, MAX(Number) as MaxNumber, SUM(CASE WHEN Number % 2 = 0 THEN 1 ELSE 0 END) as EvenCount, STRING_AGG(CAST(Number as VARCHAR(20)), ',') WITHIN GROUP (ORDER BY Number) as NumberList FROM DataRecords_07 WHERE Number BETWEEN 1000 AND 900000 GROUP BY Category OPTION (MAXDOP 4); WITH NumberRanges AS (SELECT Number, Category, NTILE(100) OVER (ORDER BY Number) as Range FROM DataRecords_08) SELECT r1.Range, COUNT(*) as Combinations, AVG(ABS(r1.Number - r2.Number)) as AvgDifference, MAX(r1.Number) as MaxNumber, MIN(r2.Number) as MinNumber FROM NumberRanges r1 JOIN NumberRanges r2 ON r1.Range = r2.Range AND r1.Number <> r2.Number GROUP BY r1.Range OPTION (MAXDOP 4); WITH ProcessingMetrics AS (SELECT Category, Status, AVG(CAST(Number as FLOAT)) as AvgNumber, COUNT(*) as RecordCount FROM DataRecords_09 GROUP BY Category, Status) SELECT Category, Status, AvgNumber, RecordCount, CAST(RecordCount as FLOAT) / NULLIF(SUM(RecordCount) OVER (PARTITION BY Category), 0) as CategoryRatio, RANK() OVER (PARTITION BY Category ORDER BY AvgNumber DESC) as ProcessingTimeRank FROM ProcessingMetrics WHERE RecordCount > 100 ORDER BY Category, ProcessingTimeRank OPTION (MAXDOP 4); END;",
                "CREATE OR ALTER PROCEDURE usp_TempTableQueries AS BEGIN SET NOCOUNT ON; CREATE TABLE #TempNumbers (ID INT IDENTITY(1,1) PRIMARY KEY, Number INT, Category NVARCHAR(50), ProcessedAt DATETIME2 DEFAULT GETUTCDATE()); INSERT INTO #TempNumbers (Number, Category) SELECT TOP 100 Number, Category FROM DataRecords_10 WHERE Category = 'Large'; SELECT DATEPART(SECOND, ProcessedAt) as ProcessedSecond, COUNT(*) as NumberCount, AVG(CAST(Number as FLOAT)) as AvgNumber FROM #TempNumbers GROUP BY DATEPART(SECOND, ProcessedAt); DROP TABLE #TempNumbers; END;",
                "CREATE OR ALTER PROCEDURE usp_IsolationLevelTests AS BEGIN SET NOCOUNT ON; SET TRANSACTION ISOLATION LEVEL READ COMMITTED; SELECT TOP 50 dr.Number, dr.Category FROM DataRecords_11 dr ORDER BY dr.Number; END;",
                "CREATE OR ALTER PROCEDURE usp_DeadlockScenarios AS BEGIN SET NOCOUNT ON; SELECT 1 AS DeadlockSimulated; END;",
                "CREATE OR ALTER PROCEDURE usp_FailedQueries AS BEGIN SET NOCOUNT ON; BEGIN TRY SELECT 1/0 AS WillFail; END TRY BEGIN CATCH SELECT ERROR_MESSAGE() AS ErrorMessage; END CATCH END;",
                "CREATE OR ALTER PROCEDURE usp_ProblematicQueries AS BEGIN SET NOCOUNT ON; SELECT a.*, b.* FROM DataRecords_12 a, DataRecords_13 b WHERE a.Number > b.Number; END;"
            };
            foreach (var procSql in procedureDefinitions)
            {
                await ExecuteSqlCommandAsync(logs, connection, procSql, "Create stored procedure", "Database");
            }

            // Log all user-defined stored procedures after creation
            try
            {
                using (var cmd = new SqlCommand("SELECT SCHEMA_NAME(schema_id) AS schema_name, name FROM sys.procedures WHERE is_ms_shipped = 0;", connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    Console.WriteLine("Stored procedures present in TestDB after creation:");
                    bool found = false;
                    while (await reader.ReadAsync())
                    {
                        found = true;
                        Console.WriteLine($"{reader.GetString(0)}.{reader.GetString(1)}");
                    }
                    if (!found)
                    {
                        Console.WriteLine("(none)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception while listing stored procedures: {ex.Message}");
            }
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
            await ExecuteSqlCommandAsync(logs, connection, $"BEGIN TRANSACTION; CREATE TABLE ##GlobalTempNumbers (ID INT IDENTITY(1,1) PRIMARY KEY, Number INT, Category NVARCHAR(50), ProcessedAt DATETIME2 DEFAULT GETUTCDATE()); CREATE INDEX IX_GTN_Number ON ##GlobalTempNumbers(Number); INSERT INTO ##GlobalTempNumbers (Number, Category) SELECT TOP 100 Number, Category FROM {randomTable1} WHERE Category = 'Large'; SELECT DATEPART(SECOND, ProcessedAt) as ProcessedSecond, COUNT(*) as NumberCount, AVG(CAST(Number as FLOAT)) as AvgNumber FROM ##GlobalTempNumbers GROUP BY DATEPART(SECOND, ProcessedAt); DROP TABLE ##GlobalTempNumbers; COMMIT TRANSACTION;",
                "Global temp table operations", "Database");

            // Test 2: Table variables for intermediate results
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"BEGIN TRANSACTION; DECLARE @CategoryStats TABLE (Category NVARCHAR(50) PRIMARY KEY, MinNumber INT, MaxNumber INT, AvgNumber FLOAT, NumberCount INT); INSERT INTO @CategoryStats SELECT Category, MIN(Number) as MinNumber, MAX(Number) as MaxNumber, AVG(CAST(Number as FLOAT)) as AvgNumber, COUNT(*) as NumberCount FROM {randomTable2} GROUP BY Category; SELECT * FROM @CategoryStats WHERE NumberCount > (SELECT AVG(NumberCount) FROM @CategoryStats); COMMIT TRANSACTION;",
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
            await ExecuteSqlCommandAsync(logs, connection, $"SET TRANSACTION ISOLATION LEVEL READ COMMITTED; BEGIN TRANSACTION; SELECT TOP 50 dr.Number, dr.Category, (SELECT COUNT(*) FROM {randomTable1} WITH (NOLOCK) WHERE Number > dr.Number) as LargerNumbers FROM {randomTable1} dr ORDER BY dr.Number; COMMIT TRANSACTION; SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Read Committed with NOLOCK", "Query");

            // Test 2: Snapshot isolation
            var randomTable2 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"ALTER DATABASE TestDB SET ALLOW_SNAPSHOT_ISOLATION ON; SET TRANSACTION ISOLATION LEVEL SNAPSHOT; BEGIN TRANSACTION; SELECT Category, COUNT(*) as NumberCount FROM {randomTable2} GROUP BY Category; UPDATE TOP (10) {randomTable2} SET Number = Number + 1 WHERE Category = 'Large'; SELECT Category, COUNT(*) as NumberCount FROM {randomTable2} GROUP BY Category; COMMIT TRANSACTION; SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Snapshot isolation", "Query");
        }

        private static async Task RunDeadlockScenarios(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();
            // Create additional table for deadlock testing
            await ExecuteSqlCommandAsync(logs, connection, "CREATE TABLE NumberCategories (CategoryID INT IDENTITY(1,1) PRIMARY KEY, CategoryName NVARCHAR(50) UNIQUE, LastUpdated DATETIME2 DEFAULT GETUTCDATE());", 
                "Create deadlock test table", "Database");

            var randomTable = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"INSERT INTO NumberCategories (CategoryName) SELECT DISTINCT Category FROM {randomTable};",
                "Populate deadlock test table", "Database");

            // Test 1: Simulate deadlock with update operations
            var stopwatch = new Stopwatch();
            try {
                // First transaction
                var task1 = Task.Run(async () => {
                    using (var conn1 = new SqlConnection(connection.ConnectionString)) {
                        await conn1.OpenAsync();
                        using (var transaction = conn1.BeginTransaction()) {
                            await ExecuteSqlCommandAsync(logs, conn1, $"UPDATE {randomTable} SET Number = Number + 1 WHERE Category = 'Large'; WAITFOR DELAY '00:00:02'; UPDATE NumberCategories SET LastUpdated = GETUTCDATE() WHERE CategoryName = 'Large';",
                                "Transaction 1", "Query", transaction);
                            transaction.Commit();
                        }
                    }
                });

                // Second transaction
                var task2 = Task.Run(async () => {
                    using (var conn2 = new SqlConnection(connection.ConnectionString)) {
                        await conn2.OpenAsync();
                        using (var transaction = conn2.BeginTransaction()) {
                            await ExecuteSqlCommandAsync(logs, conn2, $"UPDATE NumberCategories SET LastUpdated = GETUTCDATE() WHERE CategoryName = 'Large'; WAITFOR DELAY '00:00:02'; UPDATE {randomTable} SET Number = Number + 2 WHERE Category = 'Large';",
                                "Transaction 2", "Query", transaction);
                            transaction.Commit();
                        }
                    }
                });

                // Add timeout to prevent hanging
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
                var completedTask = await Task.WhenAny(
                    Task.WhenAll(task1, task2),
                    timeoutTask
                );

                if (completedTask == timeoutTask) {
                    LogAction(logs, "INFO", "Query", "Deadlock Test - Timeout occurred", stopwatch.ElapsedMilliseconds);
                } else {
                    await Task.WhenAll(task1, task2);
                }
            } catch (Exception e) {
                LogAction(logs, "INFO", "Query", "Deadlock Test", stopwatch.ElapsedMilliseconds, e.Message);
            }

            // Cleanup deadlock test objects
            await ExecuteSqlCommandAsync(logs, connection, 
                "DROP TABLE NumberCategories;",
                "Cleanup deadlock test", "Database");
        }

        private static async Task RunFailedQueries(SqlConnection connection, List<LogEntry> logs) {
            var random = new Random();

            Console.WriteLine("\n=== Testing Error Handling Scenarios ===");

            // Test 1: Arithmetic and conversion errors
            try {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                var randomTable = TestConfiguration.GetRandomTableName(random);
                await ExecuteSqlCommandAsync(logs, connection, $"SELECT CASE WHEN '{errorType}' = 'OVERFLOW' THEN CAST(2147483647 + Number as INT) WHEN '{errorType}' = 'CONVERSION' THEN CAST('invalid' as INT) ELSE 1/0 END as ErrorResult FROM {randomTable} WHERE ID = 1;", "Expected arithmetic error test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Arithmetic error handling test", 0, e.Message);
            }

            // Test 2: Constraint violations
            try {
                var randomTable = TestConfiguration.GetRandomTableName(random);
                await ExecuteSqlCommandAsync(logs, connection, $"INSERT INTO {randomTable} (ID, Number, Description, Category, Status) SELECT TOP 1 ID, Number, Description, Category, Status FROM {randomTable};", "Expected constraint violation test", "Query");
            } catch (Exception e) {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Constraint violation handling test", 0, e.Message);
            }

            // Test 3: Invalid object references
            try {
                var invalidObject = $"NonExistent_{random.Next(1000)}";
                await ExecuteSqlCommandAsync(logs, connection, $"SELECT * FROM {invalidObject};", "Expected invalid object test", "Query");
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
                await ExecuteSqlCommandAsync(logs, connection, $"BEGIN TRANSACTION; SET LOCK_TIMEOUT 1000; UPDATE TOP (5) {randomTable1} WITH (UPDLOCK) SET Description = Description + ' - Updated' WHERE Category IN (SELECT TOP 1 Category FROM {randomTable2} WITH (UPDLOCK) ORDER BY NEWID()); COMMIT TRANSACTION;", "Lock timeout test", "Query");
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
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT a.*, b.* FROM {table1} a, {table2} b WHERE a.Number > b.Number ORDER BY a.Number;", $"Expensive cross join between {table1} and {table2}", "Query");

            // Test 2: Nested loop with large result set
            var table3 = TestConfiguration.GetRandomTableName(random);
            var table4 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT a.Number, a.Category, (SELECT COUNT(*) FROM {table4} b WHERE b.Number > a.Number) as LargerNumbers FROM {table3} a;", $"Nested loop query between {table3} and {table4}", "Query");

            // Test 3: Multiple subqueries causing high CPU
            var table5 = TestConfiguration.GetRandomTableName(random);
            var table6 = TestConfiguration.GetRandomTableName(random);
            var table7 = TestConfiguration.GetRandomTableName(random);
            var table8 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT Number, Category, (SELECT AVG(CAST(Number as FLOAT)) FROM {table6} b WHERE b.Category = a.Category) as CategoryAvg, (SELECT MAX(Number) FROM {table7} c WHERE c.Status = a.Status) as StatusMax, (SELECT COUNT(*) FROM {table8} d WHERE d.Number BETWEEN a.Number - 1000 AND a.Number + 1000) as NumberRange FROM {table5} a WHERE Number > 500;", $"Multiple subqueries across {table5}, {table6}, {table7}, and {table8}", "Query");

            // Test 4: Non-indexed column search with large table scan
            var table9 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT * FROM {table9} WHERE CAST(Number AS VARCHAR) LIKE '5%' ORDER BY Description;", $"Non-indexed search on {table9}", "Query");

            // Test 5: Large result set with string operations
            var table10 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT Number, UPPER(Category) as UpperCategory, LOWER(Status) as LowerStatus, SUBSTRING(Description, 1, 50) as TruncDesc, REPLICATE(Category, 3) as RepeatedCategory FROM {table10} WHERE LEN(Description) > 10;", $"String operations on {table10}", "Query");

            // Test 6: Lock-inducing update with select
            var table11 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"BEGIN TRANSACTION; UPDATE {table11} SET Description = Description + ' - Updated' WHERE Number BETWEEN 1 AND 100; SELECT * FROM {table11} WITH (HOLDLOCK) WHERE Number BETWEEN 1 AND 200; COMMIT TRANSACTION;", $"Lock-inducing update and select on {table11}", "Query");

            // Test 7: Parallel query with sort
            var table12 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"SELECT Category, Status, COUNT(*) as Count, AVG(CAST(Number as FLOAT)) as AvgNumber, STRING_AGG(CAST(Number as VARCHAR), ',') as Numbers FROM {table12} GROUP BY Category, Status ORDER BY COUNT(*) DESC OPTION (MAXDOP 4);", $"Parallel query with sort on {table12}", "Query");

            // Test 8: Memory-intensive operation
            var table13 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"WITH NumberSequence AS (SELECT Number, Category, Status, ROW_NUMBER() OVER (ORDER BY Number) as RowNum FROM {table13}) SELECT a.Number, a.Category, COUNT(*) OVER (PARTITION BY a.Category) as CategoryCount, AVG(CAST(a.Number as FLOAT)) OVER (PARTITION BY a.Status) as StatusAvg, MAX(a.Number) OVER (ORDER BY a.Number ROWS BETWEEN 1000 PRECEDING AND 1000 FOLLOWING) as MovingMax FROM NumberSequence a ORDER BY a.RowNum;", $"Memory-intensive window functions on {table13}", "Query");

            // Test 9: Blocking insert with select
            var table14 = TestConfiguration.GetRandomTableName(random);
            var table15 = TestConfiguration.GetRandomTableName(random);
            await ExecuteSqlCommandAsync(logs, connection, $"BEGIN TRANSACTION; INSERT INTO {table14} (Number, Description, Category, Status) SELECT Number + 1000000, 'Copied Record - ' + CAST(Number as VARCHAR), Category, Status FROM {table15} WHERE Number BETWEEN 1 AND 1000; SELECT COUNT(*) FROM {table14} WITH (TABLOCKX) WHERE Number > 1000000; COMMIT TRANSACTION;", $"Blocking insert with select between {table14} and {table15}", "Query");
        }

        // SP-based test scenario methods
        private static async Task RunFastQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_FastQueries;", "Execute usp_FastQueries", "StoredProcedure");
        }
        private static async Task RunSlowQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_SlowQueries;", "Execute usp_SlowQueries", "StoredProcedure");
        }
        private static async Task RunParallelQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_ParallelQueries;", "Execute usp_ParallelQueries", "StoredProcedure");
        }
        private static async Task RunTempTableQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_TempTableQueries;", "Execute usp_TempTableQueries", "StoredProcedure");
        }
        private static async Task RunIsolationLevelTestsSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_IsolationLevelTests;", "Execute usp_IsolationLevelTests", "StoredProcedure");
        }
        private static async Task RunDeadlockScenariosSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_DeadlockScenarios;", "Execute usp_DeadlockScenarios", "StoredProcedure");
        }
        private static async Task RunFailedQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_FailedQueries;", "Execute usp_FailedQueries", "StoredProcedure");
        }
        private static async Task RunProblematicQueriesSP(SqlConnection connection, List<LogEntry> logs) {
            await ExecuteSqlCommandAsync(logs, connection, "EXEC usp_ProblematicQueries;", "Execute usp_ProblematicQueries", "StoredProcedure");
        }

        private static async Task CleanupDatabase(SqlConnection connection, List<LogEntry> logs) {
            // Do nothing: persist database and stored procedures between runs
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
