using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Commands;
using PTConsole.Infrastructure;
using PTConsole.Interfaces;
using PTConsole.UI;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PTConsole
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var test = Configuration["SQLite"];

            // Contexts
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(test));

            // Services
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Default console for CLI mode (stdout)
            services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
            services.AddSingleton<IConfiguration>(Configuration);
        }

        public void ConfigureGuiServices(IServiceCollection services, CapturingConsole console)
        {
            // Override the default IAnsiConsole with the capturing one
            services.AddSingleton<IAnsiConsole>(console);
            services.AddSingleton(console);
        }

        public void ConfigureCommands(ICommandApp app)
        {
            app.Configure(config => config.AddCommand<GuiCommand>("gui"));

            app.Configure(config => config.AddBranch("client",
                client =>
                {
                    client.AddCommand<CreateClientCommand>("create");
                    client.AddCommand<DeleteClientCommand>("delete");
                    client.AddCommand<ListClientsCommand>("list");
                }));
        }

        public void ConfigureGuiCommands(ICommandApp app, CapturingConsole console)
        {
            app.Configure(config =>
            {
                config.Settings.Console = console;

                config.AddBranch("client", client =>
                {
                    client.AddCommand<CreateClientCommand>("create");
                    client.AddCommand<DeleteClientCommand>("delete");
                    client.AddCommand<ListClientsCommand>("list");
                });
            });
        }
    }
}