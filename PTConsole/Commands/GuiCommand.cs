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

        private readonly GuiContext _guiContext;

        public GuiCommand(IConfiguration configuration, GuiContext guiContext)
        {
            _configuration = configuration;
            _guiContext = guiContext;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var startup = new Startup(_configuration);
            var app = startup.CreateGuiCommandApp();

            var dispatcher = new GuiCommandDispatcher(app, _guiContext.Console);
            var gui = new GuiContext(dispatcher);

            await gui.Draw();

            return 0;
        }
    }
}
