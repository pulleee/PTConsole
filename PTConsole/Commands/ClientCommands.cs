using LazyUI;
using PTConsole.Interfaces;
using PTConsole.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PTConsole.Commands
{
    public class CreateClientCommand : AsyncCommand<CreateClientCommand.Settings>
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IAnsiConsole _console;

        public CreateClientCommand(IRepository<Client> clientRepository, IAnsiConsole console)
        {
            _clientRepository = clientRepository;
            _console = console;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<name>")]
            [Description("Name of the client")]
            public string Name { get; set; } = string.Empty;

            [CommandArgument(1, "<alias>")]
            [Description("Alias/Short name of the client")]
            public string Alias { get; set; } = string.Empty;

            [CommandOption("-c|--color")]
            [Description("Color for the client (hex format: #RRGGBB)")]
            public string? ColorHex { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var client = new Client
            {
                Name = settings.Name,
                Alias = settings.Alias,
                Color = settings.ColorHex != null ? Color.FromHex(settings.ColorHex) : Color.Blue
            };

            await _clientRepository.CreateAsync(client);
            _console.MarkupLine($"[green]Created client '{settings.Name}' successfully![/]");
            return 0;
        }
    }

    public class ListClientsCommand : AsyncCommand
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IAnsiConsole _console;
        private readonly GuiContext _guiContext;

        public ListClientsCommand(IRepository<Client> clientRepository, IAnsiConsole console, GuiContext guiContext)
        {
            _clientRepository = clientRepository;
            _console = console;
            _guiContext = guiContext;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var clients = await _clientRepository.GetAllAsync();

            var table = new Table()
                .AddColumn("ID")
                .AddColumn("Name")
                .AddColumn("Alias")
                .AddColumn("Color");

            foreach (var client in clients)
            {
                table.AddRow(
                    client.Id.ToString(),
                    client.Name,
                    client.Alias,
                    $"[#{client.ColorHex}]{client.ColorHex}[/]"
                );
            }

            //_console.Write(table);
            _guiContext.SetContent(table);

            return 0;
        }
    }

    public class DeleteClientCommand : AsyncCommand<DeleteClientCommand.Settings>
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IAnsiConsole _console;

        public DeleteClientCommand(IRepository<Client> clientRepository, IAnsiConsole console)
        {
            _clientRepository = clientRepository;
            _console = console;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<id>")]
            [Description("ID of the client to delete")]
            public int Id { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetAsync(settings.Id);
            if (client == null)
            {
                _console.MarkupLine($"[red]Client with ID {settings.Id} not found![/]");
                return 1;
            }

            await _clientRepository.DeleteAsync(client);
            _console.MarkupLine($"[green]Deleted client '{client.Name}' successfully![/]");
            return 0;
        }
    }
}