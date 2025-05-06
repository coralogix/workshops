using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Text.Json;

class Program
{
    static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("dotnet8mssql-base"));
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri("http://localhost:4317");
                    });
                });
        });
        var logger = loggerFactory.CreateLogger<Program>();

        var connectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        var procedureName = "ConnectToDatabase";
        var sqlStatement = "OPEN CONNECTION";

        // Numeric severity: INFO=20, ERROR=40 (OpenTelemetry/Log conventions)
        const int INFO = 20;
        const int ERROR = 40;

        while (true)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var logObjOpen = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        procedure_name = procedureName,
                        sql_statement = sqlStatement
                    };
                    logger.LogInformation(JsonSerializer.Serialize(logObjOpen));
                    Console.WriteLine("Successfully connected to the database.");
                    connection.Close();
                    var logObjClose = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        procedure_name = procedureName,
                        sql_statement = "CLOSE CONNECTION"
                    };
                    logger.LogInformation(JsonSerializer.Serialize(logObjClose));
                    Console.WriteLine("Disconnected from the database.");
                }
            }
            catch (Exception ex)
            {
                var logObjError = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = ERROR,
                    procedure_name = procedureName,
                    sql_statement = sqlStatement,
                    exception = ex.Message
                };
                logger.LogError(JsonSerializer.Serialize(logObjError));
                Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
            }
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before next loop
        }
    }
}
