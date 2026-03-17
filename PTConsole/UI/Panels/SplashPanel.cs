using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels;

public class SplashPanel : IPanel
{
    public BoxBorder Border { get; set; } = BoxBorder.None;
    public Padding Padding { get; set; } = new Padding(0, 0, 0, 0);

    private readonly FigletFont _font;
    private IRenderable _cached;

    public bool IsDirty => false;

    public SplashPanel()
    {
        _font = FigletFont.Load("Resources\\graffiti.flf");
        var text = new FigletText(_font, "PTConsole");
        _cached = Align.Center(text, VerticalAlignment.Middle);
    }

    public IRenderable Render() => _cached;
}
