using Spectre.Console;
using Spectre.Console.Rendering;
using PTConsole.UI;
using PTConsole.UI.Panels;

namespace PTConsole
{
    public class GuiContext
    {
        private const string CONTENT_NAME = "Content";
        private const string INPUT_NAME = "Input";
        private const string OUTPUT_NAME = "Output";

        private Layout _root;

        private int _lastWidth;
        private int _lastHeight;

        private IRenderable? _contentPanel;

        private readonly GuiCommandDispatcher _dispatcher;
        private readonly OutputPanel _outputPanel;
        private readonly InputPanel _inputPanel;

        public GuiContext(GuiCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            // Panels
            _outputPanel = new OutputPanel(dispatcher.Console);
            _inputPanel = new InputPanel(dispatcher);

            // Wrap into caching renderables
            var output = new CachingRenderable(_outputPanel);
            var input = new CachingRenderable(_inputPanel);

            // Build layout
            _root = buildLayout(new SplashPanel(), output, input);
        }

        public void SetContent(IRenderable content)
        {
            if(content is IHasDirtyState)
            {
                _contentPanel = new CachingRenderable(content);
            }
            else
            {
                _contentPanel = content;
            }

            _root[CONTENT_NAME].Update(_contentPanel);
        }

        public void ClearContent()
        {
            _contentPanel = null;
        }

        public void ShowSlot(string slotName)
        {
            _root[slotName].IsVisible = true;
        }

        public void HideSlot(string slotName)
        {
            _root[slotName].IsVisible = false;
        }

        public bool IsSlotVisible(string slotName) => _root[slotName].IsVisible;

        public async Task Draw()
        {
            Console.Write("\x1b[?1049h");
            Console.Write("\x1b[2J\x1b[H");
            Console.CursorVisible = false;
            Console.TreatControlCAsInput = true;

            try
            {
                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var key = Console.ReadKey(true);

                        bool ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);
                        bool shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

                        switch (key.Key)
                        {
                            case ConsoleKey.PageUp:
                                _outputPanel.ScrollUp(Console.WindowHeight);
                                break;
                            case ConsoleKey.PageDown:
                                _outputPanel.ScrollDown(Console.WindowHeight);
                                break;
                            case ConsoleKey.O when ctrl:
                                _root[OUTPUT_NAME].IsVisible = _root[OUTPUT_NAME].IsVisible ? false : true;
                                break;
                            default:
                                _inputPanel.HandleKey(key);
                                break;
                        }
                    }
                });

                _lastWidth = Console.WindowWidth;
                _lastHeight = Console.WindowHeight;

                // Force initial render
                AnsiConsole.Write(_root);

                while (!_dispatcher.ExitRequested)
                {
                    var w = Console.WindowWidth;
                    var h = Console.WindowHeight;
                    bool resized = w != _lastWidth || h != _lastHeight;

                    if (resized)
                    {
                        _lastWidth = w;
                        _lastHeight = h;
                    }

                    Console.Write("\x1b[?2026h\x1b[H");
                    AnsiConsole.Write(_root);
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

        private Layout buildLayout(IRenderable content, IRenderable output, IRenderable input, bool showOutput = true)
        {
            var layout = new Layout("Root")
                .SplitRows(
                    new Layout(CONTENT_NAME).Ratio(1),
                    new Layout(OUTPUT_NAME).Ratio(1),
                    new Layout(INPUT_NAME).Size(3));

            layout[CONTENT_NAME].Update(content);
            layout[OUTPUT_NAME].Update(output);
            layout[INPUT_NAME].Update(input);

            layout[OUTPUT_NAME].IsVisible = showOutput;

            return layout;
        }
    }
}
