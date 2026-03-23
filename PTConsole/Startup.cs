using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Commands;
using PTConsole.Infrastructure;
using PTConsole.Interfaces;
using PTConsole.UI;
using PTConsole.UI.Commands;
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
        /// Creates a CommandApp for GUI mode.
        /// Registers CapturingConsole, an inner ICommandApp for dispatching GUI input,
        /// GuiCommandDispatcher, and GuiContext into DI so that GuiCommand
        /// receives a fully constructed GuiContext.
        /// </summary>
        public CommandApp CreateGuiCommandApp()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            services.AddSingleton<GuiContext>();
            services.AddSingleton<GuiCommandDispatcher>(sp =>
            {
                var innerServices = new ServiceCollection();
                var capturingConsole = new CapturingConsole();

                innerServices.AddSingleton<IAnsiConsole>(capturingConsole);

                // Load GuiContext lazily in inner application
                innerServices.AddSingleton(_ => sp.GetRequiredService<GuiContext>());

                var innerApp = new CommandApp(new TypeRegistrar(innerServices));
                innerApp.Configure(config =>
                {
                    config.Settings.Console = capturingConsole;

                    ConfigureCommands(config);
                    ConfigureGuiCommands(config);
                });

                return new GuiCommandDispatcher(innerApp, capturingConsole);
            });

            var app = new CommandApp(new TypeRegistrar(services));
            app.Configure(config =>
            {
                config.AddCommand<GuiCommand>("gui");
            });

            return app;
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

        public static void ConfigureGuiCommands(IConfigurator config)
        {
            config.AddCommand<OutputCommand>("output");
            config.AddCommand<ClockCommand>("clock");
        }
    }
}
