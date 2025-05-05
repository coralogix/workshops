using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
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
            foreach (var query in queries)
            {
                Console.WriteLine($"\n--- Results for: {query} ---");
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    // Print column names
                    for (int i = 0; i < reader.FieldCount; i++)
                        Console.Write($"{reader.GetName(i)}\t");
                    Console.WriteLine();

                    // Print rows
                    int rowCount = 0;
                    while (reader.Read() && rowCount < 5)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            Console.Write($"{reader[i]}\t");
                        Console.WriteLine();
                        rowCount++;
                    }
                }
            }
        }
        // --- New code to print stored procedures and their bodies ---
        Console.WriteLine("\n--- STORED PROCEDURE REPORT ---");
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
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
                    string procName = reader.GetString(0);
                    string definition = reader.GetString(1);
                    Console.WriteLine($"\nProcedure: {procName}\n--------------------\n{definition}\n");
                }
            }
        }

        // --- New code to print query performance metrics ---
        Console.WriteLine("\n--- QUERY PERFORMANCE METRICS REPORT ---");
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string perfQuery = @"
                SELECT 
                    DB_NAME(st.dbid) AS DatabaseName,
                    OBJECT_NAME(st.objectid, st.dbid) AS ObjectName,
                    qs.execution_count,
                    qs.total_worker_time AS TotalCPU,
                    qs.total_elapsed_time AS TotalDuration,
                    qs.total_logical_reads AS TotalReads,
                    qs.total_logical_writes AS TotalWrites,
                    qs.creation_time,
                    SUBSTRING(st.text, (qs.statement_start_offset/2) + 1,
                        ((CASE qs.statement_end_offset WHEN -1 THEN DATALENGTH(st.text) ELSE qs.statement_end_offset END - qs.statement_start_offset)/2) + 1) AS QueryText
                FROM sys.dm_exec_query_stats qs
                CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
                WHERE st.dbid = DB_ID() -- Only current DB
                ORDER BY qs.total_elapsed_time DESC;";
            using (var command = new SqlCommand(perfQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                int count = 0;
                while (reader.Read() && count < 50) // Limit to top 50 for readability
                {
                    string dbName = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string objName = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    long execCount = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                    long totalCPU = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                    long totalDuration = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                    long totalReads = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                    long totalWrites = reader.IsDBNull(6) ? 0 : reader.GetInt64(6);
                    DateTime creationTime = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7);
                    string queryText = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    Console.WriteLine($"\nQuery: {queryText}\nObject: {objName} | DB: {dbName}\nExec Count: {execCount} | Total CPU: {totalCPU} | Total Duration (µs): {totalDuration} | Reads: {totalReads} | Writes: {totalWrites} | Since: {creationTime}\n");
                    count++;
                }
            }
        }
    }
}
