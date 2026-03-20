using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels;

public class SplashPanel : IRenderable
{
    private readonly FigletFont _font;

    public SplashPanel()
    {
        _font = FigletFont.Load("Resources\\graffiti.flf");
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var text = new FigletText(_font, "PTConsole");
        var content = Align.Center(text, VerticalAlignment.Middle);

        return ((IRenderable)content).Render(options, maxWidth);
    }
}
