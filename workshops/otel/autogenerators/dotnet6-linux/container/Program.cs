using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP_Test
{
    class Program
    {
        static async Task Main()
        {
            while (true)
            {
                try
                {
                    await HTTP_GET();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                
                // Sleep 
                await Task.Delay(500);
            }
        }

        static async Task HTTP_GET()
        {
            var TARGETURL = "https://api.github.com/";

            Console.WriteLine("GET: " + TARGETURL);

            // Use a single HttpClient instance (consider HttpClientFactory in a real app)
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(TARGETURL);
                response.EnsureSuccessStatusCode(); // Check for success or throw an exception
                HttpContent content = response.Content;

                // Process content here if needed
                Console.WriteLine("Response StatusCode: " + (int)response.StatusCode);
            }
        }
    }
}