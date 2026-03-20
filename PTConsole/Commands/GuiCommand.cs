using Spectre.Console.Cli;

namespace PTConsole.Commands
{
    public sealed class GuiCommand : AsyncCommand<GuiCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
        }

        private readonly GuiContext _guiContext;

        public GuiCommand(GuiContext guiContext)
        {
            _guiContext = guiContext;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            await _guiContext.Draw();

            return 0;
        }
    }
}
