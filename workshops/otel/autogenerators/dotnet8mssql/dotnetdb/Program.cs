using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Text.Json;
using System.Collections.Generic;
using OpenTelemetry.Trace;
using System.Diagnostics;

/*
 * Program.cs
 *
 * This application connects to a SQL Server instance, logs connection events, and enumerates user-defined stored procedures in specified databases.
 * For each stored procedure, it logs metadata and performance statistics, and emits OpenTelemetry traces via OTLP gRPC to localhost:4317.
 *
 * - Logs are output to both console and OpenTelemetry.
 * - Traces are emitted for each stored procedure, with db.name prefixed by 'sp-'.
 * - The app runs in an infinite loop, repeating every 5 seconds.
 */

class Program
{
    static readonly ActivitySource ActivitySource = new("dotnetdb");

    /// <summary>
    /// Entry point. Sets up OpenTelemetry tracing and logging, then loops forever:
    /// - Connects to SQL Server
    /// - Logs connection events
    /// - For each database, lists stored procedures and emits logs/traces
    /// - Waits 5 seconds and repeats
    /// </summary>
    static void Main()
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CX-DB-query"))
            .AddSource("dotnetdb")
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri("http://localhost:4317");
            })
            .AddConsoleExporter()
            .Build();
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CX-DB-query"));
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri("http://localhost:4317");
                    });
                });
        });
        var logger = loggerFactory.CreateLogger<Program>();

        // Connection string for SQL Server (update as needed)
        var baseConnectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        // Databases to check for stored procedures
        string[] dbsToCheck = { "master", "TestDB" };
        const int INFO = 20;
        const int ERROR = 40;

        // Main loop: repeat every 5 seconds
        while (true)
        {
            try
            {
                using (var connection = new SqlConnection(baseConnectionString))
                {
                    connection.Open();
                    LogDbEvent(logger, INFO, "server", "ConnectToServer", "OPEN CONNECTION");

                    foreach (var dbName in dbsToCheck)
                    {
                        ListStoredProceduresForDatabase(baseConnectionString, dbName, logger);
                    }

                    connection.Close();
                    LogDbEvent(logger, INFO, "server", "DisconnectFromServer", "CLOSE CONNECTION");
                }
            }
            catch (Exception ex)
            {
                LogDbError(logger, ERROR, "server", "ConnectToServer", "OPEN CONNECTION", ex.Message);
                // Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
            }
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before next loop
        }
    }

    /// <summary>
    /// Lists user-defined stored procedures in the given database, logs their metadata and performance stats, and emits an OpenTelemetry span for each.
    /// </summary>
    /// <param name="baseConnectionString">Base connection string (without Database=...)</param>
    /// <param name="dbName">Database name</param>
    /// <param name="logger">ILogger instance</param>
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
                LogDbEvent(logger, INFO, dbName, "ConnectToDatabase", "OPEN CONNECTION");

                // Query for user-defined stored procedures and their definitions
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
                }
                if (procedures.Count == 0)
                {
                    LogDbEvent(logger, INFO, dbName, "(none)", "No user stored procedures found.");
                }
                else
                {
                    foreach (var (procName, procDef) in procedures)
                    {
                        // Emit a span for this stored procedure using ActivitySource
                        using (var activity = ActivitySource.StartActivity(procName, ActivityKind.Client))
                        {
                            if (activity != null)
                            {
                                Console.WriteLine($"[DEBUG] Created span: TraceId={activity.TraceId}, SpanId={activity.SpanId}, Name={dbName}");
                                activity.SetTag("db.system", "mssql");
                                activity.SetTag("db.name", "sp-" + dbName);
                                activity.SetTag("db.statement", procDef);
                                activity.SetTag("server.address", "localhost");
                                activity.SetTag("span.kind", "client");
                                activity.SetTag("cx.subsystem.name", "CX-DB-query");
                                activity.SetTag("cx.application.name", "workshop");
                                activity.SetTag("otel.library.name", "OpenTelemetry.Instrumentation.SqlClient");
                                activity.SetTag("otel.library.version", "1.11.0-beta.2");
                                activity.SetTag("otel.scope.name", "OpenTelemetry.Instrumentation.SqlClient");
                                activity.SetTag("otel.scope.version", "1.11.0-beta.2");
                            }
                            // Query sys.dm_exec_procedure_stats for metrics
                            var stats = GetProcedureStats(dbConnection, dbName, procName);

                            // Log procedure info and stats
                            var logObjProc = new
                            {
                                timestamp = DateTime.UtcNow.ToString("o"),
                                severity = INFO,
                                database = dbName,
                                procedure_name = procName,
                                sql_statement = procDef,
                                execution_count = stats.ExecutionCount,
                                last_execution_time = stats.LastExecutionTime?.ToString("o"),
                                total_worker_time = stats.TotalWorkerTime,
                                total_elapsed_time = stats.TotalElapsedTime,
                                total_logical_reads = stats.TotalLogicalReads,
                                total_logical_writes = stats.TotalLogicalWrites
                            };
                            var logJsonProc = JsonSerializer.Serialize(logObjProc);
                            logger.LogInformation(logJsonProc);
                            // Console.WriteLine(logJsonProc);
                        }
                    }
                }
                dbConnection.Close();
                LogDbEvent(logger, INFO, dbName, "DisconnectFromDatabase", "CLOSE CONNECTION");
            }
        }
        catch (Exception ex)
        {
            LogDbError(logger, ERROR, dbName, "ListStoredProcedures", "SELECT ... FROM sys.procedures ...", ex.Message);
            // Console.WriteLine($"[sqlanalyzer] Exception in {dbName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Queries sys.dm_exec_procedure_stats for the given procedure and returns performance statistics.
    /// </summary>
    /// <param name="dbConnection">Open SqlConnection</param>
    /// <param name="dbName">Database name</param>
    /// <param name="procName">Procedure name</param>
    /// <returns>ProcedureStats struct with metrics</returns>
    static ProcedureStats GetProcedureStats(SqlConnection dbConnection, string dbName, string procName)
    {
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
        return new ProcedureStats
        {
            ExecutionCount = executionCount,
            LastExecutionTime = lastExecutionTime,
            TotalWorkerTime = totalWorkerTime,
            TotalElapsedTime = totalElapsedTime,
            TotalLogicalReads = totalLogicalReads,
            TotalLogicalWrites = totalLogicalWrites
        };
    }

    /// <summary>
    /// Logs a database event (connect/disconnect/none found) as JSON.
    /// </summary>
    static void LogDbEvent(ILogger logger, int severity, string database, string procedureName, string sqlStatement)
    {
        var logObj = new
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            severity,
            database,
            procedure_name = procedureName,
            sql_statement = sqlStatement
        };
        var logJson = JsonSerializer.Serialize(logObj);
        logger.LogInformation(logJson);
        // Console.WriteLine(logJson);
    }

    /// <summary>
    /// Logs a database error as JSON.
    /// </summary>
    static void LogDbError(ILogger logger, int severity, string database, string procedureName, string sqlStatement, string exception)
    {
        var logObj = new
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            severity,
            database,
            procedure_name = procedureName,
            sql_statement = sqlStatement,
            exception
        };
        var logJson = JsonSerializer.Serialize(logObj);
        logger.LogError(logJson);
        // Console.WriteLine(logJson);
    }

    /// <summary>
    /// Struct to hold procedure performance statistics.
    /// </summary>
    struct ProcedureStats
    {
        public long? ExecutionCount;
        public DateTime? LastExecutionTime;
        public long? TotalWorkerTime;
        public long? TotalElapsedTime;
        public long? TotalLogicalReads;
        public long? TotalLogicalWrites;
    }
}
