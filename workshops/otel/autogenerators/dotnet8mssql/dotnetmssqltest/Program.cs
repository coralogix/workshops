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

namespace SqlRandomIntegersApp
{
    public class TestConfiguration
    {
        // Database Configuration
        public const string SQL_SERVER = "localhost";
        public const string SQL_PORT = "1433";
        public const string SQL_USER = "sa";
        public const string SQL_PASSWORD = "Toortoor9#";
        public const string SQL_DATABASE = "master";
        public const int CONNECTION_TIMEOUT = 30;
        public const int COMMAND_TIMEOUT = 120;
        public const int MAX_SQL_PARAMETERS = 2100;  // SQL Server limit

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
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = TestConfiguration.GetConnectionString();
            var logs = new List<LogEntry>();
            var totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            Console.WriteLine($"\n=== Starting SQL Server Performance Tests (Will run {TestConfiguration.MAX_ITERATIONS} iterations) ===\n");

            using var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            SqlConnection? connection = null;
            try
            {
                // Wait for SQL Server to be ready
                Console.WriteLine("Connecting to SQL Server...");
                await WaitForSqlServer(connectionString, logs);

                connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                LogAction(logs, "INFO", "Connection", "SQL Server connection established");

                // Initial setup
                Console.WriteLine("\nSetting up test database...");
                try
                {
                    await SetupDatabase(connection, logs);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nWARNING: Database setup failed: {e.Message}");
                    Console.WriteLine("Will attempt to continue with existing database...");
                    
                    await TryUseExistingDatabase(connection, logs);
                }

                // Run tests for specified number of iterations
                int iteration = 1;
                while (!token.IsCancellationRequested && iteration <= TestConfiguration.MAX_ITERATIONS)
                {
                    try
                    {
                        await RunTestIteration(connection, logs, iteration);
                        iteration++;
                        
                        if (iteration <= TestConfiguration.MAX_ITERATIONS)
                        {
                            Console.WriteLine($"\nWaiting 2 seconds before starting iteration {iteration}...");
                            await Task.Delay(2000, token);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\nError in iteration {iteration}: {e.Message}");
                        LogAction(logs, "ERROR", "Iteration", $"Failed in iteration {iteration}", 0, e.Message);
                        
                        if (iteration <= TestConfiguration.MAX_ITERATIONS)
                        {
                            Console.WriteLine($"\nWaiting 5 seconds before retrying iteration {iteration}...");
                            await Task.Delay(5000, token);
                        }
                    }
                }

                Console.WriteLine($"\n=== Completed all {TestConfiguration.MAX_ITERATIONS} iterations ===\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nERROR: {e.Message}");
                LogAction(logs, "ERROR", "Global Error", e.Message);
            }
            finally
            {
                await CleanupAndGenerateReport(connection, logs, totalStopwatch.ElapsedMilliseconds);
            }
        }

        private static async Task TryUseExistingDatabase(SqlConnection connection, List<LogEntry> logs)
        {
            try
            {
                connection.ChangeDatabase("TestDB");
            }
            catch
            {
                await ExecuteSqlCommandAsync(logs, connection,
                    "IF DB_ID('TestDB') IS NULL CREATE DATABASE TestDB;",
                    "Create database simple", "Database");
                connection.ChangeDatabase("TestDB");
            }
        }

        private static async Task RunTestIteration(SqlConnection connection, List<LogEntry> logs, int iteration)
        {
            Console.WriteLine($"\n=== Starting Iteration {iteration} ===");
            
            // Define test scenarios with consistent delays between each
            var testScenarios = new List<(string name, Func<SqlConnection, List<LogEntry>, Task> action)>
            {
                ("Fast queries", RunFastQueries),
                ("Slow queries", RunSlowQueries),
                ("Parallel queries", RunParallelQueries),
                ("Temp table queries", RunTempTableQueries),
                ("Isolation level tests", RunIsolationLevelTests),
                ("Deadlock scenarios", RunDeadlockScenarios),
                ("Failed queries", RunFailedQueries),
                ("Problematic queries", RunProblematicQueries)
            };

            foreach (var (name, action) in testScenarios)
            {
                try
                {
                    Console.WriteLine($"\nRunning {name}...");
                    await action(connection, logs);
                    // Add a small delay between test scenarios
                    await Task.Delay(500);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in {name}: {e.Message}");
                    LogAction(logs, "ERROR", name, $"Failed in {name}", 0, e.Message);
                    // Continue with next scenario even if one fails
                }
            }

            Console.WriteLine($"\nCompleted Iteration {iteration}");
        }

