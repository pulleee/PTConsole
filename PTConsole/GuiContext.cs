using Spectre.Console;
using Spectre.Console.Rendering;
using PTConsole.UI;
using PTConsole.UI.Panels;

namespace PTConsole
{
    public class GuiContext
    {
        private readonly string CONTENT_NAME = "Content";
        private readonly string INPUT_NAME = "Input";
        private readonly string OUTPUT_NAME = "Output";

        private Layout _root;

        private int _lastWidth;
        private int _lastHeight;

        private readonly GuiCommandDispatcher _dispatcher;
        private readonly OutputPanel _outputPanel;
        private readonly InputPanel _inputPanel;

        private IRenderable? _contentPanel;
        private bool _contentChanged;

        public GuiContext(GuiCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            // Panels
            _outputPanel = new OutputPanel(dispatcher.Console);
            _inputPanel = new InputPanel(dispatcher);

            // Content starts with the splash screen
            _contentPanel = new SplashPanel();
            _contentChanged = true;

            _root = BuildLayout(showOutput: true);
        }

        public void SetContent(IRenderable content)
        {
            _contentPanel = content;
            _contentChanged = true;
        }

        public void ClearContent()
        {
            _contentPanel = null;
            _contentChanged = true;
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
                Task.Run(() =>
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

                            default:
                                _inputPanel.HandleKey(key);
                                break;
                        }
                    }
                });

                _lastWidth = Console.WindowWidth;
                _lastHeight = Console.WindowHeight;

                // Force initial render
                UpdateSlots(force: true);

                while (!_dispatcher.ExitRequested)
                {
                    var w = Console.WindowWidth;
                    var h = Console.WindowHeight;
                    bool resized = w != _lastWidth || h != _lastHeight;

                    if (resized)
                    {
                        _lastWidth = w;
                        _lastHeight = h;

                        Console.Write("\x1b[2J");

                        UpdateSlots(force: true);
                    }
                    else
                    {
                        UpdateSlots();
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

        private void UpdateSlots(bool force = false)
        {
            if (force || _contentChanged || (_contentPanel is IHasDirtyState ds && ds.IsDirty))
            {
                _root[CONTENT_NAME].Update(_contentPanel ?? new Markup(""));
                _contentChanged = false;
            }

            if (force || _outputPanel.IsDirty)
                _root[OUTPUT_NAME].Update(_outputPanel);

            if (force || _inputPanel.IsDirty)
                _root[INPUT_NAME].Update(_inputPanel);
        }

        private Layout BuildLayout(bool showOutput)
        {
            var layout = new Layout("Root")
                .SplitRows(
                    new Layout(CONTENT_NAME).Ratio(1),
                    new Layout(OUTPUT_NAME).Ratio(1),
                    new Layout(INPUT_NAME).Size(3));

            layout[OUTPUT_NAME].IsVisible = showOutput;

            return layout;
        }
    }
}
