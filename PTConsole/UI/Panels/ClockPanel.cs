using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels;

public class ClockPanel : AbstractRenderable, IHasDirtyState
{
    public BoxBorder Border { get; set; } = BoxBorder.Square;
    public Padding Padding { get; set; } = new Padding(0, 0, 0, 0);

    private readonly FigletFont _font;
    private string _lastTimeString = "";

    public bool IsDirty => DateTime.Now.ToString() != _lastTimeString;

    public ClockPanel()
    {
        _font = FigletFont.Load("Resources\\graffiti.flf");
    }

    public override Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        _lastTimeString = DateTime.Now.ToString();
        var text = new FigletText(_font, _lastTimeString);

        var panel = new Panel(Align.Center(text, VerticalAlignment.Middle));
        panel.Expand();
        panel.Border = Border;
        panel.Padding = Padding;

        return ((IRenderable)panel).Render(options, maxWidth);
    }
}
