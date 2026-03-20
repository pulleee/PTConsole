using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels.Interfaces
{
    public abstract class AbstractRenderable : IRenderable
    {
        public IRenderable Render() => this;

        public abstract Measurement Measure(RenderOptions options, int maxWidth);

        public abstract IEnumerable<Segment> Render(RenderOptions options, int maxWidth);
    }
}
