using PTConsole.Interfaces;
using PTConsole.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PTConsole.Commands
{
    public class CreateUserCommand : AsyncCommand<CreateUserCommand.Settings>
    {
        private readonly IRepository<User> _userRepository;

        public CreateUserCommand(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<alias>")]
            [Description("Alias/Username for the user")]
            public string Alias { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var user = new User
            {
                Alias = settings.Alias
            };

            await _userRepository.CreateAsync(user);
            AnsiConsole.MarkupLine($"[green]Created user '{settings.Alias}' successfully![/]");
            return 0;
        }
    }

    public class ListUsersCommand : AsyncCommand
    {
        private readonly IRepository<User> _userRepository;

        public ListUsersCommand(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            var users = await _userRepository.GetAllAsync();
            
            var table = new Table()
                .AddColumn("ID")
                .AddColumn("Alias")
                .AddColumn("Total Time Working");

            foreach (var user in users)
            {
                table.AddRow(
                    user.Id.ToString(),
                    user.Alias,
                    user.TotalTimeWorking.ToString()
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class DeleteUserCommand : AsyncCommand<DeleteUserCommand.Settings>
    {
        private readonly IRepository<User> _userRepository;

        public DeleteUserCommand(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<id>")]
            [Description("ID of the user to delete")]
            public int Id { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var user = await _userRepository.GetAsync(settings.Id);
            if (user == null)
            {
                AnsiConsole.MarkupLine($"[red]User with ID {settings.Id} not found![/]");
                return 1;
            }

            await _userRepository.DeleteAsync(user);
            AnsiConsole.MarkupLine($"[green]Deleted user '{user.Alias}' successfully![/]");
            return 0;
        }
    }
}