        private static async Task CleanupAndGenerateReport(SqlConnection? connection, List<LogEntry> logs, long totalDuration)
        {
            if (connection != null)
            {
                try
                {
                    await CleanupDatabase(connection, logs);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nWARNING: Cleanup failed: {e.Message}");
                    LogAction(logs, "ERROR", "Cleanup", "Failed to cleanup database", 0, e.Message);
                }
                finally
                {
                    connection.Dispose();
                }
            }

            // Generate and display summary
            DisplaySummary(logs, totalDuration);
            
            // Write summary to file
            await WriteSummaryToFile(logs, totalDuration);
        }

        private static async Task WriteSummaryToFile(List<LogEntry> logs, long totalDuration)
        {
            var summaryJson = JsonConvert.SerializeObject(
                GenerateSummaryObject(logs, totalDuration), 
                Formatting.Indented);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var summaryFile = $"test_summary_{timestamp}.json";
            await File.WriteAllTextAsync(summaryFile, summaryJson);
            Console.WriteLine($"\nDetailed results have been written to: {summaryFile}");
        }

        private static void DisplaySummary(List<LogEntry> logs, long totalDuration)
        {
            Console.WriteLine("\n=== Test Execution Summary ===");
            Console.WriteLine($"Total Duration: {totalDuration/1000.0:F2} seconds");
            
            var categories = logs
                .GroupBy(l => l.Category)
                .OrderBy(g => g.Key);

            foreach (var category in categories)
            {
                var totalCategoryDuration = category.Sum(l => l.Duration);
                var errorCount = category.Count(l => l.Severity == "ERROR");
                var successCount = category.Count(l => l.Severity == "INFO");
                
                Console.WriteLine($"\n{category.Key}:");
                Console.WriteLine($"  Duration: {totalCategoryDuration/1000.0:F2} seconds");
                Console.WriteLine($"  Successful Operations: {successCount}");
                if (errorCount > 0)
                {
                    Console.WriteLine($"  Failed Operations: {errorCount}");
                    // Display error details
                    var errors = category.Where(l => l.Severity == "ERROR");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"    - {error.Operation}: {error.Result}");
                    }
                }
            }

