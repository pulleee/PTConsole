using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Commands;
using PTConsole.Infrastructure;
using PTConsole.Interfaces;
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
    }
}