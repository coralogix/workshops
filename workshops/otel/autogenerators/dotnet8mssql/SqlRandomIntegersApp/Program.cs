using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

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
        
        static async Task Main(string[] args)
        {
            string server = "localhost";
            string username = "sa";
            string password = "Toortoor9#";
            string connectionString = $"Server={server};User Id={username};Password={password};";
            var logs = new List<LogEntry>();
            var stopwatch = new Stopwatch();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    LogAction(logs, "INFO", "Connecting to SQL Server", "Connection successful");

                    // Setup Database
                    await SetupDatabase(connection, logs);

                    // Run Test Scenarios
                    await RunFastQueries(connection, logs);
                    await RunSlowQueries(connection, logs);
                    await RunParallelQueries(connection, logs);
                    await RunTempTableQueries(connection, logs);
                    await RunIsolationLevelTests(connection, logs);
                    await RunDeadlockScenarios(connection, logs);
                    await RunFailedQueries(connection, logs);

                    // Cleanup
                    await CleanupDatabase(connection, logs);
                }
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", e.Message, "An error occurred during test execution");
            }
            finally
            {
                // Write logs to a file
                await File.WriteAllTextAsync("logs.json", JsonConvert.SerializeObject(logs, Formatting.Indented));
            }
        }

        private static async Task SetupDatabase(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Drop existing database if it exists
            await ExecuteSqlCommandAsync(logs, connection, 
                "IF DB_ID('TestDB') IS NOT NULL BEGIN ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB; END", 
                "Checked and dropped existing 'TestDB'");

            // Create new database
            await ExecuteSqlCommandAsync(logs, connection, "CREATE DATABASE TestDB", "Database 'TestDB' created");
            connection.ChangeDatabase("TestDB");

            // Create tables
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE TABLE Numbers (
                    ID INT PRIMARY KEY IDENTITY(1,1),
                    Number INT,
                    Description NVARCHAR(1000),
                    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
                    Category NVARCHAR(50),
                    IsProcessed BIT DEFAULT 0
                );
                CREATE INDEX IX_Numbers_Number ON Numbers(Number);
                CREATE INDEX IX_Numbers_Category ON Numbers(Category);
                ", "Created Numbers table with indexes");

            // Insert test data in batches
            stopwatch.Start();
            Random random = new Random();
            var categories = new[] { "Small", "Medium", "Large", "Extra Large" };
            
            for (int batchStart = 0; batchStart < TOTAL_RECORDS; batchStart += BATCH_SIZE)
            {
                StringBuilder batchInsert = new StringBuilder("INSERT INTO Numbers (Number, Description, Category) VALUES ");
                var values = new List<string>();

                for (int i = 0; i < BATCH_SIZE && (batchStart + i) < TOTAL_RECORDS; i++)
                {
                    int num = random.Next(1, 1000000);
                    string category = categories[random.Next(categories.Length)];
                    string desc = $"Test number {num} in category {category} with some additional description text for testing purposes";
                    values.Add($"({num}, '{desc}', '{category}')");
                }

                batchInsert.Append(string.Join(",", values));
                await ExecuteSqlCommandAsync(logs, connection, batchInsert.ToString(), 
                    $"Inserted batch of {values.Count} records");
            }
            stopwatch.Stop();
            LogAction(logs, "INFO", "Data Generation", 
                $"Generated {TOTAL_RECORDS} records in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunFastQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Test 1: Quick indexed lookup
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, 
                "SELECT TOP 100 * FROM Numbers WHERE Number = 42", 
                "Fast Query - Indexed lookup");
            LogAction(logs, "INFO", "Fast Query", $"Indexed lookup completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 2: Category count with index
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, 
                "SELECT Category, COUNT(*) as Count FROM Numbers GROUP BY Category", 
                "Fast Query - Category count");
            LogAction(logs, "INFO", "Fast Query", $"Category count completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunSlowQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Test 1: Full table scan with string operations
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, @"
                WITH NumberGroups AS (
                    SELECT 
                        Number,
                        Description,
                        Category,
                        SUBSTRING(Description, 1, 10) as DescriptionStart,
                        ROW_NUMBER() OVER (PARTITION BY Category ORDER BY Number) as RowNum
                    FROM Numbers
                )
                SELECT 
                    Category,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    COUNT(*) as CategoryCount,
                    STRING_AGG(DescriptionStart, ',') as DescriptionStarts
                FROM NumberGroups
                WHERE RowNum <= 1000
                GROUP BY Category
                HAVING COUNT(*) > 100;", 
                "Slow Query - Complex aggregation");
            LogAction(logs, "INFO", "Slow Query", $"Complex aggregation completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 2: Cross join to generate large result set
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT TOP 10000 
                    a.Number as Number1, 
                    b.Number as Number2,
                    ABS(a.Number - b.Number) as Difference
                FROM Numbers a
                CROSS APPLY (
                    SELECT TOP 100 Number 
                    FROM Numbers 
                    WHERE Number > a.Number
                    ORDER BY Number
                ) b
                WHERE a.Category = b.Category
                ORDER BY Difference;", 
                "Slow Query - Cross apply");
            LogAction(logs, "INFO", "Slow Query", $"Cross apply completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 3: Recursive CTE
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, @"
                WITH NumberSequence AS (
                    SELECT TOP 100 
                        Number,
                        Category,
                        1 as Level
                    FROM Numbers
                    WHERE Category = 'Large'
                    
                    UNION ALL
                    
                    SELECT 
                        n.Number,
                        n.Category,
                        ns.Level + 1
                    FROM Numbers n
                    INNER JOIN NumberSequence ns ON n.Number > ns.Number
                    WHERE n.Category = ns.Category
                        AND ns.Level < 5
                )
                SELECT 
                    Level,
                    COUNT(*) as NumberCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber
                FROM NumberSequence
                GROUP BY Level
                OPTION (MAXRECURSION 0);", 
                "Slow Query - Recursive CTE");
            LogAction(logs, "INFO", "Slow Query", $"Recursive CTE completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunParallelQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Enable parallel query execution
            await ExecuteSqlCommandAsync(logs, connection, 
                "EXEC sp_configure 'max degree of parallelism', 4;", 
                "Configured parallel execution");

            // Test 1: Parallel full table scan with computation
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, @"
                SELECT 
                    Category,
                    COUNT(*) as TotalCount,
                    AVG(CAST(Number as FLOAT)) as AvgNumber,
                    MIN(Number) as MinNumber,
                    MAX(Number) as MaxNumber,
                    SUM(CASE WHEN Number % 2 = 0 THEN 1 ELSE 0 END) as EvenCount,
                    STRING_AGG(CAST(Number as VARCHAR(20)), ',') WITHIN GROUP (ORDER BY Number) as NumberList
                FROM Numbers
                WHERE Number BETWEEN 1000 AND 900000
                GROUP BY Category
                OPTION (MAXDOP 4);", 
                "Parallel Query - Complex aggregation");
            LogAction(logs, "INFO", "Parallel Query", $"Complex parallel aggregation completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 2: Parallel join operations
            stopwatch.Restart();
            await ExecuteSqlCommandAsync(logs, connection, @"
                WITH NumberRanges AS (
                    SELECT 
                        Number,
                        Category,
                        NTILE(100) OVER (ORDER BY Number) as Range
                    FROM Numbers
                )
                SELECT 
                    r1.Range,
                    COUNT(*) as Combinations,
                    AVG(ABS(r1.Number - r2.Number)) as AvgDifference
                FROM NumberRanges r1
                JOIN NumberRanges r2 ON 
                    r1.Range = r2.Range AND 
                    r1.Number <> r2.Number
                GROUP BY r1.Range
                OPTION (MAXDOP 4);",
                "Parallel Query - Join operations");
            LogAction(logs, "INFO", "Parallel Query", $"Parallel join completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunTempTableQueries(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Test 1: Global temporary table with indexes
            stopwatch.Restart();
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
                "Temp Table - Global temp table operations");
            LogAction(logs, "INFO", "Temp Table", $"Global temp table operations completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 2: Table variables for intermediate results
            stopwatch.Restart();
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
                "Temp Table - Table variable operations");
            LogAction(logs, "INFO", "Temp Table", $"Table variable operations completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunIsolationLevelTests(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Test 1: Read Committed with nolock hint
            stopwatch.Restart();
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
                "Isolation Level - Read Committed with NOLOCK");
            LogAction(logs, "INFO", "Isolation Level", $"Read Committed test completed in {stopwatch.ElapsedMilliseconds}ms");

            // Test 2: Snapshot isolation
            stopwatch.Restart();
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
                "Isolation Level - Snapshot");
            LogAction(logs, "INFO", "Isolation Level", $"Snapshot isolation test completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static async Task RunDeadlockScenarios(SqlConnection connection, List<LogEntry> logs)
        {
            var stopwatch = new Stopwatch();

            // Create additional table for deadlock testing
            await ExecuteSqlCommandAsync(logs, connection, @"
                CREATE TABLE NumberCategories (
                    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
                    CategoryName NVARCHAR(50) UNIQUE,
                    LastUpdated DATETIME2 DEFAULT GETUTCDATE()
                );
                
                INSERT INTO NumberCategories (CategoryName)
                SELECT DISTINCT Category FROM Numbers;",
                "Created NumberCategories table");

            // Test 1: Simulate deadlock with update operations
            stopwatch.Restart();
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
                                "Deadlock Test - Transaction 1",
                                transaction);
                            
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
                                "Deadlock Test - Transaction 2",
                                transaction);
                            
                            transaction.Commit();
                        }
                    }
                });

                await Task.WhenAll(task1, task2);
            }
            catch (Exception e)
            {
                LogAction(logs, "INFO", "Deadlock Test", $"Expected deadlock occurred: {e.Message}");
            }
            finally
            {
                stopwatch.Stop();
                LogAction(logs, "INFO", "Deadlock Test", $"Deadlock scenario completed in {stopwatch.ElapsedMilliseconds}ms");
            }

            // Cleanup deadlock test objects
            await ExecuteSqlCommandAsync(logs, connection, 
                "DROP TABLE NumberCategories;",
                "Cleaned up deadlock test objects");
        }

        private static async Task RunFailedQueries(SqlConnection connection, List<LogEntry> logs)
        {
            // Test 1: Division by zero
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, 
                    "SELECT 1/0 as Result", 
                    "Failed Query - Division by zero");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Failed Query", $"Division by zero error: {e.Message}");
            }

            // Test 2: Invalid column name
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, 
                    "SELECT NonExistentColumn FROM Numbers", 
                    "Failed Query - Invalid column");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Failed Query", $"Invalid column error: {e.Message}");
            }

            // Test 3: Syntax error
            try
            {
                await ExecuteSqlCommandAsync(logs, connection, 
                    "SELEC * FORM Numbers", 
                    "Failed Query - Syntax error");
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", "Failed Query", $"Syntax error: {e.Message}");
            }
        }

        private static async Task CleanupDatabase(SqlConnection connection, List<LogEntry> logs)
        {
            connection.ChangeDatabase("master");
            await ExecuteSqlCommandAsync(logs, connection, 
                "ALTER DATABASE TestDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE TestDB;", 
                "Database cleanup completed");
        }

        private static async Task ExecuteSqlCommandAsync(List<LogEntry> logs, SqlConnection connection, string sql, string successMessage, SqlTransaction? transaction = null)
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
                    await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();
                    LogAction(logs, "INFO", sql, $"{successMessage} (Duration: {stopwatch.ElapsedMilliseconds}ms)");
                }
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                LogAction(logs, "ERROR", sql, $"Error: {e.Message} (Duration: {stopwatch.ElapsedMilliseconds}ms)");
                throw;
            }
        }

        private static void LogAction(List<LogEntry> logs, string severity, string sql, string result)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Severity = severity,
                SqlStatement = sql,
                Result = result,
                Duration = 0 // Will be filled by actual query execution
            };
            Console.WriteLine(JsonConvert.SerializeObject(log, Formatting.Indented));
            logs.Add(log);
        }
    }

    class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? Severity { get; set; }
        public string? SqlStatement { get; set; }
        public string? Result { get; set; }
        public long Duration { get; set; }
    }
}
