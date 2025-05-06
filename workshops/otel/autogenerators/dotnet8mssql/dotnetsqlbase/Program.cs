using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        var connectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Successfully connected to the database.");
                connection.Close();
                Console.WriteLine("Disconnected from the database.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
        }
    }
}
