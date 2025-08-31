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

        public CreateClientCommand(IRepository<Client> clientRepository)
        {
            _clientRepository = clientRepository;
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

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var client = new Client
            {
                Name = settings.Name,
                Alias = settings.Alias,
                Color = settings.ColorHex != null ? Color.FromHex(settings.ColorHex) : Color.Blue
            };

            await _clientRepository.CreateAsync(client);
            AnsiConsole.MarkupLine($"[green]Created client '{settings.Name}' successfully![/]");
            return 0;
        }
    }

    public class ListClientsCommand : AsyncCommand
    {
        private readonly IRepository<Client> _clientRepository;

        public ListClientsCommand(IRepository<Client> clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public override async Task<int> ExecuteAsync(CommandContext context)
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
                    $"[{client.Color.ToString()}]{client.ColorHex}[/]"
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class DeleteClientCommand : AsyncCommand<DeleteClientCommand.Settings>
    {
        private readonly IRepository<Client> _clientRepository;

        public DeleteClientCommand(IRepository<Client> clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<id>")]
            [Description("ID of the client to delete")]
            public int Id { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var client = await _clientRepository.GetAsync(settings.Id);
            if (client == null)
            {
                AnsiConsole.MarkupLine($"[red]Client with ID {settings.Id} not found![/]");
                return 1;
            }

            await _clientRepository.DeleteAsync(client);
            AnsiConsole.MarkupLine($"[green]Deleted client '{client.Name}' successfully![/]");
            return 0;
        }
    }
}