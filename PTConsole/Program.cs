using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;

namespace PTConsole
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var startup = new Startup(BuildConfiguration());

            var isGui = args.Length > 0 && args[0].Equals("gui", StringComparison.OrdinalIgnoreCase);
            ICommandApp app = isGui ? startup.CreateGuiCommandApp() : startup.CreateCommandApp();

            await app.RunAsync(args);
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    ["SQLite"] = "Data Source=Db.sqlite"
                })
                .Build();
        }
    }
}