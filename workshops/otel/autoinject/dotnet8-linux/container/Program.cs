using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace HTTP_Test
{
    class Program
    {
        private static readonly IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConsole(options =>
                {
                    options.FormatterName = "customJson";
                });
                builder.AddConsoleFormatter<CustomJsonConsoleFormatter, ConsoleFormatterOptions>();
                builder.AddDebug();
            })
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
            .BuildServiceProvider();

        private static readonly ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            _ = host.RunAsync();

            await Task.Delay(2000);

            var random = new Random();
            
            while (true)
            {
                try
                {
                    await HTTP_GET();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred during HTTP GET request");
                }

                int delay = random.Next(250, 901);
                await Task.Delay(delay);
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton<ILogger<Program>>(logger);
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/", async context =>
                            {
                                logger.LogInformation("Received request: {Path}", context.Request.Path);
                                await context.Response.WriteAsync("Hello from the server!");
                            });

                            endpoints.MapGet("/hello", async context =>
                            {
                                logger.LogInformation("Received request: {Path}", context.Request.Path);
                                await context.Response.WriteAsync("Hello from /hello route!");
                            });
                        });
                    });
                    webBuilder.UseUrls("http://localhost:7080");
                });

        static async Task HTTP_GET()
        {
            var TARGETURL = "http://localhost:7080";

            logger.LogInformation("Starting GET request to {Url}", TARGETURL);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage? response = null;
                try
                {
                    response = await client.GetAsync(TARGETURL);
                    response.EnsureSuccessStatusCode();

                    logger.LogInformation("Response StatusCode: {StatusCode}, Content: {Content}", 
                        (int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "HTTP request to {Url} failed", TARGETURL);
                    throw;
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }
    }
}
