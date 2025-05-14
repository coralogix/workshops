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
    /// <summary>
    /// Configuration constants for SQL Server test environment.
    /// </summary>
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
    /// Main program for synthetic SQL Server workload generator.
    /// This application:
    /// - Drops all user databases at startup (except system DBs)
    /// - Creates a test database, tables, and stored procedures if missing
    /// - Populates test data once before the main loop
    /// - Defines a set of read-only queries that simulate locks, errors, and slow queries
    /// - For each query, creates a stored procedure version
    /// - In the main loop, runs each query both as a direct query and as a stored procedure
    /// - Logs all actions and errors
    /// </summary>
    class Program {
        /// <summary>
        /// List of read-only test queries and their corresponding stored procedure names and descriptions.
        /// </summary>
        private static readonly (string ProcName, string Query, string Description)[] ReadOnlyTestQueries = new[]
        {
            ("usp_ReadOnlyTest_ExpensiveJoin", "SELECT TOP 100 a.*, b.* FROM DataRecords_01 a CROSS JOIN DataRecords_02 b WHERE a.Number > b.Number;", "Expensive cross join between DataRecords_01 and DataRecords_02"),
            ("usp_ReadOnlyTest_WindowFunction", "SELECT TOP 100 Number, AVG(Number) OVER (ORDER BY Number ROWS BETWEEN 1000 PRECEDING AND 1000 FOLLOWING) as MovingAvg FROM DataRecords_03;", "Window function on DataRecords_03"),
            ("usp_ReadOnlyTest_LockEscalation", "SELECT * FROM DataRecords_04 WITH (TABLOCKX, HOLDLOCK);", "Lock escalation on DataRecords_04"),
            ("usp_ReadOnlyTest_DivideByZero", "SELECT 1/0 AS WillFail FROM DataRecords_05;", "Divide by zero error on DataRecords_05"),
            ("usp_ReadOnlyTest_InvalidCast", "SELECT CAST('abc' AS INT) FROM DataRecords_06;", "Invalid cast error on DataRecords_06"),
            ("usp_ReadOnlyTest_NonExistentColumn", "SELECT NotAColumn FROM DataRecords_07;", "Non-existent column error on DataRecords_07"),
            ("usp_ReadOnlyTest_SlowScan", "SELECT * FROM DataRecords_08 WHERE Description LIKE '%slow%';", "Slow scan on DataRecords_08"),
            ("usp_ReadOnlyTest_BlockingSelect", "SELECT * FROM DataRecords_09 WITH (HOLDLOCK); WAITFOR DELAY '00:00:05';", "Blocking select on DataRecords_09"),
            ("usp_ReadOnlyTest_ParallelRead1", "SELECT TOP 10 * FROM DataRecords_01;", "Parallel read DataRecords_01"),
            ("usp_ReadOnlyTest_ParallelRead2", "SELECT TOP 10 * FROM DataRecords_02;", "Parallel read DataRecords_02"),
            ("usp_ReadOnlyTest_ParallelRead3", "SELECT TOP 10 * FROM DataRecords_03;", "Parallel read DataRecords_03")
        };

        /// <summary>
        /// Entry point for the application. Handles setup and main test loop.
        /// </summary>
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args) {
            // Parse optional duration argument
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

            // Drop all user databases at startup for a clean environment
            Console.WriteLine("\nDropping all user databases (except system DBs)...");
            await DropAllUserDatabases(connectionString, logs);
            Console.WriteLine("All user databases dropped. Proceeding with setup.\n");

            // Setup graceful shutdown on Ctrl+C
            using var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true; // Prevent immediate termination
                Console.WriteLine("\nGracefully shutting down...");
                cancellationTokenSource.Cancel();
            };
            var token = cancellationTokenSource.Token;

            try {
                // Wait for SQL Server to be ready
                Console.WriteLine("Connecting to SQL Server...");
                await WaitForSqlServer(connectionString, logs);

                // Setup database, tables, and stored procedures
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogAction(logs, "INFO", "Connection", "SQL Server connection established");

                    // Create database, tables, and core stored procedures if missing
                    Console.WriteLine("\nSetting up test database...");
                    try {
                        await SetupDatabase(connection, logs);
                    } catch (Exception e) {
                        Console.WriteLine($"\nWARNING: Database setup failed: {e.Message}");
                        Console.WriteLine("Will attempt to continue with existing database...");
                        await TryUseExistingDatabase(connection, logs);
                    }

                    // Switch to TestDB and populate test data ONCE
                    connection.ChangeDatabase("TestDB");
                    await PopulateTestData(connection, logs);

                    // Create all read-only test stored procedures
                    await SetupReadOnlyTestProcedures(connection, logs);
                }

                // Main test loop: run each query as direct SQL and as a stored procedure
                int cycle = 1;
                while (!token.IsCancellationRequested && (!endTime.HasValue || DateTime.UtcNow < endTime.Value)) {
                    Console.WriteLine($"\n=== Starting New Test Cycle #{cycle} at {DateTime.Now} ===\n");
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        connection.ChangeDatabase("TestDB");
                        var tasks = new List<Task>();
                        foreach (var (procName, query, description) in ReadOnlyTestQueries)
                        {
                            // Run as direct query
                            tasks.Add(Task.Run(async () => {
                                try {
                                    await ExecuteSqlCommandAsync(logs, connection, query, $"Direct: {description}", "Query");
                                } catch (Exception ex) {
                                    LogAction(logs, "EXPECTED_ERROR", "Query", $"Direct error: {description}", 0, ex.Message);
                                }
                            }, token));
                            // Run as stored procedure
                            tasks.Add(Task.Run(async () => {
                                try {
                                    await ExecuteSqlCommandAsync(logs, connection, $"EXEC {procName};", $"StoredProc: {description}", "StoredProcedure");
                                } catch (Exception ex) {
                                    LogAction(logs, "EXPECTED_ERROR", "StoredProcedure", $"StoredProc error: {description}", 0, ex.Message);
                                }
                            }, token));
                        }
                        await Task.WhenAll(tasks);
                    }
                    // Wait before next cycle
                    if (!token.IsCancellationRequested && (!endTime.HasValue || DateTime.UtcNow < endTime.Value)) {
                        Console.WriteLine("\nWaiting 5 seconds before starting next cycle...");
                        await Task.Delay(5000, token);
                    }
                    cycle++;
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
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Drops all user databases except system databases (master, model, msdb, tempdb).
        /// </summary>
        private static async Task DropAllUserDatabases(string connectionString, List<LogEntry> logs)
        {
            var systemDbs = new HashSet<string> { "master", "model", "msdb", "tempdb" };
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var dbNames = new List<string>();
            using (var cmd = new SqlCommand("SELECT name FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb')", connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dbNames.Add(reader.GetString(0));
                }
            }
            foreach (var dbName in dbNames)
            {
                try
                {
                    var dropCmd = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}];";
                    using var cmd = new SqlCommand(dropCmd, connection);
                    await cmd.ExecuteNonQueryAsync();
                    LogAction(logs, "INFO", "Cleanup", $"Dropped database '{dbName}'");
                }
                catch (Exception ex)
                {
                    LogAction(logs, "WARN", "Cleanup", $"Failed to drop database '{dbName}'", 0, ex.Message);
                }
            }
        }

        /// <summary>
        /// Populates all test tables with synthetic data. Truncates tables before inserting.
        /// </summary>
        private static async Task PopulateTestData(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            int recordsPerTable = TestConfiguration.TOTAL_RECORDS / TestConfiguration.NUMBER_OF_TABLES;
            foreach (var tableName in TestConfiguration.GetAllTableNames())
            {
                // Truncate table before inserting
                await ExecuteSqlCommandAsync(logs, connection, $"TRUNCATE TABLE {tableName};", $"Truncate {tableName}", "Database");
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = $"INSERT INTO {tableName} (Number, Description, Category, Status) VALUES (@num, @desc, @cat, @status)";
                    var numParam = cmd.Parameters.Add("@num", System.Data.SqlDbType.Int);
                    var descParam = cmd.Parameters.Add("@desc", System.Data.SqlDbType.NVarChar, 100);
                    var catParam = cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar, 50);
                    var statusParam = cmd.Parameters.Add("@status", System.Data.SqlDbType.NVarChar, 20);
                    for (int i = 0; i < recordsPerTable; i++)
                    {
                        numParam.Value = random.Next(1, 1000000);
                        descParam.Value = $"Test record {i} in {tableName}";
                        catParam.Value = TestConfiguration.Categories[random.Next(TestConfiguration.Categories.Length)];
                        statusParam.Value = TestConfiguration.StatusCodes[random.Next(TestConfiguration.StatusCodes.Length)];
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                LogAction(logs, "INFO", "Database", $"Populated {recordsPerTable} records in {tableName}");
            }
        }

        /// <summary>
        /// Creates or updates all read-only test stored procedures for the test queries.
        /// </summary>
        private static async Task SetupReadOnlyTestProcedures(SqlConnection connection, List<LogEntry> logs)
        {
            foreach (var (procName, query, description) in ReadOnlyTestQueries)
            {
                string procSql = $"CREATE OR ALTER PROCEDURE {procName} AS BEGIN SET NOCOUNT ON; {query} END;";
                try
                {
                    await ExecuteSqlCommandAsync(logs, connection, procSql.Replace("\n", " ").Replace("\r", " "), $"Create {procName}", "Database");
                }
                catch (Exception ex)
                {
                    LogAction(logs, "WARN", "Database", $"Failed to create {procName}: {ex.Message}");
                    // Do NOT rethrow, so the loop continues
                }
            }
        }

        /// <summary>
        /// Executes a SQL command asynchronously and logs the result.
        /// </summary>
        private static async Task ExecuteSqlCommandAsync(List<LogEntry> logs, SqlConnection connection, string sql, string operation, string category, SqlTransaction? transaction = null)
        {
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

        /// <summary>
        /// Logs an action to the logs list and prints to console as a single-line summary.
        /// </summary>
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

            // Print a single-line summary to console
            string summary = $"[{log.Timestamp:O}] {severity} | {category} | {operation}";
            if (duration > 0) summary += $" | {duration}ms";
            if (!string.IsNullOrEmpty(result)) summary += $" | {result}";
            Console.WriteLine(summary);
        }

        /// <summary>
        /// Waits for SQL Server to be available, retrying if necessary.
        /// </summary>
        private static async Task WaitForSqlServer(string connectionString, List<LogEntry> logs) {
            int maxRetries = 10;
            int retryDelaySeconds = 5;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

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
                        Console.WriteLine($"\nConnection successful!");
                        LogAction(logs, "INFO", "Connection", "SQL Server ready", stopwatch.ElapsedMilliseconds);
                        return;
                    }
                } catch (SqlException e) {
                    Console.WriteLine($"\nAttempt {i} failed with SQL error(s):");
                    foreach (SqlError error in e.Errors) {
                        Console.WriteLine($"  Message: {error.Message}");
                    }
                    if (i == maxRetries) {
                        stopwatch.Stop();
                        LogAction(logs, "ERROR", "Connection", $"Failed to connect after {maxRetries} attempts. Connection string: {sanitizedConnectionString}. Last errors: {e.Message}", stopwatch.ElapsedMilliseconds);
                        throw;
                    }
                    var waitTime = TimeSpan.FromSeconds(retryDelaySeconds);
                    Console.WriteLine($"Waiting {waitTime.TotalSeconds} seconds before next attempt...");
                    await Task.Delay(waitTime);
                } catch (Exception e) {
                    Console.WriteLine($"\nAttempt {i} failed with unexpected error: {e.Message}");
                    if (i == maxRetries) {
                        stopwatch.Stop();
                        LogAction(logs, "ERROR", "Connection", $"Failed to connect after {maxRetries} attempts. Connection string: {sanitizedConnectionString}. Last error: {e.Message}", stopwatch.ElapsedMilliseconds);
                        throw;
                    }
                    var waitTime = TimeSpan.FromSeconds(retryDelaySeconds);
                    Console.WriteLine($"Waiting {waitTime.TotalSeconds} seconds before next attempt...");
                    await Task.Delay(waitTime);
                }
            }
        }

        /// <summary>
        /// Sets up the test database and tables if they do not exist. Also creates core stored procedures.
        /// </summary>
        private static async Task SetupDatabase(SqlConnection connection, List<LogEntry> logs) {
            // Only create database if it does not exist
            await ExecuteSqlCommandAsync(logs, connection, "IF DB_ID('TestDB') IS NULL CREATE DATABASE TestDB;", "Setup database", "Database");
            connection.ChangeDatabase("TestDB");
            // Create tables if they do not exist
            foreach (var tableName in TestConfiguration.GetAllTableNames()) {
                await ExecuteSqlCommandAsync(logs, connection, $"IF OBJECT_ID('{tableName}', 'U') IS NULL BEGIN CREATE TABLE {tableName} (ID BIGINT PRIMARY KEY IDENTITY(1,1), Number INT, Description NVARCHAR(100), Category NVARCHAR(50), Status NVARCHAR(20), CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()); CREATE INDEX IX_{tableName}_Number ON {tableName}(Number); CREATE INDEX IX_{tableName}_Category ON {tableName}(Category); CREATE INDEX IX_{tableName}_Status ON {tableName}(Status); END", $"Create schema for {tableName}", "Database");
            }
            // Create or alter core stored procedures (no drops)
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
                await ExecuteSqlCommandAsync(logs, connection, procSql.Replace("\n", " ").Replace("\r", " "), "Create stored procedure", "Database");
            }
        }

        /// <summary>
        /// Attempts to switch to TestDB, creating it if necessary.
        /// </summary>
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

        /// <summary>
        /// Displays a summary of all logged actions and errors.
        /// </summary>
        private static void DisplaySummary(List<LogEntry> logs, long totalDuration) {
            Console.WriteLine("\n=== Test Execution Summary ===");
            Console.WriteLine($"Total Duration: {totalDuration/1000.0:F2} seconds");
            var categories = logs.GroupBy(l => l.Category).OrderBy(g => g.Key);
            foreach (var category in categories) {
                var totalCategoryDuration = category.Sum(l => l.Duration);
                var errorCount = category.Count(l => l.Severity == "ERROR");
                var successCount = category.Count(l => l.Severity == "INFO");
                Console.WriteLine($"\n{category.Key}:");
                Console.WriteLine($"  Duration: {totalCategoryDuration/1000.0:F2} seconds");
                Console.WriteLine($"  Successful Operations: {successCount}");
                if (errorCount > 0) {
                    Console.WriteLine($"  Failed Operations: {errorCount}");
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
    }

    /// <summary>
    /// Represents a single log entry for an action or error.
    /// </summary>
    class LogEntry {
        public DateTime Timestamp { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public long Duration { get; set; }
        public string? Result { get; set; }
    }
}
