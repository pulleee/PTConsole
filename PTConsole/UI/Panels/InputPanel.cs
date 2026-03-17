using System.Text;
using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;
using TextCopy;

namespace PTConsole.UI.Panels;

public class InputPanel : IPanel
{
    private readonly GuiCommandDispatcher _dispatcher;

    public BoxBorder Border { get; set; } = BoxBorder.None;
    public Padding Padding { get; set; } = new Padding(0, 0, 0, 0);

    private string _userInput = "";
    private int _cursorPosition;
    private int? _selectionAnchor;
    private bool _dirty = true;

    public bool IsDirty => _dirty;

    public InputPanel(GuiCommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);
        bool shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

        switch (key.Key)
        {
            case ConsoleKey.Enter:
                if (!string.IsNullOrWhiteSpace(_userInput))
                {
                    var input = _userInput;
                    _userInput = "";
                    _cursorPosition = 0;
                    _selectionAnchor = null;

                    _dispatcher.Console.Write(
                        new Markup($"\n[bold grey]> {Markup.Escape(input)}[/]\n"));

                    _dispatcher.Console.Profile.Width = Math.Max(10, Console.WindowWidth - 6);

                    _ = Task.Run(async () =>
                    {
                        await _dispatcher.DispatchAsync(input);
                    });
                }
                break;

            case ConsoleKey.LeftArrow:
                if (shift)
                {
                    EnsureAnchor();
                    if (_cursorPosition > 0) _cursorPosition--;
                }
                else
                {
                    if (_selectionAnchor != null)
                    {
                        var (s, _) = GetSelectionRange();
                        _cursorPosition = s;
                        ClearSelection();
                    }
                    else if (_cursorPosition > 0)
                        _cursorPosition--;
                }
                break;

            case ConsoleKey.RightArrow:
                if (shift)
                {
                    EnsureAnchor();
                    if (_cursorPosition < _userInput.Length) _cursorPosition++;
                }
                else
                {
                    if (_selectionAnchor != null)
                    {
                        var (_, e) = GetSelectionRange();
                        _cursorPosition = e;
                        ClearSelection();
                    }
                    else if (_cursorPosition < _userInput.Length)
                        _cursorPosition++;
                }
                break;

            case ConsoleKey.Home:
                if (shift) EnsureAnchor();
                else ClearSelection();
                _cursorPosition = 0;
                break;

            case ConsoleKey.End:
                if (shift) EnsureAnchor();
                else ClearSelection();
                _cursorPosition = _userInput.Length;
                break;

            case ConsoleKey.A when ctrl:
                _selectionAnchor = 0;
                _cursorPosition = _userInput.Length;
                break;

            case ConsoleKey.C when ctrl:
                if (_selectionAnchor != null)
                {
                    var (s, e) = GetSelectionRange();
                    ClipboardService.SetText(_userInput[s..e]);
                }
                break;

            case ConsoleKey.V when ctrl:
                var clip = ClipboardService.GetText();
                if (clip != null)
                {
                    clip = new string(clip.Where(c => !char.IsControl(c)).ToArray());
                    if (_selectionAnchor != null)
                    {
                        var (s, e) = GetSelectionRange();
                        _userInput = _userInput[..s] + clip + _userInput[e..];
                        _cursorPosition = s + clip.Length;
                        _selectionAnchor = null;
                    }
                    else
                    {
                        _userInput = _userInput[.._cursorPosition] + clip + _userInput[_cursorPosition..];
                        _cursorPosition += clip.Length;
                    }
                }
                break;

            case ConsoleKey.Backspace:
                if (_selectionAnchor != null)
                {
                    DeleteSelection();
                }
                else if (_cursorPosition > 0)
                {
                    _userInput = _userInput[..(_cursorPosition - 1)] + _userInput[_cursorPosition..];
                    _cursorPosition--;
                }
                break;

            case ConsoleKey.Delete:
                if (_selectionAnchor != null)
                {
                    DeleteSelection();
                }
                else if (_cursorPosition < _userInput.Length)
                {
                    _userInput = _userInput[.._cursorPosition] + _userInput[(_cursorPosition + 1)..];
                }
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    if (_selectionAnchor != null)
                    {
                        var (s, e) = GetSelectionRange();
                        _userInput = _userInput[..s] + key.KeyChar + _userInput[e..];
                        _cursorPosition = s + 1;
                        _selectionAnchor = null;
                    }
                    else
                    {
                        _userInput = _userInput[.._cursorPosition] + key.KeyChar + _userInput[_cursorPosition..];
                        _cursorPosition++;
                    }
                }
                break;
        }

        _dirty = true;
    }

    public int GetPanelHeight()
    {
        var innerWidth = Math.Max(1, Console.WindowWidth - 2);
        var visibleLength = 2 + _userInput.Length + 1;
        var lines = (int)Math.Ceiling((double)visibleLength / innerWidth);
        return Math.Max(1, lines) + 2;
    }

    public IRenderable Render()
    {
        _dirty = false;

        var markup = BuildInputMarkup();
        var panel = new Panel(Align.Left(new Markup(markup), VerticalAlignment.Middle));
        panel.Border = Border;
        panel.Padding = Padding;

        return panel;
    }

    private (int start, int end) GetSelectionRange()
    {
        if (_selectionAnchor == null)
            return (-1, -1);
        var a = _selectionAnchor.Value;
        var b = _cursorPosition;
        return (Math.Min(a, b), Math.Max(a, b));
    }

    private void DeleteSelection()
    {
        var (start, end) = GetSelectionRange();
        if (start < 0) return;
        _userInput = _userInput[..start] + _userInput[end..];
        _cursorPosition = start;
        _selectionAnchor = null;
    }

    private void ClearSelection() => _selectionAnchor = null;

    private void EnsureAnchor()
    {
        _selectionAnchor ??= _cursorPosition;
    }

    private string BuildInputMarkup()
    {
        var sb = new StringBuilder("[blue]>[/] ");
        var (selStart, selEnd) = GetSelectionRange();

        for (int i = 0; i <= _userInput.Length; i++)
        {
            bool isCursor = i == _cursorPosition;
            bool isSelected = selStart >= 0 && i >= selStart && i < selEnd;
            char c = i < _userInput.Length ? _userInput[i] : ' ';
            var escaped = Markup.Escape(c.ToString());

            if (isCursor && isSelected)
                sb.Append($"[black on white]{escaped}[/]");
            else if (isCursor)
                sb.Append($"[invert]{escaped}[/]");
            else if (isSelected)
                sb.Append($"[white on blue]{escaped}[/]");
            else if (i < _userInput.Length)
                sb.Append(escaped);
        }

        return sb.ToString();
    }
}
