using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Infrastructure;
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

        /*
         * Creates a new instance of the CommandApp with configured services.
         * This method sets up the dependency injection container and configures the command line application.
         *
         * @returns An instance of ICommandApp ready to run commands.
         */
        public static ICommandApp CreateCommandApp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    ["SQLite"] = "Data Source=Db.sqlite"
                })
                .Build();

            var startup = new Startup(configuration);

            var services = new ServiceCollection();
            startup.ConfigureServices(services);

            var app = new CommandApp(new TypeRegistrar(services));            

            startup.ConfigureCommands(app);            

            return app;
        }
    }
}