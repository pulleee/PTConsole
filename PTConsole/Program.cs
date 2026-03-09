using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;

namespace PTConsole
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            int returnCode = 0;
            ICommandApp app = CreateCommandApp();

            do
            {
                returnCode = await app.RunAsync(args);

            } while (returnCode > 0);
        }


        public static ICommandApp CreateCommandApp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    ["SQLite"] = "Data Source=Db.sqlite"
                })
                .Build();

            var app = new Startup(configuration).CreateCommandApp(); 

            return app;
        }
    }
}