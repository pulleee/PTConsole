using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI;

public class ContentPanel : IPanel
{
    private readonly FigletFont _font;
    private string _lastTimeString = "";
    private IRenderable _cached = null!;

    public bool IsDirty => DateTime.Now.ToString() != _lastTimeString;

    public ContentPanel()
    {
        _font = FigletFont.Load("Resources\\graffiti.flf");
        RebuildPanel();
    }

    public IRenderable Render()
    {
        var timeString = DateTime.Now.ToString();
        if (timeString != _lastTimeString)
            RebuildPanel();

        return _cached;
    }

    private void RebuildPanel()
    {
        _lastTimeString = DateTime.Now.ToString();
        var text = new FigletText(_font, _lastTimeString);

        var panel = new Panel(Align.Center(text, VerticalAlignment.Middle));
        panel.Expand();
        panel.Border = BoxBorder.Rounded;
        panel.Padding = new Padding(0, 0, 0, 0);

        _cached = panel;
    }
}
