using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using System.Text.Json;

class Program
{
    static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole()
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("dotnetdmv"));
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri("http://localhost:4317");
                        otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                });
        });
        var logger = loggerFactory.CreateLogger("dotnetdmv");

        try
        {
            var connectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
            string[] queries = new[]
            {
                "SELECT TOP 5 * FROM sys.dm_exec_requests",
                "SELECT TOP 5 * FROM sys.dm_exec_sessions",
                "SELECT TOP 5 * FROM sys.dm_os_wait_stats",
                "SELECT TOP 5 * FROM sys.dm_os_performance_counters"
            };

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Output logs for DMV queries
                foreach (var query in queries)
                {
                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var logTemplate = "DMV row";
                            var logValues = new List<object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var name = reader.GetName(i);
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                logTemplate += $" {{{name}}}";
                                logValues.Add(value ?? "");
                            }
                            logger.LogInformation(logTemplate, logValues.ToArray());
                        }
                    }
                }

                var cts = new System.Threading.CancellationTokenSource();
                Console.CancelKeyPress += (s, e) => {
                    e.Cancel = true;
                    cts.Cancel();
                };
                var token = cts.Token;
                while (!token.IsCancellationRequested)
                {
                    // --- Datadog-style Stored Procedure Performance Logging ---
                    using (var procStatsCmd = new SqlCommand(@"
                        SELECT
                            DB_NAME(database_id) as database_name,
                            OBJECT_NAME(object_id, database_id) as procedure_name,
                            execution_count,
                            total_worker_time,
                            total_elapsed_time,
                            total_logical_reads,
                            total_logical_writes,
                            total_physical_reads
                        FROM sys.dm_exec_procedure_stats
                        WHERE database_id = DB_ID()", connection))
                    using (var procStatsReader = procStatsCmd.ExecuteReader())
                    {
                        while (procStatsReader.Read())
                        {
                            logger.LogInformation(
                                "ProcStats {Database} {Procedure} {ExecutionCount} {WorkerTime} {ElapsedTime} {LogicalReads} {LogicalWrites} {PhysicalReads}",
                                procStatsReader["database_name"],
                                procStatsReader["procedure_name"],
                                procStatsReader["execution_count"],
                                procStatsReader["total_worker_time"],
                                procStatsReader["total_elapsed_time"],
                                procStatsReader["total_logical_reads"],
                                procStatsReader["total_logical_writes"],
                                procStatsReader["total_physical_reads"]
                            );
                        }
                    }

                    // Log execution stats and SQL text for running/cached queries
                    using (var queryStatsCmd = new SqlCommand(@"
                        SELECT
                            qs.sql_handle,
                            qs.execution_count,
                            qs.total_worker_time,
                            qs.total_elapsed_time,
                            qs.total_logical_reads,
                            qs.total_logical_writes,
                            qs.total_physical_reads,
                            st.text as sql_text
                        FROM sys.dm_exec_query_stats qs
                        CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
                        WHERE qs.execution_count > 0", connection))
                    using (var queryStatsReader = queryStatsCmd.ExecuteReader())
                    {
                        while (queryStatsReader.Read())
                        {
                            logger.LogInformation(
                                "QueryStats {SqlHandle} {ExecutionCount} {WorkerTime} {ElapsedTime} {LogicalReads} {LogicalWrites} {PhysicalReads} {SqlText}",
                                queryStatsReader["sql_handle"],
                                queryStatsReader["execution_count"],
                                queryStatsReader["total_worker_time"],
                                queryStatsReader["total_elapsed_time"],
                                queryStatsReader["total_logical_reads"],
                                queryStatsReader["total_logical_writes"],
                                queryStatsReader["total_physical_reads"],
                                queryStatsReader["sql_text"]
                            );
                        }
                    }

                    // --- Datadog-style Wait Stats Logging ---
                    using (var waitStatsCmd = new SqlCommand(@"
                        SELECT
                            wait_type,
                            waiting_tasks_count,
                            wait_time_ms,
                            max_wait_time_ms,
                            signal_wait_time_ms
                        FROM sys.dm_os_wait_stats
                        WHERE waiting_tasks_count > 0
                    ", connection))
                    using (var waitStatsReader = waitStatsCmd.ExecuteReader())
                    {
                        while (waitStatsReader.Read())
                        {
                            logger.LogInformation(
                                "WaitStats {WaitType} {WaitingTasksCount} {WaitTimeMs} {MaxWaitTimeMs} {SignalWaitTimeMs}",
                                waitStatsReader["wait_type"],
                                waitStatsReader["waiting_tasks_count"],
                                waitStatsReader["wait_time_ms"],
                                waitStatsReader["max_wait_time_ms"],
                                waitStatsReader["signal_wait_time_ms"]
                            );
                        }
                    }

                    // --- Datadog-style TempDB Usage Logging ---
                    using (var tempdbCmd = new SqlCommand(@"
                        SELECT
                            counter_name,
                            instance_name,
                            cntr_value
                        FROM sys.dm_os_performance_counters
                        WHERE object_name LIKE '%tempdb%' AND (
                            counter_name = 'Data File(s) Size (KB)'
                            OR counter_name = 'Log File(s) Size (KB)'
                            OR counter_name = 'Version Store Size (KB)'
                            OR counter_name = 'Free Space (KB)'
                        )
                    ", connection))
                    using (var tempdbReader = tempdbCmd.ExecuteReader())
                    {
                        while (tempdbReader.Read())
                        {
                            logger.LogInformation(
                                "TempDBUsage {CounterName} {InstanceName} {Value}",
                                tempdbReader["counter_name"],
                                tempdbReader["instance_name"],
                                tempdbReader["cntr_value"]
                            );
                        }
                    }

                    // --- Datadog-style Deadlock Logging (Deadlock count from performance counters) ---
                    using (var deadlockCmd = new SqlCommand(@"
                        SELECT
                            cntr_value
                        FROM sys.dm_os_performance_counters
                        WHERE counter_name = 'Number of Deadlocks/sec'
                            AND instance_name = '_Total'
                    ", connection))
                    using (var deadlockReader = deadlockCmd.ExecuteReader())
                    {
                        while (deadlockReader.Read())
                        {
                            logger.LogInformation(
                                "Deadlocks {DeadlocksPerSec}",
                                deadlockReader["cntr_value"]
                            );
                        }
                    }

                    // Wait 10 seconds before next iteration
                    System.Threading.Tasks.Task.Delay(10000, token).Wait(token);
                }

                // --- Stored Procedure Report ---
                // Step 1: Read all procedures into a list
                var procedures = new List<(string Name, string Definition)>();
                string procQuery = @"
                    SELECT p.name, m.definition
                    FROM sys.procedures p
                    JOIN sys.sql_modules m ON p.object_id = m.object_id
                    ORDER BY p.name;";
                using (var command = new SqlCommand(procQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string procName = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        string definition = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        procedures.Add((procName, definition));
                        // Print to console for debugging
                        Console.WriteLine($"Procedure: {procName}\nDefinition:\n{definition}\n");
                        // Log the definition directly for debugging
                        logger.LogInformation("Procedure definition for {ProcedureName}: {Definition}", procName, definition);
                    }
                }
                // Step 2: For each procedure, run stats queries
                foreach (var (procName, definition) in procedures)
                {
                    // Find all queries in the procedure (simple split on ';' for demo)
                    var queriesInProc = new List<string>();
                    foreach (var stmt in definition.Split(';'))
                    {
                        var trimmed = stmt.Trim();
                        if (!string.IsNullOrEmpty(trimmed) && (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)))
                            queriesInProc.Add(trimmed);
                    }
                    // Gather related stats: lock waits, blocking, execution stats
                    var stats = new Dictionary<string, object?>();
                    // Lock waits
                    using (var lockCmd = new SqlCommand($@"
                        SELECT COUNT(*) AS LockCount FROM sys.dm_tran_locks WHERE resource_associated_entity_id = OBJECT_ID(@procName)", connection))
                    {
                        lockCmd.Parameters.AddWithValue("@procName", procName);
                        var lockCount = lockCmd.ExecuteScalar();
                        stats["LockCount"] = lockCount;
                    }
                    // Blocking sessions (advanced: join with sys.dm_exec_sql_text and look for EXEC/EXECUTE procName)
                    using (var blockCmd = new SqlCommand($@"
                        SELECT COUNT(*) FROM sys.dm_exec_requests r
                        CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
                        WHERE r.blocking_session_id <> 0
                          AND (
                            t.text LIKE '%EXEC ' + @procName + '%'
                            OR t.text LIKE '%EXECUTE ' + @procName + '%'
                            OR t.text LIKE '% ' + @procName + '%'
                          )", connection))
                    {
                        blockCmd.Parameters.AddWithValue("@procName", procName);
                        var blockingCount = blockCmd.ExecuteScalar();
                        stats["BlockingCount"] = blockingCount;
                    }
                    // Execution stats
                    using (var execCmd = new SqlCommand($@"
                        SELECT TOP 1 execution_count, total_worker_time, total_elapsed_time FROM sys.dm_exec_procedure_stats WHERE object_id = OBJECT_ID(@procName)", connection))
                    {
                        execCmd.Parameters.AddWithValue("@procName", procName);
                        using (var execReader = execCmd.ExecuteReader())
                        {
                            if (execReader.Read())
                            {
                                stats["ExecutionCount"] = execReader.IsDBNull(0) ? 0 : execReader.GetInt64(0);
                                stats["TotalWorkerTime"] = execReader.IsDBNull(1) ? 0 : execReader.GetInt64(1);
                                stats["TotalElapsedTime"] = execReader.IsDBNull(2) ? 0 : execReader.GetInt64(2);
                            }
                        }
                    }
                    var procBody = new Dictionary<string, object?>
                    {
                        ["ProcedureName"] = procName,
                        ["Definition"] = definition,
                        ["Queries"] = queriesInProc,
                        ["Stats"] = stats
                    };
                    logger.LogInformation(
                        "Stored procedure report {ProcedureName} {Definition} {Queries} {LockCount} {BlockingCount}",
                        procBody["ProcedureName"],
                        procBody["Definition"],
                        JsonSerializer.Serialize(procBody["Queries"]),
                        ((Dictionary<string, object?>)procBody["Stats"])["LockCount"],
                        ((Dictionary<string, object?>)procBody["Stats"])["BlockingCount"]
                    );
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[dotnetdmv] Exception");
        }
    }
}
