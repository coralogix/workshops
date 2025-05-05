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
    }
}
