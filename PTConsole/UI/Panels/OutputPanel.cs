using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels;

public class OutputPanel : IRenderable, IPanel
{
    public BoxBorder Border { get; set; } = BoxBorder.Square;
    public Padding Padding { get; set; } = new Padding(1, 0, 1, 0);

    private readonly CapturingConsole _console;
    private int _scrollOffset; // 0 = bottom (auto-scroll), positive = scrolled up
    private int _lastCapturedCount;

    public bool IsDirty => _console.GetCaptured().Count != _lastCapturedCount;

    public OutputPanel(CapturingConsole console)
    {
        _console = console;
    }

    public void ScrollUp(int visibleHeight)
    {
        var amount = Math.Max(1, visibleHeight / 2);
        _scrollOffset += amount;
    }

    public void ScrollDown(int visibleHeight)
    {
        var amount = Math.Max(1, visibleHeight / 2);
        _scrollOffset = Math.Max(0, _scrollOffset - amount);
    }

    // IPanel.Render - returns self since we are the renderable
    public IRenderable Render() => this;

    // IRenderable.Measure
    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    // IRenderable.Render
    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth)
    {
        var captured = _console.GetCaptured();

        if (captured.Count != _lastCapturedCount)
        {
            if (_scrollOffset == 0 || _lastCapturedCount == 0)
                _scrollOffset = 0;
            else
                _scrollOffset += captured.Count - _lastCapturedCount;
            _lastCapturedCount = captured.Count;
        }

        // Border takes 2 chars width, padding takes left+right
        var innerWidth = Math.Max(1, maxWidth - 2 - Padding.Left - Padding.Right);

        // Estimate available height from console, minus border top/bottom
        var totalHeight = options.Height ?? Console.WindowHeight;
        var innerHeight = Math.Max(1, totalHeight - 2 - Padding.Top - Padding.Bottom);

        IRenderable content;
        string scrollIndicator = "";

        if (captured.Count == 0)
        {
            content = new Markup("[dim]Type a command and press Enter.[/]");
        }
        else
        {
            var maxOffset = Math.Max(0, captured.Count - innerHeight);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, maxOffset);

            var startIndex = captured.Count - innerHeight - _scrollOffset;
            var count = innerHeight;

            if (startIndex < 0)
            {
                count += startIndex;
                startIndex = 0;
            }
            count = Math.Min(count, captured.Count - startIndex);

            var visible = captured.GetRange(startIndex, Math.Max(0, count));
            content = visible.Count > 0 ? new Rows(visible) : new Markup("");

            if (_scrollOffset > 0)
                scrollIndicator = $" [dim](+{_scrollOffset} below)[/]";
        }

        var panel = new Panel(content);
        panel.Expand();
        panel.Header = new PanelHeader($"{scrollIndicator}");
        panel.Border = Border;
        panel.Padding = Padding;

        // Delegate segment rendering to the composed Panel
        return ((IRenderable)panel).Render(options, maxWidth);
    }
}
