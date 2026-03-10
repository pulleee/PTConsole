using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI;

public class OutputPanel : IPanel
{
    private readonly CapturingConsole _console;
    private int _scrollOffset; // 0 = bottom (auto-scroll), positive = scrolled up
    private int _lastCapturedCount;
    private readonly Func<int> _getInnerHeight;

    public bool IsDirty => _console.GetCaptured().Count != _lastCapturedCount;

    public OutputPanel(CapturingConsole console, Func<int> getInnerHeight)
    {
        _console = console;
        _getInnerHeight = getInnerHeight;
    }

    public void ScrollUp()
    {
        _scrollOffset += _getInnerHeight() / 2;
    }

    public void ScrollDown()
    {
        _scrollOffset = Math.Max(0, _scrollOffset - _getInnerHeight() / 2);
    }

    public IRenderable Render()
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

        IRenderable content;
        string scrollIndicator = "";

        if (captured.Count == 0)
        {
            content = new Markup("[dim]Type a command and press Enter.[/]");
        }
        else
        {
            var innerHeight = _getInnerHeight();

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
        panel.Header = new PanelHeader($" Output{scrollIndicator} ");
        panel.Border = BoxBorder.Rounded;
        panel.Padding = new Padding(1, 0, 1, 0);
        return panel;
    }
}
