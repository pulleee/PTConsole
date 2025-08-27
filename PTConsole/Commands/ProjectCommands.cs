using PTConsole.Interfaces;
using PTConsole.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Drawing;

namespace PTConsole.Commands
{
    public class CreateProjectCommand : AsyncCommand<CreateProjectCommand.Settings>
    {
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Client> _clientRepository;

        public CreateProjectCommand(IRepository<Project> projectRepository, IRepository<Client> clientRepository)
        {
            _projectRepository = projectRepository;
            _clientRepository = clientRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<name>")]
            [Description("Name of the project")]
            public string Name { get; set; } = string.Empty;

            [CommandOption("-d|--description")]
            [Description("Description of the project")]
            public string Description { get; set; } = string.Empty;

            [CommandOption("-c|--client-id")]
            [Description("ID of the client for this project")]
            public int? ClientId { get; set; }

            [CommandOption("--color")]
            [Description("Color for the project (hex format: #RRGGBB)")]
            public string? Color { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            Client? client = null;
            if (settings.ClientId.HasValue)
            {
                client = await _clientRepository.GetAsync(settings.ClientId.Value);
                if (client == null)
                {
                    AnsiConsole.MarkupLine($"[red]Client with ID {settings.ClientId} not found![/]");
                    return 1;
                }
            }

            var project = new Project
            {
                Name = settings.Name,
                Description = settings.Description,
                Client = client,
                Color = settings.Color != null ? ColorTranslator.FromHtml(settings.Color) : System.Drawing.Color.Blue,
            };

            await _projectRepository.CreateAsync(project);
            AnsiConsole.MarkupLine($"[green]Created project '{settings.Name}' successfully![/]");
            return 0;
        }
    }

    public class ListProjectsCommand : AsyncCommand
    {
        private readonly IRepository<Project> _projectRepository;

        public ListProjectsCommand(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            var projects = await _projectRepository.GetAllAsync();
            
            var table = new Table()
                .AddColumn("ID")
                .AddColumn("Name")
                .AddColumn("Description")
                .AddColumn("Client")
                .AddColumn("Duration")
                .AddColumn("Color");

            foreach (var project in projects)
            {
                table.AddRow(
                    project.Id.ToString(),
                    project.Name,
                    project.Description,
                    project.Client?.Name ?? "N/A",
                    project.Duration.ToString(),
                    $"[{ColorTranslator.ToHtml(project.Color)}]■[/]"
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class DeleteProjectCommand : AsyncCommand<DeleteProjectCommand.Settings>
    {
        private readonly IRepository<Project> _projectRepository;

        public DeleteProjectCommand(IRepository<Project> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<id>")]
            [Description("ID of the project to delete")]
            public int Id { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var project = await _projectRepository.GetAsync(settings.Id);
            if (project == null)
            {
                AnsiConsole.MarkupLine($"[red]Project with ID {settings.Id} not found![/]");
                return 1;
            }

            await _projectRepository.DeleteAsync(project);
            AnsiConsole.MarkupLine($"[green]Deleted project '{project.Name}' successfully![/]");
            return 0;
        }
    }
}