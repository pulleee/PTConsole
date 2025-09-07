using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace PTConsole.Commands
{
    public sealed class GuiCommand : AsyncCommand<GuiCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
        }

        public GuiCommand()
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var xd = new GuiContext();

            await xd.Draw();

            return 0;
        }
    }
}
