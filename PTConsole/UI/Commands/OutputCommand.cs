using LazyUI;
using LazyUI.Layouts;
using Spectre.Console.Cli;

namespace PTConsole.UI.Commands;

public class OutputCommand : Command<OutputCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    private readonly GuiContext _guiContext;

    public OutputCommand(GuiContext guiContext)
    {
        _guiContext = guiContext;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (_guiContext.IsSlotVisible(LazyLayout.OUTPUT_NAME))
        {
            _guiContext.HideSlot(LazyLayout.OUTPUT_NAME);
        }
        else
        {
            _guiContext.ShowSlot(LazyLayout.OUTPUT_NAME);
        }

        return 0;
    }
}
