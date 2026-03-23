using LazyUI;
using PTConsole.UI.Panels;
using Spectre.Console.Cli;

namespace PTConsole.UI.Commands;

public class ClockCommand : Command<ClockCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    private readonly GuiContext _guiContext;

    public ClockCommand(GuiContext guiContext)
    {
        _guiContext = guiContext;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var clockPanel = new ClockPanel();
        _guiContext.SetContent(clockPanel);

        return 0;
    }
}
