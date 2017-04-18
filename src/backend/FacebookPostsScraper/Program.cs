using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace FacebookCivicInsights
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Boilerplate: start the HTTP server.
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
