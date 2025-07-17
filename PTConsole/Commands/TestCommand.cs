using Spectre.Console.Cli;
using System.ComponentModel;

namespace PTConsole.Commands
{
    public sealed class TestCommand : Command<TestCommand.Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandOption("-n|--name <NAME>")]
            [Description("Test Output")]
            [DefaultValue("Test")]
            public string TestSetting { get; set; }
        }

        public TestCommand()
        {
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            Console.WriteLine(settings.TestSetting);
            return 0;
        }
    }
}
