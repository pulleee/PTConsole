using System.Text;
using Spectre.Console;
using TextCopy;

namespace PTConsole
{
    public class GuiContext
    {
        public Layout Layout { get; private set; }

        private Layout _contentLayout;
        private Layout _inputLayout;

        private string _userInput = "";
        private int _cursorPosition;
        private int? _selectionAnchor;

        public GuiContext()
        {
            _contentLayout = new Layout("Content");
            _inputLayout = new Layout("Input");

            Layout = new Layout("Root")
                .SplitRows(
                    _contentLayout,
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
            panel.NoBorder();

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

                while (true)
                {
                    _inputLayout.Size(GetInputPanelHeight());
                    _inputLayout.Update(CreateInputPanel());
                    _contentLayout.Update(CreateContentPanel());

                    Console.Write("\x1b[H");
                    AnsiConsole.Write(Layout);
                    Console.Write("\x1b[H");

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
