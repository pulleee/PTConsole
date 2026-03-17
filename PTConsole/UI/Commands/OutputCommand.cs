using Spectre.Console;
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

    public override int Execute(CommandContext context, Settings settings)
    {
        if (_guiContext.IsSlotVisible("Output"))
        {
            _guiContext.HideSlot("Output");
        }
        else
        {
            _guiContext.ShowSlot("Output");
        }

        return 0;
    }
}
