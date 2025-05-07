using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Text.Json;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("dotnet8mssql-db"));
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri("http://localhost:4317");
                    });
                });
        });
        var logger = loggerFactory.CreateLogger<Program>();

        var baseConnectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        string[] dbsToCheck = { "master", "TestDB" };
        const int INFO = 20;
        const int ERROR = 40;

        while (true)
        {
            try
            {
                using (var connection = new SqlConnection(baseConnectionString))
                {
                    connection.Open();
                    var logObjConnect = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        database = "server",
                        procedure_name = "ConnectToServer",
                        sql_statement = "OPEN CONNECTION"
                    };
                    var logJsonConnect = JsonSerializer.Serialize(logObjConnect);
                    logger.LogInformation(logJsonConnect);
                    Console.WriteLine(logJsonConnect);

                    foreach (var dbName in dbsToCheck)
                    {
                        ListStoredProceduresForDatabase(baseConnectionString, dbName, logger);
                    }

                    connection.Close();
                    var logObjDisconnect = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        database = "server",
                        procedure_name = "DisconnectFromServer",
                        sql_statement = "CLOSE CONNECTION"
                    };
                    var logJsonDisconnect = JsonSerializer.Serialize(logObjDisconnect);
                    logger.LogInformation(logJsonDisconnect);
                    Console.WriteLine(logJsonDisconnect);
                }
            }
            catch (Exception ex)
            {
                var logObjError = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = ERROR,
                    database = "server",
                    procedure_name = "ConnectToServer",
                    sql_statement = "OPEN CONNECTION",
                    exception = ex.Message
                };
                var logJsonError = JsonSerializer.Serialize(logObjError);
                logger.LogError(logJsonError);
                Console.WriteLine(logJsonError);
                Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
            }
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before next loop
        }
    }

    static void ListStoredProceduresForDatabase(string baseConnectionString, string dbName, ILogger logger)
    {
        var dbConnectionString = baseConnectionString + $"Database={dbName};";
        const int INFO = 20;
        const int ERROR = 40;
        try
        {
            using (var dbConnection = new SqlConnection(dbConnectionString))
            {
                dbConnection.Open();
                var logObjDbConnect = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = INFO,
                    database = dbName,
                    procedure_name = "ConnectToDatabase",
                    sql_statement = "OPEN CONNECTION"
                };
                var logJsonDbConnect = JsonSerializer.Serialize(logObjDbConnect);
                logger.LogInformation(logJsonDbConnect);
                Console.WriteLine(logJsonDbConnect);

                string procQuery = @"
                    SELECT p.name, m.definition
                    FROM sys.procedures p
                    JOIN sys.sql_modules m ON p.object_id = m.object_id
                    WHERE p.is_ms_shipped = 0
                    ORDER BY p.name;";
                var procedures = new List<(string Name, string Definition)>();
                using (var procCommand = new SqlCommand(procQuery, dbConnection))
                using (var procReader = procCommand.ExecuteReader())
                {
                    while (procReader.Read())
                    {
                        procedures.Add((procReader.GetString(0), procReader.GetString(1)));
                    }
                } // procReader is now closed
                if (procedures.Count == 0)
                {
                    var logObjNone = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        database = dbName,
                        procedure_name = "(none)",
                        sql_statement = "No user stored procedures found."
                    };
                    var logJsonNone = JsonSerializer.Serialize(logObjNone);
                    logger.LogInformation(logJsonNone);
                    Console.WriteLine(logJsonNone);
                }
                else
                {
                    foreach (var (procName, procDef) in procedures)
                    {
                        // Query sys.dm_exec_procedure_stats for metrics
                        string statsQuery = @"
                            SELECT 
                                execution_count,
                                last_execution_time,
                                total_worker_time,
                                total_elapsed_time,
                                total_logical_reads,
                                total_logical_writes
                            FROM sys.dm_exec_procedure_stats
                            WHERE database_id = DB_ID(@dbName) AND OBJECT_NAME(object_id, database_id) = @procName;";
                        long? executionCount = null;
                        DateTime? lastExecutionTime = null;
                        long? totalWorkerTime = null;
                        long? totalElapsedTime = null;
                        long? totalLogicalReads = null;
                        long? totalLogicalWrites = null;
                        using (var statsCommand = new SqlCommand(statsQuery, dbConnection))
                        {
                            statsCommand.Parameters.AddWithValue("@dbName", dbName);
                            statsCommand.Parameters.AddWithValue("@procName", procName);
                            using (var statsReader = statsCommand.ExecuteReader())
                            {
                                if (statsReader.Read())
                                {
                                    executionCount = statsReader.IsDBNull(0) ? null : statsReader.GetInt64(0);
                                    lastExecutionTime = statsReader.IsDBNull(1) ? null : statsReader.GetDateTime(1);
                                    totalWorkerTime = statsReader.IsDBNull(2) ? null : statsReader.GetInt64(2);
                                    totalElapsedTime = statsReader.IsDBNull(3) ? null : statsReader.GetInt64(3);
                                    totalLogicalReads = statsReader.IsDBNull(4) ? null : statsReader.GetInt64(4);
                                    totalLogicalWrites = statsReader.IsDBNull(5) ? null : statsReader.GetInt64(5);
                                }
                            }
                        }
                        var logObjProc = new
                        {
                            timestamp = DateTime.UtcNow.ToString("o"),
                            severity = INFO,
                            database = dbName,
                            procedure_name = procName,
                            sql_statement = procDef,
                            execution_count = executionCount,
                            last_execution_time = lastExecutionTime?.ToString("o"),
                            total_worker_time = totalWorkerTime,
                            total_elapsed_time = totalElapsedTime,
                            total_logical_reads = totalLogicalReads,
                            total_logical_writes = totalLogicalWrites
                        };
                        var logJsonProc = JsonSerializer.Serialize(logObjProc);
                        logger.LogInformation(logJsonProc);
                        Console.WriteLine(logJsonProc);
                    }
                }
                dbConnection.Close();
                var logObjDbDisconnect = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = INFO,
                    database = dbName,
                    procedure_name = "DisconnectFromDatabase",
                    sql_statement = "CLOSE CONNECTION"
                };
                var logJsonDbDisconnect = JsonSerializer.Serialize(logObjDbDisconnect);
                logger.LogInformation(logJsonDbDisconnect);
                Console.WriteLine(logJsonDbDisconnect);
            }
        }
        catch (Exception ex)
        {
            var logObjError = new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                severity = ERROR,
                database = dbName,
                procedure_name = "ListStoredProcedures",
                sql_statement = "SELECT ... FROM sys.procedures ...",
                exception = ex.Message
            };
            var logJsonError = JsonSerializer.Serialize(logObjError);
            logger.LogError(logJsonError);
            Console.WriteLine(logJsonError);
            Console.WriteLine($"[sqlanalyzer] Exception in {dbName}: {ex.Message}");
        }
    }
}
