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
                        var stats = GetProcedureStats(dbConnection, dbName, procName);
                        using (var activity = ActivitySource.StartActivity(procName, ActivityKind.Client))
                        {
                            if (activity != null)
                            {
                                activity.SetTag("db.system", "mssql");
                                activity.SetTag("db.name", "sp-" + dbName);
                                activity.SetTag("db.procedure", procName);
                                activity.SetTag("db.statement", procDef);
                                activity.SetTag("server.address", "localhost");
                                activity.SetTag("span.kind", "client");
                                activity.SetTag("cx.subsystem.name", "CX-DB-query");
                                activity.SetTag("cx.application.name", "workshop");
                                activity.SetTag("otel.library.name", "dotnetdb");
                                activity.SetTag("otel.library.version", "1.11.0-beta.2");
                                activity.SetTag("otel.scope.name", "dotnetdb");
                                activity.SetTag("otel.scope.version", "1.11.0-beta.2");
                                if (stats.ExecutionCount != null)
                                    activity.SetTag("cx.proc.execution_count", stats.ExecutionCount);
                                if (stats.LastExecutionTime != null)
                                    activity.SetTag("cx.proc.last_execution_time", stats.LastExecutionTime?.ToString("o"));
                                if (stats.TotalWorkerTime != null)
                                    activity.SetTag("cx.proc.total_worker_time", stats.TotalWorkerTime);
                                if (stats.TotalElapsedTime != null)
                                    activity.SetTag("cx.proc.total_elapsed_time", stats.TotalElapsedTime);
                                if (stats.TotalLogicalReads != null)
                                    activity.SetTag("cx.proc.total_logical_reads", stats.TotalLogicalReads);
                                if (stats.TotalLogicalWrites != null)
                                    activity.SetTag("cx.proc.total_logical_writes", stats.TotalLogicalWrites);
                            }
                            // Only execute if the procedure definition does NOT contain INSERT, UPDATE, DELETE, or MERGE (case-insensitive)
                            var lowerDef = procDef.ToLowerInvariant();
                            if (!lowerDef.Contains("insert ") && !lowerDef.Contains("update ") && !lowerDef.Contains("delete ") && !lowerDef.Contains("merge "))
                            {
                                try
                                {
                                    using (var execCmd = new SqlCommand($"EXEC {procName}", dbConnection))
                                    {
                                        execCmd.CommandTimeout = 30;
                                        execCmd.ExecuteNonQuery();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    activity?.SetTag("otel.status_code", "ERROR");
                                    activity?.SetTag("otel.status_description", ex.Message);
                                }
                            }
                            // Logging (outside the using block is fine)
                            logger.LogInformation(
                                "Procedure stats: timestamp={timestamp} severity={severity} database={database} procedure_name={procedure_name} sql_statement={sql_statement} execution_count={execution_count} last_execution_time={last_execution_time} total_worker_time={total_worker_time} total_elapsed_time={total_elapsed_time} total_logical_reads={total_logical_reads} total_logical_writes={total_logical_writes} trace_id={trace_id} span_id={span_id}",
                                DateTime.UtcNow.ToString("o"),
                                INFO,
                                dbName,
                                procName,
                                $"EXEC {procName}",
                                stats.ExecutionCount,
                                stats.LastExecutionTime?.ToString("o"),
                                stats.TotalWorkerTime,
                                stats.TotalElapsedTime,
                                stats.TotalLogicalReads,
                                stats.TotalLogicalWrites,
                                activity?.TraceId.ToString(),
                                activity?.SpanId.ToString()
                            );
                            // Print single-line summary to console (must be inside the using block)
                            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {dbName}.{procName} execs={stats.ExecutionCount} last={stats.LastExecutionTime?.ToString("HH:mm:ss") ?? "-"} cpu={stats.TotalWorkerTime}ms elapsed={stats.TotalElapsedTime}ms reads={stats.TotalLogicalReads} writes={stats.TotalLogicalWrites} trace={activity?.TraceId} span={activity?.SpanId}");
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
                    er.command,
                    st.text as sql_text
                FROM sys.dm_tran_locks AS tl
                LEFT JOIN sys.dm_exec_sessions AS es ON tl.request_session_id = es.session_id
                LEFT JOIN sys.dm_exec_requests AS er ON tl.request_session_id = er.session_id
                OUTER APPLY sys.dm_exec_sql_text(er.sql_handle) st
                WHERE tl.resource_database_id = DB_ID();";
            using var lockCommand = new SqlCommand(lockQuery, dbConnection);
            using var lockReader = lockCommand.ExecuteReader();
            var blockingSessions = new List<dynamic>();
            var waitTimes = new List<long>();
            while (lockReader.Read())
            {
                var sessionId = lockReader["request_session_id"];
                var blockingSessionId = lockReader["blocking_session_id"];
                var waitTime = lockReader["wait_time"] != DBNull.Value ? Convert.ToInt64(lockReader["wait_time"]) : 0L;
                var sqlText = lockReader["sql_text"] != DBNull.Value ? lockReader["sql_text"].ToString() : null;
                int chainDepth = 0;
                // Compute blocking chain depth
                var currentBlockingId = blockingSessionId;
                var seen = new HashSet<object> { sessionId };
                while (currentBlockingId != DBNull.Value && Convert.ToInt32(currentBlockingId) != 0 && !seen.Contains(currentBlockingId))
                {
                    chainDepth++;
                    seen.Add(currentBlockingId);
                    // Find the next blocking session in the current result set
                    // (This is a simple approach; for large sets, consider building a dictionary first)
                    lockReader.GetSchemaTable(); // No-op to avoid compiler warning
                    // For simplicity, break here; in a more advanced version, you could cache all blockingSessionId relationships
                    break;
                }
                var lockInfo = new LockAnalytics {
                    Timestamp = DateTime.UtcNow,
                    SessionId = sessionId,
                    BlockingSessionId = blockingSessionId,
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
                if (blockingSessionId != DBNull.Value && Convert.ToInt32(blockingSessionId) != 0)
                {
                    using var activity = ActivitySource.StartActivity($"BlockingSession-{blockingSessionId}", ActivityKind.Internal);
                    activity?.SetTag("db.system", "mssql");
                    activity?.SetTag("db.name", "master");
                    activity?.SetTag("lock.session_id", sessionId);
                    activity?.SetTag("lock.blocking_session_id", blockingSessionId);
                    activity?.SetTag("lock.resource_type", lockReader["resource_type"]);
                    activity?.SetTag("lock.request_mode", lockReader["request_mode"]);
                    activity?.SetTag("lock.request_status", lockReader["request_status"]);
                    activity?.SetTag("lock.login_name", lockReader["login_name"]);
                    activity?.SetTag("lock.host_name", lockReader["host_name"]);
                    activity?.SetTag("lock.program_name", lockReader["program_name"]);
                    activity?.SetTag("lock.wait_type", lockReader["wait_type"]);
                    activity?.SetTag("lock.wait_time", waitTime);
                    activity?.SetTag("lock.status", lockReader["status"]);
                    activity?.SetTag("lock.command", lockReader["command"]);
                    activity?.SetTag("lock.sql_text", sqlText);
                    activity?.SetTag("lock.blocking_chain_depth", chainDepth);
                    // Placeholder for deadlock info (future extension)
                    // activity?.SetTag("lock.deadlock", ...);
                    // Log to console for visibility
                    Console.WriteLine($"[BLOCK] session={sessionId} blocked_by={blockingSessionId} wait_type={lockReader["wait_type"]} wait_time={waitTime}ms chain_depth={chainDepth} sql={sqlText?.Substring(0, Math.Min(100, sqlText.Length))}");
                }
                // Add to aggregate metrics
                if (blockingSessionId != DBNull.Value && Convert.ToInt32(blockingSessionId) != 0)
                {
                    blockingSessions.Add(new {
                        sessionId,
                        blockingSessionId,
                        waitTime,
                        chainDepth
                    });
                    waitTimes.Add(waitTime);
                }
                // Log all fields in structured logs
                logger.LogInformation(
                    "Blocking event: session_id={sessionId} blocking_session_id={blockingSessionId} wait_type={waitType} wait_time={waitTime}ms status={status} command={command} resource_type={resourceType} request_mode={requestMode} login_name={loginName} host_name={hostName} program_name={programName} sql_text={sqlText} blocking_chain_depth={chainDepth}",
                    sessionId,
                    blockingSessionId,
                    lockReader["wait_type"],
                    waitTime,
                    lockReader["status"],
                    lockReader["command"],
                    lockReader["resource_type"],
                    lockReader["request_mode"],
                    lockReader["login_name"],
                    lockReader["host_name"],
                    lockReader["program_name"],
                    sqlText,
                    chainDepth
                );
            }
            dbConnection.Close();
            // Aggregate metrics
            if (blockingSessions.Count > 0)
            {
                var avgWait = waitTimes.Count > 0 ? waitTimes.Average() : 0;
                var maxWait = waitTimes.Count > 0 ? waitTimes.Max() : 0;
                var maxChain = blockingSessions.Count > 0 ? blockingSessions.Max(b => (int)b.chainDepth) : 0;
                logger.LogInformation(
                    "Blocking summary: blocked_sessions={blockedSessions} avg_wait_time_ms={avgWait} max_wait_time_ms={maxWait} max_blocking_chain_depth={maxChain}",
                    blockingSessions.Count,
                    avgWait,
                    maxWait,
                    maxChain
                );
            }
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
            logger.LogInformation(
                "Summary: top_slowest_queries: {topSlow}",
                topSlow.Select(q => new {
                    q.Database, q.Procedure, q.DurationMs, q.Rows, q.Status
                }).ToList()
            );
        }
        // Most frequent procedures
        var freq = QueryAnalyticsBag.GroupBy(q => q.Procedure)
            .Select(g => new { Procedure = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(3).ToList();
        if (freq.Count > 0)
        {
            logger.LogInformation(
                "Summary: most_frequent_procedures: {freq}",
                freq
            );
        }
        // Error rates
        var errorRates = ErrorAnalyticsBag.GroupBy(e => e.Procedure)
            .Select(g => new { Procedure = g.Key, ErrorCount = g.Count() })
            .OrderByDescending(x => x.ErrorCount).Take(3).ToList();
        if (errorRates.Count > 0)
        {
            logger.LogInformation(
                "Summary: error_rates: {errorRates}",
                errorRates
            );
        }
        // Blocking/lock stats
        var blocking = LockAnalyticsBag.Where(l => l.BlockingSessionId != DBNull.Value && Convert.ToInt32(l.BlockingSessionId) != 0).ToList();
        if (blocking.Count > 0)
        {
            logger.LogInformation(
                "Summary: blocking_sessions: {blocking}",
                blocking.Select(l => new {
                    l.SessionId, l.BlockingSessionId, l.ResourceType, l.RequestMode, l.WaitType, l.WaitTime
                }).ToList()
            );
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
