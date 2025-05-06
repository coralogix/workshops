using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

class Program
{
    // Helper to compute SHA256 hash of a string
    static string ComputeSha256Hash(string rawData)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

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
                var queryStatsDict = new Dictionary<string, long>(); // hash -> exec count
                while (!token.IsCancellationRequested)
                {
                    // Log execution stats and SQL text for running/cached queries
                    queryStatsDict.Clear();
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
                            var sqlText = queryStatsReader["sql_text"]?.ToString() ?? "";
                            var queryHash = ComputeSha256Hash(sqlText);
                            var execCount = queryStatsReader["execution_count"] is long l ? l : Convert.ToInt64(queryStatsReader["execution_count"]);
                            // For metrics: keep max exec count per hash
                            if (!queryStatsDict.ContainsKey(queryHash) || execCount > queryStatsDict[queryHash])
                                queryStatsDict[queryHash] = execCount;
                            // Log with hash and full SQL text
                            logger.LogInformation(
                                "QueryStats {SqlHandle} {ExecutionCount} {WorkerTime} {ElapsedTime} {LogicalReads} {LogicalWrites} {PhysicalReads} {SqlText} {QueryHash}",
                                queryStatsReader["sql_handle"],
                                queryStatsReader["execution_count"],
                                queryStatsReader["total_worker_time"],
                                queryStatsReader["total_elapsed_time"],
                                queryStatsReader["total_logical_reads"],
                                queryStatsReader["total_logical_writes"],
                                queryStatsReader["total_physical_reads"],
                                sqlText,
                                queryHash
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
                    // Emit as structured log state (not message template)
                    logger.LogInformation(new {
                        ProcedureName = procName,
                        Definition = definition,
                        Queries = queriesInProc,
                        LockCount = stats.ContainsKey("LockCount") ? stats["LockCount"] : null,
                        BlockingCount = stats.ContainsKey("BlockingCount") ? stats["BlockingCount"] : null
                    });
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[dotnetdmv] Exception");
        }
    }
}
