using PTConsole.Interfaces;
using PTConsole.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PTConsole.Commands
{
    public class CreateTaskCommand : AsyncCommand<CreateTaskCommand.Settings>
    {
        private readonly IRepository<WorkTask> _taskRepository;

        public CreateTaskCommand(IRepository<WorkTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<description>")]
            [Description("Description of the task")]
            public string Description { get; set; } = string.Empty;

            [CommandOption("-p|--priority")]
            [Description("Priority of the task (default: 0)")]
            public int Priority { get; set; } = 0;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var task = new WorkTask
            {
                Description = settings.Description,
                Priority = settings.Priority
            };

            await _taskRepository.CreateAsync(task);
            AnsiConsole.MarkupLine($"[green]Created task successfully![/]");
            return 0;
        }
    }

    public class ListTasksCommand : AsyncCommand
    {
        private readonly IRepository<WorkTask> _taskRepository;

        public ListTasksCommand(IRepository<WorkTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            var tasks = await _taskRepository.GetAllAsync();
            
            var table = new Table()
                .AddColumn("ID")
                .AddColumn("Description")
                .AddColumn("Priority")
                .AddColumn("Duration");

            foreach (var task in tasks)
            {
                table.AddRow(
                    task.Id.ToString(),
                    task.Description,
                    task.Priority.ToString(),
                    task.Duration.ToString()
                );
            }

            AnsiConsole.Write(table);
            return 0;
        }
    }

    public class DeleteTaskCommand : AsyncCommand<DeleteTaskCommand.Settings>
    {
        private readonly IRepository<WorkTask> _taskRepository;

        public DeleteTaskCommand(IRepository<WorkTask> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<id>")]
            [Description("ID of the task to delete")]
            public int Id { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var task = await _taskRepository.GetAsync(settings.Id);
            if (task == null)
            {
                AnsiConsole.MarkupLine($"[red]Task with ID {settings.Id} not found![/]");
                return 1;
            }

            await _taskRepository.DeleteAsync(task);
            AnsiConsole.MarkupLine($"[green]Deleted task successfully![/]");
            return 0;
        }
    }
}