using System;
using Microsoft.Data.SqlClient;
using OpenTelemetry.Trace;

namespace SqlServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "localhost";  // Replace with your server name or IP address
            string username = "sa";  // Replace with your SQL Server username
            string password = "Toortoor9#";  // Replace with your SQL Server password
            string connectionString = $"Server={server};User Id={username};Password={password};TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Connection successful");

                    // Check if TestDB exists and drop it if it does
                    ExecuteSqlCommand(connection, "IF DB_ID('TestDB') IS NOT NULL DROP DATABASE TestDB");
                    Console.WriteLine("Checked and dropped existing 'TestDB' if it existed");

                    // Create TestDB database
                    ExecuteSqlCommand(connection, "CREATE DATABASE TestDB");
                    Console.WriteLine("Database 'TestDB' created successfully");

                    // List all databases to verify TestDB creation
                    ListDatabases(connection, "Databases after creation:");

                    // Connect to TestDB database
                    connection.ChangeDatabase("TestDB");

                    // Create a test table in TestDB
                    ExecuteSqlCommand(connection, @"
                        CREATE TABLE TestTable (
                            ID INT PRIMARY KEY,
                            Name NVARCHAR(50)
                        )");
                    Console.WriteLine("Test table created successfully in TestDB");

                    // Insert test data into TestTable
                    ExecuteSqlCommand(connection, "INSERT INTO TestTable (ID, Name) VALUES (1, 'TestName')");
                    Console.WriteLine("Test data inserted successfully in TestDB");

                    // Query test data from TestTable
                    QueryTestTable(connection);

                    // Close connection to TestDB before dropping it
                    connection.ChangeDatabase("master");

                    // Drop TestDB database
                    ExecuteSqlCommand(connection, "DROP DATABASE TestDB");
                    Console.WriteLine("Database 'TestDB' deleted successfully");

                    // List all databases to verify TestDB deletion
                    ListDatabases(connection, "Databases after deletion:");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }

        static void ExecuteSqlCommand(SqlConnection connection, string sql)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        static void ListDatabases(SqlConnection connection, string message)
        {
            using (SqlCommand command = new SqlCommand("SELECT name FROM sys.databases;", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Console.WriteLine(message);
                    while (reader.Read())
                    {
                        Console.WriteLine(reader["name"]);
                    }
                }
            }
        }

        static void QueryTestTable(SqlConnection connection)
        {
            using (SqlCommand command = new SqlCommand("SELECT * FROM TestTable", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["ID"]}, Name: {reader["Name"]}");
                    }
                }
            }
        }
    }
}
