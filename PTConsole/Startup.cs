using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Commands;
using PTConsole.Infrastructure;
using PTConsole.Interfaces;
using PTConsole.Models;
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

        /// <summary>
        /// Creates a new instance of the CommandApp with configured services.
        /// This method sets up the dependency injection container and configures the command line application.        
        /// </summary>
        /// <returns>Instance of ICommandApp</returns>
        public CommandApp CreateCommandApp()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Default console for CLI mode (stdout)
            services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);

            var app = new CommandApp(new TypeRegistrar(services));
            app.Configure(config =>
            {
                config.AddCommand<GuiCommand>("gui");
                ConfigureCommands(config);
            });

            return app;
        }

        /// <summary>
        /// Creates a new instance of the CommandApp with configured services.
        /// This method sets up the dependency injection container and configures the command line application.
        /// It utilises a custom CapturingConsole to delegate the apps output into the GUI process.
        /// </summary>
        /// <returns>(CommandApp, CapturingConsole)</returns>
        public (ICommandApp app, CapturingConsole console) CreateGuiCommandApp()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Override the default IAnsiConsole with the capturing one
            var console = new CapturingConsole();
            services.AddSingleton<IAnsiConsole>(console);
            services.AddSingleton(console);

            var app = new CommandApp(new TypeRegistrar(services));
            app.Configure(config =>
            {
                config.Settings.Console = console;
                ConfigureCommands(config);
            });

            return (app, console);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dataSource = Configuration["SQLite"];

            // Contexts
            services.AddDbContext<DatabaseContext>(options => options.UseSqlite(dataSource));

            // Config
            services.AddSingleton<IConfiguration>(Configuration);

            // Services
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }

        private static void ConfigureCommands(IConfigurator config)
        {
            config.AddCommand<CreateClientCommand>("create");
            config.AddCommand<DeleteClientCommand>("delete");
            config.AddCommand<ListClientsCommand>("list");
        }
    }
}