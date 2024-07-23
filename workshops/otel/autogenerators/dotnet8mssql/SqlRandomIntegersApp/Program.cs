using System;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SqlRandomIntegersApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "localhost";  // Replace with your server name or IP address
            string username = "sa";  // Replace with your SQL Server username
            string password = "Toortoor9#";  // Replace with your SQL Server password
            string connectionString = $"Server={server};User Id={username};Password={password};";
            var logs = new List<LogEntry>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    LogAction(logs, "INFO", "Connecting to SQL Server", "Connection successful");

                    // Check if TestDB exists and drop it if it does
                    ExecuteSqlCommand(logs, connection, "IF DB_ID('TestDB') IS NOT NULL DROP DATABASE TestDB", "Checked and dropped existing 'TestDB' if it existed");

                    // Create TestDB database
                    ExecuteSqlCommand(logs, connection, "CREATE DATABASE TestDB", "Database 'TestDB' created successfully");

                    // Connect to TestDB database
                    connection.ChangeDatabase("TestDB");

                    // Create a test table in TestDB
                    ExecuteSqlCommand(logs, connection, "CREATE TABLE TestTable (ID INT PRIMARY KEY IDENTITY(1,1), Number INT)", "Test table created successfully in TestDB");

                    // Insert 100 random integers into TestTable
                    Random random = new Random();
                    for (int i = 0; i < 100; i++)
                    {
                        int randomNumber = random.Next();
                        using (SqlCommand command = new SqlCommand("INSERT INTO TestTable (Number) VALUES (@Number)", connection))
                        {
                            command.Parameters.AddWithValue("@Number", randomNumber);
                            command.ExecuteNonQuery();
                        }
                    }
                    LogAction(logs, "INFO", "Inserting 100 random integers into TestTable", "Inserted 100 random integers into TestTable");

                    // Query and print the inserted integers
                    using (SqlCommand command = new SqlCommand("SELECT * FROM TestTable", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var log = new LogEntry
                                {
                                    Timestamp = DateTime.UtcNow,
                                    Severity = "INFO",
                                    SqlStatement = "SELECT * FROM TestTable",
                                    Result = "Query result",
                                    ID = reader["ID"].ToString(),
                                    Number = reader["Number"].ToString()
                                };
                                Console.WriteLine(JsonConvert.SerializeObject(log, Formatting.Indented));
                                logs.Add(log);
                            }
                        }
                    }

                    // Delete the inserted integers
                    ExecuteSqlCommand(logs, connection, "DELETE FROM TestTable", "Deleted all integers from TestTable");

                    // Drop TestDB database
                    connection.ChangeDatabase("master");
                    ExecuteSqlCommand(logs, connection, "DROP DATABASE TestDB", "Database 'TestDB' deleted successfully");
                }
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", e.Message, "An error occurred");
            }
            finally
            {
                // Write logs to a file
                System.IO.File.WriteAllText("logs.json", JsonConvert.SerializeObject(logs, Formatting.Indented));
            }
        }

        static void ExecuteSqlCommand(List<LogEntry> logs, SqlConnection connection, string sql, string successMessage)
        {
            try
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    LogAction(logs, "INFO", sql, successMessage);
                }
            }
            catch (Exception e)
            {
                LogAction(logs, "ERROR", sql, e.Message);
                throw;
            }
        }

        static void LogAction(List<LogEntry> logs, string severity, string sql, string result, string? id = null, string? number = null)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Severity = severity,
                SqlStatement = sql,
                Result = result,
                ID = id,
                Number = number
            };
            Console.WriteLine(JsonConvert.SerializeObject(log, Formatting.Indented));
            logs.Add(log);
        }
    }

    class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? Severity { get; set; }
        public string? SqlStatement { get; set; }
        public string? Result { get; set; }
        public string? ID { get; set; }
        public string? Number { get; set; }
    }
}