            var totalErrors = logs.Count(l => l.Severity == "ERROR");
            var totalSuccess = logs.Count(l => l.Severity == "INFO");
            Console.WriteLine($"\nTotal Successful Operations: {totalSuccess}");
            Console.WriteLine($"Total Failed Operations: {totalErrors}");
        }

        private static object GenerateSummaryObject(List<LogEntry> logs, long totalDuration)
        {
            var iterations = logs
                .Where(l => l.Operation?.StartsWith("Iteration") == true)
                .Count();

            return new
            {
                TotalDurationSeconds = totalDuration / 1000.0,
                TotalIterations = iterations,
                TotalOperations = logs.Count,
                TotalErrors = logs.Count(l => l.Severity == "ERROR"),
                AverageOperationsPerIteration = iterations > 0 ? (double)logs.Count / iterations : 0,
                Categories = logs
                    .GroupBy(l => l.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Name = g.Key,
                        DurationSeconds = g.Sum(l => l.Duration) / 1000.0,
                        OperationCount = g.Count(),
                        ErrorCount = g.Count(l => l.Severity == "ERROR"),
                        Operations = g.Select(l => new
                        {
                            Operation = l.Operation,
                            DurationMs = l.Duration,
                            Status = l.Severity,
                            Error = l.Severity == "ERROR" ? l.Result : null
                        }).ToList()
                    })
                    .ToList()
            };
        }

        private static async Task WaitForSqlServer(string connectionString, List<LogEntry> logs)
        {
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

            for (int i = 1; i <= maxRetries; i++)
            {
                try
                {
                    Console.WriteLine($"Connection attempt {i}/{maxRetries}...");
                    using (var connection = new SqlConnection(connectionString))
                    {
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
                }
                catch (SqlException e)
                {
                    var errorDetails = new StringBuilder();
                    errorDetails.AppendLine($"\nAttempt {i} failed with SQL error(s):");
                    
                    // Log each error in the collection
                    for (int j = 0; j < e.Errors.Count; j++)
                    {
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

                    if (i == maxRetries)
                    {
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
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nAttempt {i} failed with unexpected error:");
                    Console.WriteLine($"Error Type: {e.GetType().Name}");
                    Console.WriteLine($"Message: {e.Message}");
                    if (e.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {e.InnerException.Message}");
                    }

                    if (i == maxRetries)
                    {
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

        private static async Task<(string version, string state)> GetServerInfo(SqlConnection connection)
        {
            using (var cmd = new SqlCommand("SELECT @@VERSION", connection))
            {
                var version = (await cmd.ExecuteScalarAsync())?.ToString() ?? "Unknown";
                return (version, connection.State.ToString());
            }
        }

        private static async Task CleanupExistingDatabases(SqlConnection connection, List<LogEntry> logs)
        {
            try
            {
                // Get list of databases that might be left over from previous runs
                var cmd = new SqlCommand(@"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name IN ('TestDB')", connection);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var databasesToDelete = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        databasesToDelete.Add(reader.GetString(0));
                    }
                    reader.Close();

                    foreach (var dbName in databasesToDelete)
                    {
                        try
                        {
                            await ExecuteSqlCommandAsync(logs, connection,
                                $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}];",
                                $"Drop existing database '{dbName}'", "Cleanup");
                        }
                        catch (Exception ex)
                        {
                            LogAction(logs, "WARN", "Cleanup", $"Failed to drop database '{dbName}'", 0, ex.Message);
                        }
                    }

                    if (databasesToDelete.Count > 0)
                    {
                        Console.WriteLine($"Cleaned up {databasesToDelete.Count} existing test database(s).");
                    }
                    else
                    {
                        Console.WriteLine("No existing test databases found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogAction(logs, "WARN", "Cleanup", "Database cleanup check failed", 0, ex.Message);
                Console.WriteLine("Warning: Could not perform initial database cleanup check.");
            }
        }

        private static async Task ExecuteSqlCommandAsync(List<LogEntry> logs, SqlConnection connection, string sql, string operation, string category, SqlTransaction? transaction = null)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = transaction;
                    }
                    command.CommandTimeout = TestConfiguration.COMMAND_TIMEOUT;
                    await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();
                    LogAction(logs, "INFO", category, operation, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                LogAction(logs, "ERROR", category, operation, stopwatch.ElapsedMilliseconds, e.Message);
                throw;
            }
        }

        private static void LogAction(List<LogEntry> logs, string severity, string category, string operation, long duration = 0, string? result = null)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Category = category,
                Operation = operation,
                Severity = severity,
                Duration = duration,
                Result = result
            };
            logs.Add(log);

            // Print progress to console
            if (severity == "ERROR")
            {
                Console.WriteLine($"  {operation} - Failed: {result}");
            }
            else if (duration > 0)
            {
                Console.WriteLine($"  {operation} - Completed in {duration/1000.0:F2}s");
            }
        }

        private static async Task SetupDatabase(SqlConnection connection, List<LogEntry> logs)
        {
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

            // Create schema
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE TABLE DataRecords (
                    ID BIGINT PRIMARY KEY IDENTITY(1,1),
                    Number INT,
                    Description NVARCHAR(100),
                    Category NVARCHAR(50),
                    Status NVARCHAR(20),
                    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE()
                );

                CREATE INDEX IX_DataRecords_Number ON DataRecords(Number);
                CREATE INDEX IX_DataRecords_Category ON DataRecords(Category);
                CREATE INDEX IX_DataRecords_Status ON DataRecords(Status);", 
                "Create schema", "Database");

            // Insert test data
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var random = new Random();

            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = @"
                    INSERT INTO DataRecords (Number, Description, Category, Status)
                    VALUES (@num, @desc, @cat, @status)";

                var numParam = cmd.Parameters.Add("@num", System.Data.SqlDbType.Int);
                var descParam = cmd.Parameters.Add("@desc", System.Data.SqlDbType.NVarChar, 100);
                var catParam = cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar, 50);
                var statusParam = cmd.Parameters.Add("@status", System.Data.SqlDbType.NVarChar, 20);

                for (int i = 0; i < TestConfiguration.TOTAL_RECORDS; i++)
                {
                    numParam.Value = random.Next(1, 1000000);
                    descParam.Value = $"Test record {i}";
                    catParam.Value = TestConfiguration.Categories[random.Next(TestConfiguration.Categories.Length)];
                    statusParam.Value = TestConfiguration.StatusCodes[random.Next(TestConfiguration.StatusCodes.Length)];
                    
                    await cmd.ExecuteNonQueryAsync();

                    if (i % TestConfiguration.LOG_BATCH_SIZE == 0)
                    {
                        LogAction(logs, "INFO", "Database", $"Inserted {i} records", stopwatch.ElapsedMilliseconds);
                    }
                }
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

        private static async Task RunFastQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            
            // Add small random delay
            await Task.Delay(random.Next(100, 300));
            
            // Test 1: Quick indexed lookups with randomization
            var range = TestConfiguration.NumberRanges[random.Next(TestConfiguration.NumberRanges.Length)];
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:01';
                SELECT TOP {TestConfiguration.QUERY_LIMIT} * FROM DataRecords 
                WHERE Number BETWEEN {range.min} AND {range.max} 
                AND Category = '{GetRandomWeighted(random, TestConfiguration.Categories)}'", 
                "Indexed range lookup", "Query");

            // Test 2: Category and status aggregation with index
            var status = GetRandomWeighted(random, TestConfiguration.StatusCodes);
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:01';
                SELECT Category, Status, COUNT(*) as Count, 
                MIN(Number) as MinNumber, MAX(Number) as MaxNumber
                FROM DataRecords 
                WHERE Status = '{status}'
                GROUP BY Category, Status
                HAVING COUNT(*) > 5", 
                "Category-status aggregation", "Query");

            // Test 3: Recent records lookup
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:01';
                SELECT TOP {TestConfiguration.QUERY_LIMIT} ID, Number, Category, Status, CreatedAt
                FROM DataRecords
                WHERE Category = '{GetRandomWeighted(random, TestConfiguration.Categories)}'
                AND CreatedAt >= DATEADD(MINUTE, -5, GETUTCDATE())
                ORDER BY CreatedAt DESC", 
                "Recent records lookup", "Query");
        }

        private static async Task RunSlowQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            
            // Add medium random delay
            await Task.Delay(random.Next(300, 500));

            // Test 1: Complex aggregation with string operations
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:02';
                WITH DataStats AS (
                    SELECT TOP {TestConfiguration.QUERY_LIMIT}
                        Category,
                        Status,
                        COUNT(*) as RecordCount,
                        AVG(CAST(Number as FLOAT)) as AvgNumber,
                        STRING_AGG(CAST(ID as VARCHAR(20)), ',') as RecordIDs
                    FROM DataRecords
                    GROUP BY Category, Status
                )
                SELECT 
                    Category,
                    Status,
                    RecordCount,
                    AvgNumber,
                    COUNT(*) OVER (PARTITION BY Category) as CategoryTotal
                FROM DataStats
                WHERE RecordCount > 5
                ORDER BY CategoryTotal DESC, AvgNumber DESC;", 
                "Complex aggregation", "Query");

            // Test 2: Cross apply with string operations
            var searchCategory = GetRandomWeighted(random, TestConfiguration.Categories);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:02';
                SELECT TOP {TestConfiguration.QUERY_LIMIT}
                    dr1.Category,
                    dr1.Status,
                    Matches.MatchCount,
                    Matches.AvgNumber
                FROM DataRecords dr1
                CROSS APPLY (
                    SELECT 
                        COUNT(*) as MatchCount,
                        AVG(CAST(dr2.Number as FLOAT)) as AvgNumber
                    FROM DataRecords dr2
                    WHERE dr2.Category = dr1.Category
                    AND dr2.Status = dr1.Status
                    AND dr2.Number > dr1.Number
                ) Matches
                WHERE dr1.Category = '{searchCategory}'
                AND Matches.MatchCount > 0
                ORDER BY Matches.AvgNumber DESC;",
                "Cross apply with processing time", "Query");
        }

        private static async Task RunParallelQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            
            // Add random delay between parallel operations
            await Task.Delay(random.Next(500, 1500));

            // Test 1: Parallel full table scan with computation
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                SELECT 
                    Category,
                    COUNT(*) as TotalCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    MIN(Number) as MinNumber,
                    MAX(Number) as MaxNumber,
                    SUM(CASE WHEN Number % 2 = 0 THEN 1 ELSE 0 END) as EvenCount,
                    STRING_AGG(CAST(Number as VARCHAR(20)), ',') WITHIN GROUP (ORDER BY Number) as NumberList
                FROM DataRecords
                WHERE Number BETWEEN 1000 AND 900000
                GROUP BY Category
                OPTION (MAXDOP 4);", 
                "Complex parallel aggregation", "Query");

            // Add random delay between parallel operations
            await Task.Delay(random.Next(1000, 2000));

            // Test 2: Parallel join operations
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                WITH NumberRanges AS (
                    SELECT 
                        Number,
                        Category,
                        NTILE(100) OVER (ORDER BY Number) as Range
                    FROM DataRecords
                )
                SELECT 
                    r1.Range,
                    COUNT(*) as Combinations,
                    AVG(ABS(r1.Number - r2.Number)) as AvgDifference,
                    MAX(r1.Number) as MaxNumber,
                    MIN(r2.Number) as MinNumber
                FROM NumberRanges r1
                JOIN NumberRanges r2 ON 
                    r1.Range = r2.Range AND 
                    r1.Number <> r2.Number
                GROUP BY r1.Range
                OPTION (MAXDOP 4);",
                "Parallel join operations", "Query");

            // Test 3: Parallel data analysis with multiple operations
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                WITH ProcessingMetrics AS (
                    SELECT 
                        Category,
                        Status,
                        AVG(CAST(Number as FLOAT)) as AvgNumber,
                        COUNT(*) as RecordCount
                    FROM DataRecords
                    GROUP BY Category, Status
                )
                SELECT 
                    Category,
                    Status,
                    AvgNumber,
                    RecordCount,
                    CAST(RecordCount as FLOAT) / NULLIF(SUM(RecordCount) OVER (PARTITION BY Category), 0) as CategoryRatio,
                    RANK() OVER (PARTITION BY Category ORDER BY AvgNumber DESC) as ProcessingTimeRank
                FROM ProcessingMetrics
                WHERE RecordCount > 100
                ORDER BY Category, ProcessingTimeRank
                OPTION (MAXDOP 4);",
                "Parallel metrics analysis", "Query");
        }

        private static async Task RunTempTableQueries(SqlConnection connection, List<LogEntry> logs)
        {
            // Reset isolation level to default
            await ExecuteSqlCommandAsync(logs, connection,
                "SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Reset isolation level", "Configuration");

            // Test 1: Global temporary table with indexes
            await ExecuteSqlCommandAsync(logs, connection, @"
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
                FROM DataRecords 
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
            await ExecuteSqlCommandAsync(logs, connection, @"
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
                FROM DataRecords
                GROUP BY Category;

                SELECT * FROM @CategoryStats
                WHERE NumberCount > (SELECT AVG(NumberCount) FROM @CategoryStats);

                COMMIT TRANSACTION;",
                "Table variable operations", "Database");
        }

        private static async Task RunIsolationLevelTests(SqlConnection connection, List<LogEntry> logs)
        {
            // Reset to default isolation level before starting tests
            await ExecuteSqlCommandAsync(logs, connection,
                "SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Reset isolation level", "Configuration");

            // Test 1: Read Committed with nolock hint
            await ExecuteSqlCommandAsync(logs, connection, @"
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
                BEGIN TRANSACTION;
                
                SELECT TOP 50 
                    dr.Number,
                    dr.Category,
                    (
                        SELECT COUNT(*) 
                        FROM DataRecords WITH (NOLOCK) 
                        WHERE Number > dr.Number
                    ) as LargerNumbers
                FROM DataRecords dr
                ORDER BY dr.Number;
                
                COMMIT TRANSACTION;

                -- Reset isolation level
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Read Committed with NOLOCK", "Query");

            // Test 2: Snapshot isolation
            await ExecuteSqlCommandAsync(logs, connection, @"
                -- Enable snapshot isolation for TestDB
                ALTER DATABASE TestDB SET ALLOW_SNAPSHOT_ISOLATION ON;
                
                SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                BEGIN TRANSACTION;
                
                SELECT Category, COUNT(*) as NumberCount
                FROM DataRecords
                GROUP BY Category;
                
                -- Simulate some updates in a separate transaction
                UPDATE TOP (10) DataRecords
                SET Number = Number + 1
                WHERE Category = 'Large';
                
                -- Read again in the same transaction
                SELECT Category, COUNT(*) as NumberCount
                FROM DataRecords
                GROUP BY Category;
                
                COMMIT TRANSACTION;

                -- Reset isolation level
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;",
                "Snapshot isolation", "Query");
        }

        private static async Task RunDeadlockScenarios(SqlConnection connection, List<LogEntry> logs)
        {
            // Create additional table for deadlock testing
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE TABLE NumberCategories (
                    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
                    CategoryName NVARCHAR(50) UNIQUE,
                    LastUpdated DATETIME2 DEFAULT GETUTCDATE()
                );
                
                INSERT INTO NumberCategories (CategoryName)
                SELECT DISTINCT Category FROM DataRecords;",
                "Create deadlock test table", "Database");

            // Test 1: Simulate deadlock with update operations
            var stopwatch = new Stopwatch();
            try
            {
                // First transaction
                var task1 = Task.Run(async () =>
                {
                    using (var conn1 = new SqlConnection(connection.ConnectionString))
                    {
                        await conn1.OpenAsync();
                        using (var transaction = conn1.BeginTransaction())
                        {
                            await ExecuteSqlCommandAsync(logs, conn1, @"
                                UPDATE DataRecords 
                                SET Number = Number + 1
                                WHERE Category = 'Large';
                                
                                WAITFOR DELAY '00:00:02';
                                
                                UPDATE NumberCategories
                                SET LastUpdated = GETUTCDATE()
                                WHERE CategoryName = 'Large';",
                                "Transaction 1", "Query", transaction);
                            
                            transaction.Commit();
                        }
                    }
                });

                // Second transaction
                var task2 = Task.Run(async () =>
                {
                    using (var conn2 = new SqlConnection(connection.ConnectionString))
                    {
                        await conn2.OpenAsync();
                        using (var transaction = conn2.BeginTransaction())
                        {
                            await ExecuteSqlCommandAsync(logs, conn2, @"
                                UPDATE NumberCategories
                                SET LastUpdated = GETUTCDATE()
                                WHERE CategoryName = 'Large';
                                
                                WAITFOR DELAY '00:00:02';
                                
                                UPDATE DataRecords 
                                SET Number = Number + 2
                                WHERE Category = 'Large';",
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

                if (completedTask == timeoutTask)
                {
                    LogAction(logs, "INFO", "Query", "Deadlock Test - Timeout occurred", stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    await Task.WhenAll(task1, task2);
                }
            }
            catch (Exception e)
            {
                LogAction(logs, "INFO", "Query", "Deadlock Test", stopwatch.ElapsedMilliseconds, e.Message);
            }

            // Cleanup deadlock test objects
            await ExecuteSqlCommandAsync(logs, connection, 
                "DROP TABLE NumberCategories;",
                "Cleanup deadlock test", "Database");
        }

        private static async Task RunFailedQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();

            Console.WriteLine("\n=== Testing Error Handling Scenarios ===");

            // Test 1: Arithmetic and conversion errors
            try
            {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                await ExecuteSqlCommandAsync(logs, connection, 
                    $@"SELECT 
                        CASE WHEN '{errorType}' = 'OVERFLOW' THEN CAST(2147483647 + Number as INT)
                             WHEN '{errorType}' = 'CONVERSION' THEN CAST('invalid' as INT)
                             ELSE 1/0 
                        END as ErrorResult
                    FROM DataRecords 
                    WHERE ID = 1;", 
                    "Expected arithmetic error test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Arithmetic error handling test", 0, e.Message);
            }

            // Test 2: Constraint violations
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, @"
                    -- Attempting to violate identity constraint (expected to fail)
                    INSERT INTO DataRecords (ID, Number, Description, Category, Status)
                    SELECT TOP 1 ID, Number, Description, Category, Status 
                    FROM DataRecords;",
                    "Expected constraint violation test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Constraint violation handling test", 0, e.Message);
            }

            // Test 3: Invalid object references
            try
            {
                var invalidObject = $"NonExistent_{random.Next(1000)}";
                await ExecuteSqlCommandAsync(logs, connection, 
                    $"SELECT * FROM {invalidObject};",
                    "Expected invalid object test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Invalid object handling test", 0, e.Message);
            }

            // Test 4: Syntax errors with varying complexity
            try
            {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                var errorQuery = errorType switch
                {
                    "SYNTAX" => "SELEC * FORM DataRecords",
                    "JOIN" => "SELECT * FROM DataRecords INNER JOIN",
                    "GROUP" => "SELECT Category, COUNT(*) DataRecords GROUP Category",
                    _ => "SELECT * FROM DataRecords WHERE;"
                };
                await ExecuteSqlCommandAsync(logs, connection, errorQuery, "Expected syntax error test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Syntax error handling test", 0, e.Message);
            }

            // Test 5: Lock timeout simulation (this one might occasionally succeed)
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, @"
                    BEGIN TRANSACTION;
                    
                    -- Set a short lock timeout
                    SET LOCK_TIMEOUT 1000;
                    
                    -- Try to update records that might be locked
                    UPDATE TOP (5) DataRecords WITH (UPDLOCK)
                    SET Description = Description + ' - Updated'
                    WHERE Category IN (
                        SELECT TOP 1 Category 
                        FROM DataRecords WITH (UPDLOCK)
                        ORDER BY NEWID()
                    );
                    
                    COMMIT TRANSACTION;",
                    "Lock timeout test", "Query");
                LogAction(logs, "INFO", "Query", "Lock timeout test completed without errors", 0);
            }
            catch (Exception e)
            {
                LogAction(logs, "EXPECTED_ERROR", "Query", "Lock timeout handling test", 0, e.Message);
            }

            Console.WriteLine("=== Error Handling Tests Complete ===\n");
        }

        private static async Task RunProblematicQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();

            // Test 1: Cartesian product (cross join) - very expensive
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT a.*, b.* 
                FROM DataRecords a, DataRecords b 
                WHERE a.Number > b.Number
                ORDER BY a.Number;", 
                "Expensive cross join", "Query");

            // Test 2: Nested loop with large result set
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT a.Number, a.Category,
                    (SELECT COUNT(*) 
                     FROM DataRecords b 
                     WHERE b.Number > a.Number) as LargerNumbers
                FROM DataRecords a;",
                "Nested loop query", "Query");

            // Test 3: Multiple subqueries causing high CPU
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT 
                    Number,
                    Category,
                    (SELECT AVG(CAST(Number as FLOAT)) 
                     FROM DataRecords b 
                     WHERE b.Category = a.Category) as CategoryAvg,
                    (SELECT MAX(Number) 
                     FROM DataRecords c 
                     WHERE c.Status = a.Status) as StatusMax,
                    (SELECT COUNT(*) 
                     FROM DataRecords d 
                     WHERE d.Number BETWEEN a.Number - 1000 AND a.Number + 1000) as NumberRange
                FROM DataRecords a
                WHERE Number > 500;",
                "Multiple subqueries", "Query");

            // Test 4: Non-indexed column search with large table scan
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT * 
                FROM DataRecords 
                WHERE CAST(Number AS VARCHAR) LIKE '5%'
                ORDER BY Description;",
                "Non-indexed search", "Query");

            // Test 5: Large result set with string operations
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT 
                    Number,
                    UPPER(Category) as UpperCategory,
                    LOWER(Status) as LowerStatus,
                    SUBSTRING(Description, 1, 50) as TruncDesc,
                    REPLICATE(Category, 3) as RepeatedCategory
                FROM DataRecords
                WHERE LEN(Description) > 10;",
                "String operations", "Query");

            // Test 6: Lock-inducing update with select
            await ExecuteSqlCommandAsync(logs, connection, @"
                BEGIN TRANSACTION;
                
                UPDATE DataRecords 
                SET Description = Description + ' - Updated'
                WHERE Number BETWEEN 1 AND 100;

                SELECT * 
                FROM DataRecords WITH (HOLDLOCK)
                WHERE Number BETWEEN 1 AND 200;

                COMMIT TRANSACTION;",
                "Lock-inducing update and select", "Query");

            // Test 7: Parallel query with sort
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT 
                    Category,
                    Status,
                    COUNT(*) as Count,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    STRING_AGG(CAST(Number as VARCHAR), ',') as Numbers
                FROM DataRecords
                GROUP BY Category, Status
                ORDER BY COUNT(*) DESC
                OPTION (MAXDOP 4);",
                "Parallel query with sort", "Query");

            // Test 8: Memory-intensive operation
            await ExecuteSqlCommandAsync(logs, connection, @"
                WITH NumberSequence AS (
                    SELECT Number, Category, Status,
                           ROW_NUMBER() OVER (ORDER BY Number) as RowNum
                    FROM DataRecords
                )
                SELECT 
                    a.Number,
                    a.Category,
                    COUNT(*) OVER (PARTITION BY a.Category) as CategoryCount,
                    AVG(CAST(a.Number as FLOAT)) OVER (PARTITION BY a.Status) as StatusAvg,
                    MAX(a.Number) OVER (ORDER BY a.Number ROWS BETWEEN 1000 PRECEDING AND 1000 FOLLOWING) as MovingMax
                FROM NumberSequence a
                ORDER BY a.RowNum;",
                "Memory-intensive window functions", "Query");

            // Test 9: Blocking insert with select
            await ExecuteSqlCommandAsync(logs, connection, @"
                BEGIN TRANSACTION;
                
                INSERT INTO DataRecords (Number, Description, Category, Status)
                SELECT 
                    Number + 1000000,
                    'Copied Record - ' + CAST(Number as VARCHAR),
                    Category,
                    Status
                FROM DataRecords
                WHERE Number BETWEEN 1 AND 1000;

                -- This select will be blocked by the insert
                SELECT COUNT(*) 
                FROM DataRecords WITH (TABLOCKX)
                WHERE Number > 1000000;

                COMMIT TRANSACTION;",
                "Blocking insert with select", "Query");

            // Test 10: Resource-intensive batch insert with proper parameterization
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = connection;
                var sql = new StringBuilder("INSERT INTO DataRecords (Number, Description, Category, Status) VALUES ");
                var values = new List<string>();
                
                // Calculate max batch size based on parameters per record (4) and SQL Server limit
                int maxBatchSize = TestConfiguration.MAX_SQL_PARAMETERS / 4;
                int batchSize = Math.Min(500, maxBatchSize);  // Use smaller of 500 or max allowed
                
                for (int i = 0; i < batchSize; i++)
                {
                    string paramPrefix = $"p{i}_";
                    values.Add($"(@{paramPrefix}num, @{paramPrefix}desc, @{paramPrefix}cat, @{paramPrefix}status)");
                    
                    cmd.Parameters.AddWithValue($"@{paramPrefix}num", random.Next(1, 1000000));
                    cmd.Parameters.AddWithValue($"@{paramPrefix}desc", $"Batch Insert {i}");
                    cmd.Parameters.AddWithValue($"@{paramPrefix}cat", $"Category{i % 5}");
                    cmd.Parameters.AddWithValue($"@{paramPrefix}status", $"Status{i % 3}");
                }
                
                cmd.CommandText = sql.Append(string.Join(",", values)).ToString() + "; SELECT COUNT(*) FROM DataRecords;";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task CleanupDatabase(SqlConnection connection, List<LogEntry> logs)
        {
            connection.ChangeDatabase("master");
            await ExecuteSqlCommandAsync(logs, connection, 
                "ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB;",
                "Drop database", "Database");
        }

        // Helper method to get random test data with weighted probabilities
        private static T GetRandomWeighted<T>(Random random, T[] items, double errorProbability = 0.1)
        {
            if (random.NextDouble() < errorProbability)
            {
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

    class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public long Duration { get; set; }
        public string? Result { get; set; }
    }
}
