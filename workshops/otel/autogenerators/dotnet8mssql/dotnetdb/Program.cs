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
using System.Linq;
using System.Collections.Concurrent;

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

    // In-memory analytics for periodic summary
    private static readonly ConcurrentBag<QueryAnalytics> QueryAnalyticsBag = new();
    private static readonly ConcurrentBag<ErrorAnalytics> ErrorAnalyticsBag = new();
    private static readonly ConcurrentBag<LockAnalytics> LockAnalyticsBag = new();

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

        var summaryTimer = new System.Timers.Timer(60000); // 1 minute
        summaryTimer.Elapsed += (s, e) => EmitSummaryLogs(logger);
        summaryTimer.Start();

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

                    // --- NEW: Check for locks and blocking sessions ---
                    CheckLocksAndBlocking(baseConnectionString, logger);
                    // --- END NEW ---

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
    /// Enhanced: logs execution time, rows, logical reads/writes, errors, and trace context in structured JSON.
    /// </summary>
    /// <param name="baseConnectionString">Base connection string (without Database=...)</param>
    /// <param name="dbName">Database name</param>
    /// <param name="logger">ILogger instance</param>
    static void ListStoredProceduresForDatabase(string baseConnectionString, string dbName, ILogger logger)
    {
        string dbConnectionString = baseConnectionString + $"Database={dbName};";
        const int INFO = 20;
        const int ERROR = 40;
        try
        {
            using var dbConnection = new SqlConnection(dbConnectionString);
            dbConnection.Open();
            LogDbEvent(logger, INFO, dbName, "ConnectToDatabase", "OPEN CONNECTION");

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
                    string procName = procReader.GetString(0);
                    string procDef = procReader.GetString(1);
                    procedures.Add((procName, procDef));
                }
            }
            if (procedures.Count == 0)
            {
                LogDbEvent(logger, INFO, dbName, "(none)", "No user stored procedures found.");
                dbConnection.Close();
                return;
            }
            foreach (var (procName, procDef) in procedures)
            {
                var stats = GetProcedureStats(dbConnection, dbName, procName);
                using var activity = ActivitySource.StartActivity(procName, ActivityKind.Client);
                var stopwatch = Stopwatch.StartNew();
                int rows = 0;
                string status = "success";
                string? errorMessage = null;
                try
                {
                    using var execCmd = new SqlCommand($"EXEC {procName}", dbConnection);
                    execCmd.CommandTimeout = 30;
                    using var reader = execCmd.ExecuteReader();
                    while (reader.Read()) rows++;
                    stopwatch.Stop();
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    status = "error";
                    errorMessage = ex.Message;
                    ErrorAnalyticsBag.Add(new ErrorAnalytics {
                        Timestamp = DateTime.UtcNow,
                        Database = dbName,
                        Procedure = procName,
                        ErrorMessage = ex.Message
                    });
                }
                // Add analytics for summary
                QueryAnalyticsBag.Add(new QueryAnalytics {
                    Timestamp = DateTime.UtcNow,
                    Database = dbName,
                    Procedure = procName,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    Rows = rows,
                    LogicalReads = stats.TotalLogicalReads,
                    LogicalWrites = stats.TotalLogicalWrites,
                    CpuTime = stats.TotalWorkerTime,
                    Status = status
                });
                // Log as structured JSON
                var logObj = new {
                    type = "query",
                    timestamp = DateTime.UtcNow,
                    database = dbName,
                    procedure = procName,
                    duration_ms = stopwatch.ElapsedMilliseconds,
                    rows,
                    logical_reads = stats.TotalLogicalReads,
                    logical_writes = stats.TotalLogicalWrites,
                    cpu_time_ms = stats.TotalWorkerTime,
                    status,
                    error_message = errorMessage,
                    trace_id = activity?.TraceId.ToString(),
                    span_id = activity?.SpanId.ToString()
                };
                logger.LogInformation(JsonSerializer.Serialize(logObj));
                // Add tags to span
                activity?.SetTag("db.rows", rows);
                activity?.SetTag("db.duration_ms", stopwatch.ElapsedMilliseconds);
                activity?.SetTag("db.system", "mssql");
                activity?.SetTag("db.name", "sp-" + dbName);
                activity?.SetTag("db.statement", procDef);
                activity?.SetTag("db.operation", procName);
                activity?.SetTag("db.logical_reads", stats.TotalLogicalReads);
                activity?.SetTag("db.logical_writes", stats.TotalLogicalWrites);
                activity?.SetTag("db.cpu_time_ms", stats.TotalWorkerTime);
                if (status == "error")
                {
                    activity?.SetTag("otel.status_code", "ERROR");
                    activity?.SetTag("otel.status_description", errorMessage);
                }
            }
            dbConnection.Close();
            LogDbEvent(logger, INFO, dbName, "DisconnectFromDatabase", "CLOSE CONNECTION");
        }
        catch (Exception ex)
        {
            LogDbError(logger, ERROR, dbName, "ListStoredProcedures", "SELECT ... FROM sys.procedures ...", ex.Message);
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
        logger.LogInformation(
            "DbEvent: timestamp={timestamp} severity={severity} database={database} procedure_name={procedure_name} sql_statement={sql_statement}",
            DateTime.UtcNow.ToString("o"),
            severity,
            database,
            procedureName,
            sqlStatement
        );
        // Console.WriteLine(...);
    }

    /// <summary>
    /// Logs a database error as JSON.
    /// </summary>
    static void LogDbError(ILogger logger, int severity, string database, string procedureName, string sqlStatement, string exception)
    {
        logger.LogError(
            "DbError: timestamp={timestamp} severity={severity} database={database} procedure_name={procedure_name} sql_statement={sql_statement} exception={exception}",
            DateTime.UtcNow.ToString("o"),
            severity,
            database,
            procedureName,
            sqlStatement,
            exception
        );
        // Console.WriteLine(...);
    }

    /// <summary>
    /// Checks for current locks and blocking sessions in SQL Server, logs the results, and emits OpenTelemetry spans for blocking sessions.
    /// Enhanced: logs blocking info, wait stats, and trace context in structured JSON.
    /// </summary>
    /// <param name="baseConnectionString">Base connection string (without Database=...)</param>
    /// <param name="logger">ILogger instance</param>
    static void CheckLocksAndBlocking(string baseConnectionString, ILogger logger)
    {
        const int INFO = 20;
        const int ERROR = 40;
        string dbConnectionString = baseConnectionString + "Database=master;";
        try
        {
            using var dbConnection = new SqlConnection(dbConnectionString);
            dbConnection.Open();
            string lockQuery = @"
                SELECT 
                    tl.resource_type,
                    tl.request_mode,
                    tl.request_status,
                    tl.request_session_id,
                    es.login_name,
                    es.host_name,
                    es.program_name,
                    er.blocking_session_id,
                    er.wait_type,
                    er.wait_time,
                    er.status,
                    er.command
                FROM sys.dm_tran_locks AS tl
                LEFT JOIN sys.dm_exec_sessions AS es ON tl.request_session_id = es.session_id
                LEFT JOIN sys.dm_exec_requests AS er ON tl.request_session_id = er.session_id
                WHERE tl.resource_database_id = DB_ID();";
            using var lockCommand = new SqlCommand(lockQuery, dbConnection);
            using var lockReader = lockCommand.ExecuteReader();
            while (lockReader.Read())
            {
                var lockInfo = new LockAnalytics {
                    Timestamp = DateTime.UtcNow,
                    SessionId = lockReader["request_session_id"],
                    BlockingSessionId = lockReader["blocking_session_id"],
                    ResourceType = lockReader["resource_type"],
                    RequestMode = lockReader["request_mode"],
                    RequestStatus = lockReader["request_status"],
                    LoginName = lockReader["login_name"],
                    HostName = lockReader["host_name"],
                    ProgramName = lockReader["program_name"],
                    WaitType = lockReader["wait_type"],
                    WaitTime = lockReader["wait_time"],
                    Status = lockReader["status"],
                    Command = lockReader["command"]
                };
                LockAnalyticsBag.Add(lockInfo);
                var logObj = new {
                    type = "lock",
                    timestamp = lockInfo.Timestamp,
                    session_id = lockInfo.SessionId,
                    blocking_session_id = lockInfo.BlockingSessionId,
                    resource_type = lockInfo.ResourceType,
                    request_mode = lockInfo.RequestMode,
                    request_status = lockInfo.RequestStatus,
                    login_name = lockInfo.LoginName,
                    host_name = lockInfo.HostName,
                    program_name = lockInfo.ProgramName,
                    wait_type = lockInfo.WaitType,
                    wait_time = lockInfo.WaitTime,
                    status = lockInfo.Status,
                    command = lockInfo.Command
                };
                logger.LogInformation(JsonSerializer.Serialize(logObj));
                if (lockInfo.BlockingSessionId != DBNull.Value && Convert.ToInt32(lockInfo.BlockingSessionId) != 0)
                {
                    using var activity = ActivitySource.StartActivity($"BlockingSession-{lockInfo.BlockingSessionId}", ActivityKind.Internal);
                    activity?.SetTag("db.system", "mssql");
                    activity?.SetTag("db.name", "master");
                    activity?.SetTag("lock.session_id", lockInfo.SessionId);
                    activity?.SetTag("lock.blocking_session_id", lockInfo.BlockingSessionId);
                    activity?.SetTag("lock.resource_type", lockInfo.ResourceType);
                    activity?.SetTag("lock.request_mode", lockInfo.RequestMode);
                    activity?.SetTag("lock.request_status", lockInfo.RequestStatus);
                    activity?.SetTag("lock.login_name", lockInfo.LoginName);
                    activity?.SetTag("lock.host_name", lockInfo.HostName);
                    activity?.SetTag("lock.program_name", lockInfo.ProgramName);
                    activity?.SetTag("lock.wait_type", lockInfo.WaitType);
                    activity?.SetTag("lock.wait_time", lockInfo.WaitTime);
                    activity?.SetTag("lock.status", lockInfo.Status);
                    activity?.SetTag("lock.command", lockInfo.Command);
                }
            }
            dbConnection.Close();
        }
        catch (Exception ex)
        {
            LogDbError(logger, ERROR, "master", "CheckLocksAndBlocking", "sys.dm_tran_locks ...", ex.Message);
        }
    }

    /// <summary>
    /// Emits periodic summary logs for analytics: top slowest queries, most frequent procedures, error rates, blocking/lock stats.
    /// </summary>
    static void EmitSummaryLogs(ILogger logger)
    {
        var now = DateTime.UtcNow;
        // Top 3 slowest queries
        var topSlow = QueryAnalyticsBag.OrderByDescending(q => q.DurationMs).Take(3).ToList();
        if (topSlow.Count > 0)
        {
            logger.LogInformation(JsonSerializer.Serialize(new {
                type = "summary",
                timestamp = now,
                summary_type = "top_slowest_queries",
                queries = topSlow.Select(q => new {
                    q.Database, q.Procedure, q.DurationMs, q.Rows, q.Status
                })
            }));
        }
        // Most frequent procedures
        var freq = QueryAnalyticsBag.GroupBy(q => q.Procedure)
            .Select(g => new { Procedure = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(3).ToList();
        if (freq.Count > 0)
        {
            logger.LogInformation(JsonSerializer.Serialize(new {
                type = "summary",
                timestamp = now,
                summary_type = "most_frequent_procedures",
                procedures = freq
            }));
        }
        // Error rates
        var errorRates = ErrorAnalyticsBag.GroupBy(e => e.Procedure)
            .Select(g => new { Procedure = g.Key, ErrorCount = g.Count() })
            .OrderByDescending(x => x.ErrorCount).Take(3).ToList();
        if (errorRates.Count > 0)
        {
            logger.LogInformation(JsonSerializer.Serialize(new {
                type = "summary",
                timestamp = now,
                summary_type = "error_rates",
                errors = errorRates
            }));
        }
        // Blocking/lock stats
        var blocking = LockAnalyticsBag.Where(l => l.BlockingSessionId != DBNull.Value && Convert.ToInt32(l.BlockingSessionId) != 0).ToList();
        if (blocking.Count > 0)
        {
            logger.LogInformation(JsonSerializer.Serialize(new {
                type = "summary",
                timestamp = now,
                summary_type = "blocking_sessions",
                blocking_sessions = blocking.Select(l => new {
                    l.SessionId, l.BlockingSessionId, l.ResourceType, l.RequestMode, l.WaitType, l.WaitTime
                })
            }));
        }
        // Clear analytics for next interval
        QueryAnalyticsBag.Clear();
        ErrorAnalyticsBag.Clear();
        LockAnalyticsBag.Clear();
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

    // --- Analytics record types for in-memory aggregation ---
    /// <summary>
    /// Analytics for a single query execution.
    /// </summary>
    class QueryAnalytics
    {
        public DateTime Timestamp { get; set; }
        public string? Database { get; set; }
        public string? Procedure { get; set; }
        public long DurationMs { get; set; }
        public int Rows { get; set; }
        public long? LogicalReads { get; set; }
        public long? LogicalWrites { get; set; }
        public long? CpuTime { get; set; }
        public string? Status { get; set; }
    }
    /// <summary>
    /// Analytics for a single error.
    /// </summary>
    class ErrorAnalytics
    {
        public DateTime Timestamp { get; set; }
        public string? Database { get; set; }
        public string? Procedure { get; set; }
        public string? ErrorMessage { get; set; }
    }
    /// <summary>
    /// Analytics for a single lock/blocking event.
    /// </summary>
    class LockAnalytics
    {
        public DateTime Timestamp { get; set; }
        public object? SessionId { get; set; }
        public object? BlockingSessionId { get; set; }
        public object? ResourceType { get; set; }
        public object? RequestMode { get; set; }
        public object? RequestStatus { get; set; }
        public object? LoginName { get; set; }
        public object? HostName { get; set; }
        public object? ProgramName { get; set; }
        public object? WaitType { get; set; }
        public object? WaitTime { get; set; }
        public object? Status { get; set; }
        public object? Command { get; set; }
    }
}
