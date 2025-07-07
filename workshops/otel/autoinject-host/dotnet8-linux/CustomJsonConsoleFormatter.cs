using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

public class CustomJsonConsoleFormatter : ConsoleFormatter
{
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomJsonConsoleFormatter() : base("customJson")
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var logRecord = new
        {
            logEntry.LogLevel,
            EventId = new { logEntry.EventId.Id, logEntry.EventId.Name },
            logEntry.Category,
            Message = logEntry.Formatter(logEntry.State, logEntry.Exception),
            Exception = logEntry.Exception != null ? new
            {
                logEntry.Exception.Message,
                logEntry.Exception.StackTrace
            } : null
        };

        string jsonString = JsonSerializer.Serialize(logRecord, _jsonOptions);
        textWriter.WriteLine(jsonString);
    }
}
