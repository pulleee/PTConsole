using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;
using PTConsole.UI;

namespace PTConsole.Commands
{
    public sealed class GuiCommand : AsyncCommand<GuiCommand.Settings>
    {
        private readonly IConfiguration _configuration;

        public sealed class Settings : CommandSettings
        {
        }

        public GuiCommand(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var startup = new Startup(_configuration);
            var (app, console) = startup.CreateGuiCommandApp();

            var dispatcher = new GuiCommandDispatcher(app, console);
            var gui = new GuiContext(dispatcher);

            await gui.Draw();

            return 0;
        }
    }
}
