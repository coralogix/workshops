using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        var baseConnectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        try
        {
            using (var connection = new SqlConnection(baseConnectionString))
            {
                connection.Open();
                Console.WriteLine("Successfully connected to the server.");

                // Only check 'master' and 'TestDB' databases
                string[] dbsToCheck = { "master", "TestDB" };
                foreach (var dbName in dbsToCheck)
                {
                    Console.WriteLine($"\n--- Database: {dbName} ---");
                    ListStoredProceduresForDatabase(baseConnectionString, dbName);
                }

                connection.Close();
                Console.WriteLine("Disconnected from the server.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
        }
    }

    static void ListStoredProceduresForDatabase(string baseConnectionString, string dbName)
    {
        var dbConnectionString = baseConnectionString + $"Database={dbName};";
        try
        {
            using (var dbConnection = new SqlConnection(dbConnectionString))
            {
                dbConnection.Open();
                string procQuery = @"
                    SELECT p.name, m.definition
                    FROM sys.procedures p
                    JOIN sys.sql_modules m ON p.object_id = m.object_id
                    WHERE p.is_ms_shipped = 0
                    ORDER BY p.name;";
                using (var procCommand = new SqlCommand(procQuery, dbConnection))
                using (var procReader = procCommand.ExecuteReader())
                {
                    Console.WriteLine("Stored Procedures:");
                    int procCount = 0;
                    while (procReader.Read())
                    {
                        procCount++;
                        string procName = procReader.GetString(0);
                        string procDef = procReader.GetString(1);
                        Console.WriteLine($"Name: {procName}\nDefinition:\n{procDef}\n");
                    }
                    if (procCount == 0)
                    {
                        Console.WriteLine("(none)");
                    }
                    else
                    {
                        Console.WriteLine($"Total stored procedures found: {procCount}");
                    }
                }
                dbConnection.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[dotnetdmv] Exception in {dbName}: {ex.Message}");
        }
    }
}
