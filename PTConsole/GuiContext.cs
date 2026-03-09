using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;
using PTConsole.UI;
using TextCopy;

namespace PTConsole
{
    public class GuiContext
    {
        public Layout Layout { get; private set; }

        private Layout _contentLayout;
        private Layout _outputLayout;
        private Layout _inputLayout;

        private string _userInput = "";
        private int _cursorPosition;
        private int? _selectionAnchor;

        private int _lastWidth;
        private int _lastHeight;

        private int _outputScrollOffset; // 0 = bottom (auto-scroll), positive = scrolled up
        private int _lastCapturedCount;

        private readonly GuiCommandDispatcher _dispatcher;

        public GuiContext(GuiCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            _contentLayout = new Layout("Content");
            _outputLayout = new Layout("Output");
            _inputLayout = new Layout("Input");

            Layout = new Layout("Root")
                .SplitRows(
                    _contentLayout,
                    _outputLayout,
                    _inputLayout);
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
                // skip trailing space when cursor is not at end
            }

            return sb.ToString();
        }

        private int GetInputPanelHeight()
        {
            var innerWidth = Math.Max(1, Console.WindowWidth - 2);
            // +1 for cursor block at end
            var visibleLength = 2 + _userInput.Length + 1;
            var lines = (int)Math.Ceiling((double)visibleLength / innerWidth);
            return Math.Max(1, lines) + 2;
        }

        public Panel CreateContentPanel()
        {
            var font = FigletFont.Load("Resources\\graffiti.flf");
            var text = new FigletText(font, DateTime.Now.ToString());

            var panel = new Panel(Align.Center(text, VerticalAlignment.Middle));
            panel.Expand();
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(0, 0, 0, 0);
            //panel.NoBorder();

            return panel;
        }

        private int GetOutputPanelInnerHeight()
        {
            var contentHeight = _contentLayout.Size ?? 0;
            var inputHeight = _inputLayout.Size ?? GetInputPanelHeight();
            // total height minus content, input, and 2 for output panel borders
            return Math.Max(1, Console.WindowHeight - contentHeight - inputHeight - 2);
        }

        public Panel CreateOutputPanel()
        {
            var captured = _dispatcher.Console.GetCaptured();

            // Auto-scroll when new output arrives and we're at the bottom
            if (captured.Count != _lastCapturedCount)
            {
                if (_outputScrollOffset == 0 || _lastCapturedCount == 0)
                    _outputScrollOffset = 0; // stay at bottom
                else
                    _outputScrollOffset += captured.Count - _lastCapturedCount; // keep position
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
                var innerHeight = GetOutputPanelInnerHeight();

                // Clamp scroll offset
                var maxOffset = Math.Max(0, captured.Count - innerHeight);
                _outputScrollOffset = Math.Clamp(_outputScrollOffset, 0, maxOffset);

                // Slice visible window
                var startIndex = captured.Count - innerHeight - _outputScrollOffset;
                var count = innerHeight;

                if (startIndex < 0)
                {
                    count += startIndex;
                    startIndex = 0;
                }
                count = Math.Min(count, captured.Count - startIndex);

                var visible = captured.GetRange(startIndex, Math.Max(0, count));
                content = visible.Count > 0 ? new Rows(visible) : new Markup("");

                if (_outputScrollOffset > 0)
                    scrollIndicator = $" [dim](+{_outputScrollOffset} below)[/]";
            }

            var panel = new Panel(content);
            panel.Expand();
            panel.Header = new PanelHeader($" Output{scrollIndicator} ");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(1, 0, 1, 0);
            return panel;
        }

        public Panel CreateInputPanel()
        {
            var markup = BuildInputMarkup();
            var panel = new Panel(Align.Left(new Markup(markup), VerticalAlignment.Middle));
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(0, 0, 0, 0);

            return panel;
        }

        private void HandleKey(ConsoleKeyInfo key)
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

                        _dispatcher.Console.Profile.Width = Math.Max(10, System.Console.WindowWidth - 6);

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
                        // Sanitize: strip control chars except nothing
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

                case ConsoleKey.PageUp:
                    _outputScrollOffset += GetOutputPanelInnerHeight() / 2;
                    break;

                case ConsoleKey.PageDown:
                    _outputScrollOffset = Math.Max(0, _outputScrollOffset - GetOutputPanelInnerHeight() / 2);
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
        }

        public async Task Draw()
        {
            Console.Write("\x1b[?1049h");
            Console.Write("\x1b[2J\x1b[H");
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = true;

            try
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        HandleKey(key);
                    }
                });

                _lastWidth = Console.WindowWidth;
                _lastHeight = Console.WindowHeight;

                while (!_dispatcher.ExitRequested)
                {
                    var w = Console.WindowWidth;
                    var h = Console.WindowHeight;
                    if (w != _lastWidth || h != _lastHeight)
                    {
                        _lastWidth = w;
                        _lastHeight = h;
                        Console.Write("\x1b[2J");
                    }

                    _inputLayout.Size(GetInputPanelHeight());
                    _inputLayout.Update(CreateInputPanel());
                    _contentLayout.Update(CreateContentPanel());
                    _outputLayout.Update(CreateOutputPanel());

                    Console.Write("\x1b[?2026h\x1b[H");
                    AnsiConsole.Write(Layout);
                    Console.Write("\x1b[0J\x1b[?2026l");

                    await Task.Delay(7);
                }
            }
            finally
            {
                Console.TreatControlCAsInput = false;
                Console.Write("\x1b[?1049l");
                Console.CursorVisible = true;
            }
        }
    }
}
