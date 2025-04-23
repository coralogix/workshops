using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace SqlRandomIntegersApp
{
    /// <summary>
    /// Enhanced SQL Server test application that demonstrates various query patterns and performance scenarios
    /// </summary>
    class Program
    {
        // Configuration
        private const int TOTAL_RECORDS = 1000000; // 1 million records for meaningful performance testing
        private const int BATCH_SIZE = 1000; // Insert records in batches for better performance
        private const int CONNECTION_TIMEOUT = 30; // Connection timeout in seconds
        private const int COMMAND_TIMEOUT = 120; // Command timeout in seconds
        
        // SQL Server Connection Configuration
        private const string SQL_SERVER = "localhost";
        private const string SQL_PORT = "1433";
        private const string SQL_USER = "sa";
        private const string SQL_PASSWORD = "Toortoor9#";

        // Test Data Configuration
        private static readonly string[] Categories = new[] { 
            "Small", "Medium", "Large", "Extra Large", "Critical", "Low Priority", "Archived", 
            "Batch", "Real-time", "Historical", "Analytical"  // Added more categories
        };
        private static readonly string[] StatusCodes = new[] { 
            "ACTIVE", "PENDING", "COMPLETED", "FAILED", "BLOCKED", "IN_PROGRESS",
            "RETRYING", "CANCELLED", "TIMEOUT", "PARTIAL"  // Added more status codes
        };
        private static readonly string[] DataTypes = new[] { 
            "JSON", "XML", "CSV", "BINARY", "TEXT", "ENCRYPTED",
            "COMPRESSED", "BASE64", "SERIALIZED", "HASHED"  // Added more data types
        };
        private static readonly string[] Priorities = new[] { 
            "P0", "P1", "P2", "P3", "P4",
            "URGENT", "NORMAL", "LOW", "BACKGROUND"  // Added more priorities
        };
        
        // Added more test variations
        private static readonly (int min, int max)[] NumberRanges = new[] {
            (1, 100), (100, 1000), (1000, 10000), (10000, 100000), (-1000, 1000)
        };
        
        private static readonly string[] ErrorTypes = new[] {
            "TIMEOUT", "DEADLOCK", "CONSTRAINT", "OVERFLOW", "CONVERSION",
            "PERMISSION", "MEMORY", "IO", "NETWORK", "VALIDATION"
        };

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

        // Helper method to generate random decimal with precision
        private static decimal GetRandomDecimal(Random random, int precision = 4)
        {
            double multiplier = Math.Pow(10, precision);
            return (decimal)(random.NextDouble() * multiplier);
        }

        static async Task Main(string[] args)
        {
            // Parse duration from command line arguments
            int durationMinutes = 1; // Default to 1 minute if no duration specified
            if (args.Length > 0 && int.TryParse(args[0], out int minutes))
            {
                durationMinutes = Math.Max(1, minutes); // Ensure at least 1 minute
            }

            string connectionString = $"Server={SQL_SERVER},{SQL_PORT};User Id={SQL_USER};Password={SQL_PASSWORD};Connection Timeout={CONNECTION_TIMEOUT};";
            var logs = new List<LogEntry>();
            var totalStopwatch = new Stopwatch();
            totalStopwatch.Start();

            Console.WriteLine($"\n=== Starting SQL Server Performance Tests (Running for {durationMinutes} minute{(durationMinutes > 1 ? "s" : "")}) ===\n");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(durationMinutes));
            var token = cancellationTokenSource.Token;

            try
            {
                // Wait for SQL Server to be ready
                Console.WriteLine("Connecting to SQL Server...");
                await WaitForSqlServer(connectionString, logs);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogAction(logs, "INFO", "Connection", "SQL Server connection established");

                    // Initial setup
                    Console.WriteLine("\nSetting up test database...");
                    await SetupDatabase(connection, logs);

                    // Run tests in a loop until the time expires
                    int iteration = 1;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            Console.WriteLine($"\n=== Starting Iteration {iteration} ===");
                            
                            Console.WriteLine("\nRunning fast queries...");
                            await RunFastQueries(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning slow queries...");
                            await RunSlowQueries(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning parallel queries...");
                            await RunParallelQueries(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning temp table queries...");
                            await RunTempTableQueries(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning isolation level tests...");
                            await RunIsolationLevelTests(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning deadlock scenarios...");
                            await RunDeadlockScenarios(connection, logs);

                            if (token.IsCancellationRequested) break;

                            Console.WriteLine("\nRunning failed queries...");
                            await RunFailedQueries(connection, logs);

                            iteration++;
                            Console.WriteLine($"\nCompleted Iteration {iteration - 1}");
                            
                            // Optional small delay between iterations
                            await Task.Delay(1000, token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"\nError in iteration {iteration}: {e.Message}");
                            LogAction(logs, "ERROR", "Iteration", $"Failed in iteration {iteration}", 0, e.Message);
                            // Continue with next iteration
                        }
                    }

                    Console.WriteLine("\nTime limit reached. Cleaning up...");
                    await CleanupDatabase(connection, logs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nERROR: {e.Message}");
                LogAction(logs, "ERROR", "Global Error", e.Message);
            }
            finally
            {
                totalStopwatch.Stop();
                
                // Generate and display summary
                DisplaySummary(logs, totalStopwatch.ElapsedMilliseconds);
                
                // Write summary to a file
                var summaryJson = JsonConvert.SerializeObject(
                    GenerateSummaryObject(logs, totalStopwatch.ElapsedMilliseconds), 
                    Formatting.Indented);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var summaryFile = $"test_summary_{timestamp}.json";
                await File.WriteAllTextAsync(summaryFile, summaryJson);
                Console.WriteLine($"\nDetailed results have been written to: {summaryFile}");
            }

            // Keep console window open if running as executable
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
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

            for (int i = 1; i <= maxRetries; i++)
            {
                try
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        stopwatch.Stop();
                        LogAction(logs, "INFO", "Connection", "SQL Server ready", stopwatch.ElapsedMilliseconds);

                        // Clean up any existing test databases
                        Console.WriteLine("\nChecking for and removing any existing test databases...");
                        await CleanupExistingDatabases(connection, logs);
                        return;
                    }
                }
                catch (SqlException)
                {
                    if (i == maxRetries)
                    {
                        stopwatch.Stop();
                        LogAction(logs, "ERROR", "Connection", 
                            $"Failed to connect after {maxRetries} attempts", 
                            stopwatch.ElapsedMilliseconds);
                        throw;
                    }
                    await Task.Delay(retryDelaySeconds * 1000);
                }
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
                    command.CommandTimeout = COMMAND_TIMEOUT;
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
            var stopwatch = new Stopwatch();

            // Drop existing database if it exists
            await ExecuteSqlCommandAsync(logs, connection, 
                "IF DB_ID('TestDB') IS NOT NULL BEGIN ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB; END", 
                "Drop existing database", "Database");

            // Create new database with specific configurations
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE DATABASE TestDB
                ON PRIMARY (
                    NAME = TestDB_Data,
                    SIZE = 1GB,
                    FILEGROWTH = 256MB
                )
                LOG ON (
                    NAME = TestDB_Log,
                    SIZE = 512MB,
                    FILEGROWTH = 128MB
                )", 
                "Create database", "Database");

            connection.ChangeDatabase("TestDB");

            // Create a rich schema with various data types and relationships
            await ExecuteSqlCommandAsync(logs, connection, @"
                -- Main data table with various data types
                CREATE TABLE DataRecords (
                    ID BIGINT PRIMARY KEY IDENTITY(1,1),
                    Number INT,
                    FloatValue FLOAT,
                    DecimalValue DECIMAL(18,6),
                    Description NVARCHAR(1000),
                    LongText NVARCHAR(MAX),
                    BinaryData VARBINARY(MAX),
                    CreatedAt DATETIME2(7) DEFAULT GETUTCDATE(),
                    UpdatedAt DATETIME2(7),
                    Category NVARCHAR(50),
                    Status NVARCHAR(20),
                    Priority NVARCHAR(10),
                    DataType NVARCHAR(20),
                    IsProcessed BIT DEFAULT 0,
                    ProcessingTime INT,
                    RetryCount TINYINT DEFAULT 0,
                    JsonData NVARCHAR(MAX),
                    XmlData XML,
                    ComputedHash AS HASHBYTES('SHA2_256', CONCAT(CAST(Number AS NVARCHAR(20)), Description)) PERSISTED,
                    Version ROWVERSION
                );

                -- Indexes for various query patterns
                CREATE INDEX IX_DataRecords_Number ON DataRecords(Number);
                CREATE INDEX IX_DataRecords_Category ON DataRecords(Category);
                CREATE INDEX IX_DataRecords_Status ON DataRecords(Status);
                CREATE INDEX IX_DataRecords_Priority ON DataRecords(Priority);
                CREATE INDEX IX_DataRecords_CreatedAt ON DataRecords(CreatedAt);
                CREATE INDEX IX_DataRecords_ComputedHash ON DataRecords(ComputedHash);

                -- Partitioned historical data
                CREATE PARTITION FUNCTION PF_CreatedAt (DATETIME2)
                AS RANGE RIGHT FOR VALUES (
                    '2023-01-01', '2023-04-01', '2023-07-01', '2023-10-01',
                    '2024-01-01', '2024-04-01', '2024-07-01', '2024-10-01'
                );

                CREATE PARTITION SCHEME PS_CreatedAt
                AS PARTITION PF_CreatedAt ALL TO ([PRIMARY]);

                CREATE TABLE HistoricalData (
                    ID BIGINT PRIMARY KEY IDENTITY(1,1),
                    RecordID BIGINT,
                    ChangeType NVARCHAR(20),
                    ChangedAt DATETIME2(7),
                    OldValue NVARCHAR(MAX),
                    NewValue NVARCHAR(MAX),
                    UserID NVARCHAR(50)
                ) ON PS_CreatedAt(ChangedAt);

                -- Table for handling locks and blocking scenarios
                CREATE TABLE ProcessingQueue (
                    ID BIGINT PRIMARY KEY IDENTITY(1,1),
                    RecordID BIGINT,
                    Status NVARCHAR(20),
                    LockVersion ROWVERSION,
                    LockedBy NVARCHAR(50),
                    LockExpiration DATETIME2(7),
                    RetryCount INT DEFAULT 0,
                    CONSTRAINT FK_Queue_DataRecords FOREIGN KEY (RecordID) 
                    REFERENCES DataRecords(ID)
                );

                -- Table for deadlock testing
                CREATE TABLE ResourceLocks (
                    ResourceID BIGINT PRIMARY KEY IDENTITY(1,1),
                    ResourceType NVARCHAR(50),
                    LockHolder NVARCHAR(50),
                    AcquiredAt DATETIME2(7),
                    Priority INT,
                    Status NVARCHAR(20)
                );

                -- Create stored procedures for complex operations
                CREATE PROCEDURE ProcessRecord
                    @RecordID BIGINT,
                    @ProcessorID NVARCHAR(50)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;
                    
                    BEGIN TRY
                        BEGIN TRANSACTION;
                        
                        DECLARE @LockExpiration DATETIME2(7) = DATEADD(MINUTE, 5, GETUTCDATE());
                        
                        UPDATE ProcessingQueue
                        SET Status = 'PROCESSING',
                            LockedBy = @ProcessorID,
                            LockExpiration = @LockExpiration
                        WHERE RecordID = @RecordID
                        AND (Status = 'PENDING' OR (Status = 'PROCESSING' AND LockExpiration < GETUTCDATE()));
                        
                        IF @@ROWCOUNT > 0
                        BEGIN
                            UPDATE DataRecords
                            SET IsProcessed = 1,
                                ProcessingTime = CAST(RAND() * 1000 AS INT),
                                UpdatedAt = GETUTCDATE()
                            WHERE ID = @RecordID;
                            
                            INSERT INTO HistoricalData (RecordID, ChangeType, ChangedAt, OldValue, NewValue, UserID)
                            VALUES (@RecordID, 'PROCESS', GETUTCDATE(), 'PENDING', 'PROCESSED', @ProcessorID);
                        END
                        
                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH
                END;

                -- Create functions for complex calculations
                CREATE FUNCTION CalculateProcessingMetrics (
                    @Category NVARCHAR(50),
                    @StartDate DATETIME2,
                    @EndDate DATETIME2
                )
                RETURNS TABLE
                AS
                RETURN (
                    SELECT 
                        Category,
                        COUNT(*) as TotalRecords,
                        AVG(CAST(ProcessingTime as FLOAT)) as AvgProcessingTime,
                        MIN(ProcessingTime) as MinProcessingTime,
                        MAX(ProcessingTime) as MaxProcessingTime,
                        SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) as ProcessedCount,
                        SUM(CASE WHEN RetryCount > 0 THEN 1 ELSE 0 END) as RetryCount
                    FROM DataRecords
                    WHERE Category = @Category
                    AND CreatedAt BETWEEN @StartDate AND @EndDate
                    GROUP BY Category
                );", 
                "Create schema", "Database");

            // Insert test data in batches
            stopwatch.Start();
            Random random = new Random();
            
            for (int batchStart = 0; batchStart < TOTAL_RECORDS; batchStart += BATCH_SIZE)
            {
                StringBuilder batchInsert = new StringBuilder(
                    "INSERT INTO DataRecords (Number, FloatValue, DecimalValue, Description, Category, Status, Priority, DataType, JsonData, XmlData, LongText, BinaryData) VALUES ");
                var values = new List<string>();

                for (int i = 0; i < BATCH_SIZE && (batchStart + i) < TOTAL_RECORDS; i++)
                {
                    int num = random.Next(1, 1000000);
                    double floatVal = random.NextDouble() * 1000;
                    decimal decimalVal = (decimal)(random.NextDouble() * 1000);
                    string category = Categories[random.Next(Categories.Length)];
                    string status = StatusCodes[random.Next(StatusCodes.Length)];
                    string priority = Priorities[random.Next(Priorities.Length)];
                    string dataType = DataTypes[random.Next(DataTypes.Length)];
                    
                    // Generate complex test data
                    var jsonData = new
                    {
                        id = num,
                        metrics = new { value = floatVal, unit = "ms" },
                        tags = new[] { category, status, priority },
                        timestamp = DateTime.UtcNow
                    };

                    var xmlData = $@"<record>
                        <id>{num}</id>
                        <metrics><value>{floatVal}</value><unit>ms</unit></metrics>
                        <tags><category>{category}</category><status>{status}</status></tags>
                    </record>";

                    string desc = $"Test record {num} in category {category} with status {status} and priority {priority}";
                    string longText = GenerateRandomText(random.Next(100, 1000));
                    string binaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(GenerateRandomText(50)));

                    values.Add($@"(
                        {num}, 
                        {floatVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                        {decimalVal.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
                        '{desc.Replace("'", "''")}', 
                        '{category}', 
                        '{status}', 
                        '{priority}',
                        '{dataType}',
                        '{JsonConvert.SerializeObject(jsonData).Replace("'", "''")}',
                        '{xmlData.Replace("'", "''")}',
                        '{longText.Replace("'", "''")}',
                        CAST('{binaryData}' AS VARBINARY(MAX))
                    )");
                }

                batchInsert.Append(string.Join(",", values));
                await ExecuteSqlCommandAsync(logs, connection, batchInsert.ToString(), 
                    $"Insert batch {batchStart/BATCH_SIZE + 1}", "Database");

                // Add some records to the processing queue
                if (random.NextDouble() < 0.3) // 30% chance for each batch
                {
                    await ExecuteSqlCommandAsync(logs, connection, $@"
                        INSERT INTO ProcessingQueue (RecordID, Status)
                        SELECT TOP {random.Next(1, 10)} ID, 'PENDING'
                        FROM DataRecords
                        WHERE ID > {batchStart} AND ID <= {batchStart + BATCH_SIZE}
                        AND ID NOT IN (SELECT RecordID FROM ProcessingQueue);",
                        "Queue records for processing", "Database");
                }

                // Add some resource locks for deadlock testing
                if (random.NextDouble() < 0.2) // 20% chance for each batch
                {
                    await ExecuteSqlCommandAsync(logs, connection, $@"
                        INSERT INTO ResourceLocks (ResourceType, LockHolder, AcquiredAt, Priority, Status)
                        VALUES 
                        ('RECORD', 'SYSTEM', GETUTCDATE(), {random.Next(1, 5)}, 'LOCKED');",
                        "Create resource lock", "Database");
                }
            }
            stopwatch.Stop();
            LogAction(logs, "INFO", "Database", "Data Generation", stopwatch.ElapsedMilliseconds);
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
            await Task.Delay(random.Next(100, 500));
            
            // Test 1: Quick indexed lookups with randomization
            var range = NumberRanges[random.Next(NumberRanges.Length)];
            var number = random.Next(range.min, range.max);
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:0{random.Next(1, 3)}';
                SELECT TOP 100 * FROM DataRecords 
                WHERE Number BETWEEN {range.min} AND {range.max} 
                AND Category = '{GetRandomWeighted(random, Categories)}'", 
                "Indexed range lookup", "Query");

            // Test 2: Category and status aggregation with index
            var status = GetRandomWeighted(random, StatusCodes);
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:0{random.Next(1, 3)}';
                SELECT Category, Status, COUNT(*) as Count, 
                MIN(Number) as MinNumber, MAX(Number) as MaxNumber
                FROM DataRecords 
                WHERE Status = '{status}'
                GROUP BY Category, Status
                HAVING COUNT(*) > 10", 
                "Category-status aggregation", "Query");

            // Test 3: Recent records lookup with composite index
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:0{random.Next(1, 3)}';
                SELECT TOP 1000 ID, Number, Category, Status, CreatedAt
                FROM DataRecords
                WHERE Category = '{GetRandomWeighted(random, Categories)}'
                AND CreatedAt >= DATEADD(MINUTE, -5, GETUTCDATE())
                ORDER BY CreatedAt DESC", 
                "Recent records lookup", "Query");

            // Test 4: Priority-based filtered count
            var priority = GetRandomWeighted(random, Priorities);
            await ExecuteSqlCommandAsync(logs, connection, 
                $@"WAITFOR DELAY '00:00:0{random.Next(1, 3)}';
                SELECT DataType, 
                COUNT(*) as TotalCount,
                SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) as ProcessedCount
                FROM DataRecords
                WHERE Priority = '{priority}'
                GROUP BY DataType", 
                "Priority-based counts", "Query");
        }

        private static async Task RunSlowQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            
            // Add medium random delay
            await Task.Delay(random.Next(500, 1500));

            // Test 1: Complex aggregation with string operations and JSON
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                WITH DataStats AS (
                    SELECT 
                        Category,
                        Status,
                        COUNT(*) as RecordCount,
                        AVG(CAST(Number as FLOAT)) as AvgNumber,
                        STRING_AGG(CAST(ID as VARCHAR(20)), ',') as RecordIDs,
                        JSON_VALUE(JsonData, '$.metrics.value') as MetricValue
                    FROM DataRecords
                    WHERE IsProcessed = 1
                    GROUP BY Category, Status, JSON_VALUE(JsonData, '$.metrics.value')
                )
                SELECT 
                    Category,
                    Status,
                    RecordCount,
                    AvgNumber,
                    CAST(MetricValue as FLOAT) as MetricValue,
                    COUNT(*) OVER (PARTITION BY Category) as CategoryTotal
                FROM DataStats
                WHERE RecordCount > 10
                ORDER BY CategoryTotal DESC, AvgNumber DESC;", 
                "Complex aggregation with JSON", "Query");

            // Test 2: Cross apply with string operations
            var searchCategory = GetRandomWeighted(random, Categories);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                SELECT TOP 1000 
                    dr1.Category,
                    dr1.Status,
                    dr1.Priority,
                    Matches.MatchCount,
                    Matches.AvgProcessingTime
                FROM DataRecords dr1
                CROSS APPLY (
                    SELECT 
                        COUNT(*) as MatchCount,
                        AVG(CAST(dr2.ProcessingTime as FLOAT)) as AvgProcessingTime
                    FROM DataRecords dr2
                    WHERE dr2.Category = dr1.Category
                    AND dr2.Status = dr1.Status
                    AND dr2.ProcessingTime > dr1.ProcessingTime
                ) Matches
                WHERE dr1.Category = '{searchCategory}'
                AND Matches.MatchCount > 0
                ORDER BY Matches.AvgProcessingTime DESC;",
                "Cross apply with processing time", "Query");

            // Add longer random delay between complex operations
            await Task.Delay(random.Next(1000, 2000));

            // Test 3: Window functions with partitioning
            var dataType = GetRandomWeighted(random, DataTypes);
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                WITH ProcessingStats AS (
                    SELECT 
                        ID,
                        Category,
                        Status,
                        ProcessingTime,
                        ROW_NUMBER() OVER (PARTITION BY Category ORDER BY ProcessingTime DESC) as RankInCategory,
                        PERCENT_RANK() OVER (PARTITION BY Status ORDER BY ProcessingTime) as PercentileInStatus,
                        LAG(ProcessingTime, 1, 0) OVER (PARTITION BY Category ORDER BY ProcessingTime) as PrevProcessingTime,
                        LEAD(ProcessingTime, 1, 0) OVER (PARTITION BY Category ORDER BY ProcessingTime) as NextProcessingTime
                    FROM DataRecords
                    WHERE DataType = '{dataType}'
                )
                SELECT *
                FROM ProcessingStats
                WHERE RankInCategory <= 100
                ORDER BY Category, ProcessingTime DESC;",
                "Window functions analysis", "Query");

            // Test 4: XML and hierarchical data
            await ExecuteSqlCommandAsync(logs, connection, $@"
                WAITFOR DELAY '00:00:0{random.Next(3, 6)}';
                WITH XMLDATA AS (
                    SELECT 
                        ID,
                        Category,
                        XmlData.value('(/record/metrics/value)[1]', 'float') as MetricValue,
                        XmlData.value('(/record/tags/category)[1]', 'nvarchar(50)') as XmlCategory,
                        XmlData.value('(/record/tags/status)[1]', 'nvarchar(50)') as XmlStatus
                    FROM DataRecords
                    WHERE DataType IN ('XML', 'SERIALIZED')
                )
                SELECT 
                    Category,
                    XmlCategory,
                    XmlStatus,
                    COUNT(*) as RecordCount,
                    AVG(MetricValue) as AvgMetricValue,
                    MIN(MetricValue) as MinMetricValue,
                    MAX(MetricValue) as MaxMetricValue
                FROM XMLDATA
                WHERE MetricValue IS NOT NULL
                GROUP BY Category, XmlCategory, XmlStatus
                HAVING COUNT(*) > 5
                ORDER BY AvgMetricValue DESC;",
                "XML data analysis", "Query");
        }

        private static async Task RunParallelQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var random = new Random();
            
            // Add random delay between parallel operations
            await Task.Delay(random.Next(500, 1500));

            // Enable parallel query execution
            await ExecuteSqlCommandAsync(logs, connection, 
                "EXEC sp_configure 'max degree of parallelism', 4;", 
                "Configure parallel execution", "Configuration");

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
                    WHERE ProcessingTime > 0
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
                        Priority,
                        AVG(CAST(ProcessingTime as FLOAT)) as AvgProcessingTime,
                        COUNT(*) as RecordCount,
                        SUM(CASE WHEN IsProcessed = 1 THEN 1 ELSE 0 END) as ProcessedCount
                    FROM DataRecords
                    GROUP BY Category, Status, Priority
                )
                SELECT 
                    Category,
                    Status,
                    Priority,
                    AvgProcessingTime,
                    RecordCount,
                    ProcessedCount,
                    CAST(ProcessedCount as FLOAT) / NULLIF(RecordCount, 0) as ProcessingRatio,
                    RANK() OVER (PARTITION BY Category ORDER BY AvgProcessingTime DESC) as ProcessingTimeRank
                FROM ProcessingMetrics
                WHERE RecordCount > 100
                ORDER BY Category, ProcessingTimeRank
                OPTION (MAXDOP 4);",
                "Parallel metrics analysis", "Query");
        }

        private static async Task RunTempTableQueries(SqlConnection connection, List<LogEntry> logs)
        {
            // Test 1: Global temporary table with indexes
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE TABLE ##GlobalTempNumbers (
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    Number INT,
                    Category NVARCHAR(50),
                    ProcessedAt DATETIME2 DEFAULT GETUTCDATE()
                );
                CREATE INDEX IX_GTN_Number ON ##GlobalTempNumbers(Number);
                
                INSERT INTO ##GlobalTempNumbers (Number, Category)
                SELECT TOP 100000 Number, Category 
                FROM Numbers 
                WHERE Category = 'Large';
                
                SELECT 
                    DATEPART(SECOND, ProcessedAt) as ProcessedSecond,
                    COUNT(*) as NumberCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber
                FROM ##GlobalTempNumbers
                GROUP BY DATEPART(SECOND, ProcessedAt);
                
                DROP TABLE ##GlobalTempNumbers;",
                "Global temp table operations", "Database");

            // Test 2: Table variables for intermediate results
            await ExecuteSqlCommandAsync(logs, connection, @"
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
                FROM Numbers
                GROUP BY Category;

                SELECT * FROM @CategoryStats
                WHERE NumberCount > (SELECT AVG(NumberCount) FROM @CategoryStats);",
                "Table variable operations", "Database");
        }

        private static async Task RunIsolationLevelTests(SqlConnection connection, List<LogEntry> logs)
        {
            // Test 1: Read Committed with nolock hint
            await ExecuteSqlCommandAsync(logs, connection, @"
                SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
                BEGIN TRANSACTION;
                
                SELECT TOP 1000 
                    n1.Number,
                    n1.Category,
                    (SELECT COUNT(*) FROM Numbers WITH (NOLOCK) n2 
                     WHERE n2.Number > n1.Number) as LargerNumbers
                FROM Numbers n1
                ORDER BY n1.Number;
                
                COMMIT TRANSACTION;",
                "Read Committed with NOLOCK", "Query");

            // Test 2: Snapshot isolation
            await ExecuteSqlCommandAsync(logs, connection, @"
                ALTER DATABASE TestDB SET ALLOW_SNAPSHOT_ISOLATION ON;
                
                SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                BEGIN TRANSACTION;
                
                SELECT Category, COUNT(*) as NumberCount
                FROM Numbers
                GROUP BY Category;
                
                -- Simulate some updates in a separate transaction
                UPDATE TOP (100) Numbers
                SET Number = Number + 1
                WHERE Category = 'Large';
                
                -- Read again in the same transaction
                SELECT Category, COUNT(*) as NumberCount
                FROM Numbers
                GROUP BY Category;
                
                COMMIT TRANSACTION;",
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
                SELECT DISTINCT Category FROM Numbers;",
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
                                UPDATE Numbers 
                                SET IsProcessed = 1 
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
                                
                                UPDATE Numbers 
                                SET IsProcessed = 1 
                                WHERE Category = 'Large';",
                                "Transaction 2", "Query", transaction);
                            
                            transaction.Commit();
                        }
                    }
                });

                await Task.WhenAll(task1, task2);
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

            // Test 1: Arithmetic and conversion errors
            try
            {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8); // High probability of error
                await ExecuteSqlCommandAsync(logs, connection, 
                    $@"SELECT 
                        CASE WHEN '{errorType}' = 'OVERFLOW' THEN CAST(2147483647 + Number as INT)
                             WHEN '{errorType}' = 'CONVERSION' THEN CAST('invalid' as INT)
                             ELSE 1/0 
                        END as ErrorResult
                    FROM DataRecords 
                    WHERE ID = 1;", 
                    "Arithmetic error test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Query", "Arithmetic error", 0, e.Message);
            }

            // Test 2: Constraint violations
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, @"
                    -- Attempt to insert duplicate key
                    INSERT INTO DataRecords (ID, Number, Category, Status)
                    SELECT TOP 1 ID, Number, Category, Status FROM DataRecords;",
                    "Constraint violation test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Query", "Constraint violation", 0, e.Message);
            }

            // Test 3: Invalid object references
            try
            {
                var invalidObject = $"NonExistent_{random.Next(1000)}";
                await ExecuteSqlCommandAsync(logs, connection, 
                    $"SELECT * FROM {invalidObject};",
                    "Invalid object test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Query", "Invalid object", 0, e.Message);
            }

            // Test 4: Syntax errors with varying complexity
            try
            {
                var errorType = GetRandomWeighted(random, ErrorTypes, 0.8);
                var errorQuery = errorType switch
                {
                    "SYNTAX" => "SELEC * FORM DataRecords",
                    "JOIN" => "SELECT * FROM DataRecords INNER JOIN NumberSequence",
                    "GROUP" => "SELECT Category, COUNT(*) DataRecords GROUP Category",
                    _ => "SELECT * FROM DataRecords WHERE;"
                };
                await ExecuteSqlCommandAsync(logs, connection, errorQuery, "Syntax error test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Query", "Syntax error", 0, e.Message);
            }

            // Test 5: Lock timeout simulation
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, @"
                    BEGIN TRANSACTION;
                    
                    -- Set a short lock timeout
                    SET LOCK_TIMEOUT 1000;
                    
                    -- Try to update records that might be locked
                    UPDATE DataRecords WITH (UPDLOCK)
                    SET ProcessingTime = ProcessingTime + 1
                    WHERE Category IN (
                        SELECT TOP 1 Category 
                        FROM DataRecords WITH (UPDLOCK)
                        ORDER BY NEWID()
                    );
                    
                    COMMIT TRANSACTION;",
                    "Lock timeout test", "Query");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Query", "Lock timeout", 0, e.Message);
            }
        }

        private static async Task CleanupDatabase(SqlConnection connection, List<LogEntry> logs)
        {
            connection.ChangeDatabase("master");
            await ExecuteSqlCommandAsync(logs, connection, 
                "ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB;",
                "Drop database", "Database");
        }
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
