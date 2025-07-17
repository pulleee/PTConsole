using Spectre.Console.Cli;

namespace PTConsole.Commands
{
    public sealed class ConfigCommand : Command<ConfigCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
        }

        public ConfigCommand()
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {            
            return 0;
        }
    }
}
