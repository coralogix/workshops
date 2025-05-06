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
                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("dotnet8mssql-db"));
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri("http://localhost:4317");
                    });
                });
        });
        var logger = loggerFactory.CreateLogger<Program>();

        var baseConnectionString = "Server=localhost,1433;User Id=sa;Password=Toortoor9#;TrustServerCertificate=True;";
        string[] dbsToCheck = { "master", "TestDB" };
        const int INFO = 20;
        const int ERROR = 40;

        while (true)
        {
            try
            {
                using (var connection = new SqlConnection(baseConnectionString))
                {
                    connection.Open();
                    var logObjConnect = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        database = "server",
                        procedure_name = "ConnectToServer",
                        sql_statement = "OPEN CONNECTION"
                    };
                    var logJsonConnect = JsonSerializer.Serialize(logObjConnect);
                    logger.LogInformation(logJsonConnect);
                    Console.WriteLine(logJsonConnect);

                    foreach (var dbName in dbsToCheck)
                    {
                        ListStoredProceduresForDatabase(baseConnectionString, dbName, logger);
                    }

                    connection.Close();
                    var logObjDisconnect = new
                    {
                        timestamp = DateTime.UtcNow.ToString("o"),
                        severity = INFO,
                        database = "server",
                        procedure_name = "DisconnectFromServer",
                        sql_statement = "CLOSE CONNECTION"
                    };
                    var logJsonDisconnect = JsonSerializer.Serialize(logObjDisconnect);
                    logger.LogInformation(logJsonDisconnect);
                    Console.WriteLine(logJsonDisconnect);
                }
            }
            catch (Exception ex)
            {
                var logObjError = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = ERROR,
                    database = "server",
                    procedure_name = "ConnectToServer",
                    sql_statement = "OPEN CONNECTION",
                    exception = ex.Message
                };
                var logJsonError = JsonSerializer.Serialize(logObjError);
                logger.LogError(logJsonError);
                Console.WriteLine(logJsonError);
                Console.WriteLine($"[dotnetdmv] Exception: {ex.Message}");
            }
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before next loop
        }
    }

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
                var logObjDbConnect = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = INFO,
                    database = dbName,
                    procedure_name = "ConnectToDatabase",
                    sql_statement = "OPEN CONNECTION"
                };
                var logJsonDbConnect = JsonSerializer.Serialize(logObjDbConnect);
                logger.LogInformation(logJsonDbConnect);
                Console.WriteLine(logJsonDbConnect);

                string procQuery = @"
                    SELECT p.name, m.definition
                    FROM sys.procedures p
                    JOIN sys.sql_modules m ON p.object_id = m.object_id
                    WHERE p.is_ms_shipped = 0
                    ORDER BY p.name;";
                using (var procCommand = new SqlCommand(procQuery, dbConnection))
                using (var procReader = procCommand.ExecuteReader())
                {
                    int procCount = 0;
                    while (procReader.Read())
                    {
                        procCount++;
                        string procName = procReader.GetString(0);
                        string procDef = procReader.GetString(1);
                        var logObjProc = new
                        {
                            timestamp = DateTime.UtcNow.ToString("o"),
                            severity = INFO,
                            database = dbName,
                            procedure_name = procName,
                            sql_statement = procDef
                        };
                        var logJsonProc = JsonSerializer.Serialize(logObjProc);
                        logger.LogInformation(logJsonProc);
                        Console.WriteLine(logJsonProc);
                    }
                    if (procCount == 0)
                    {
                        var logObjNone = new
                        {
                            timestamp = DateTime.UtcNow.ToString("o"),
                            severity = INFO,
                            database = dbName,
                            procedure_name = "(none)",
                            sql_statement = "No user stored procedures found."
                        };
                        var logJsonNone = JsonSerializer.Serialize(logObjNone);
                        logger.LogInformation(logJsonNone);
                        Console.WriteLine(logJsonNone);
                    }
                }
                dbConnection.Close();
                var logObjDbDisconnect = new
                {
                    timestamp = DateTime.UtcNow.ToString("o"),
                    severity = INFO,
                    database = dbName,
                    procedure_name = "DisconnectFromDatabase",
                    sql_statement = "CLOSE CONNECTION"
                };
                var logJsonDbDisconnect = JsonSerializer.Serialize(logObjDbDisconnect);
                logger.LogInformation(logJsonDbDisconnect);
                Console.WriteLine(logJsonDbDisconnect);
            }
        }
        catch (Exception ex)
        {
            var logObjError = new
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                severity = ERROR,
                database = dbName,
                procedure_name = "ListStoredProcedures",
                sql_statement = "SELECT ... FROM sys.procedures ...",
                exception = ex.Message
            };
            var logJsonError = JsonSerializer.Serialize(logObjError);
            logger.LogError(logJsonError);
            Console.WriteLine(logJsonError);
            Console.WriteLine($"[dotnetdmv] Exception in {dbName}: {ex.Message}");
        }
    }
}
