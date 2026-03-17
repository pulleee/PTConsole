using Spectre.Console;
using PTConsole.UI;
using PTConsole.UI.Layouts;
using PTConsole.UI.Panels;
using PTConsole.UI.Panels.Interfaces;

namespace PTConsole
{
    public class GuiContext
    {
        private readonly RootLayout _root = new RootLayout();
        private readonly ContentLayout _contentLayout = new ContentLayout();

        private int _lastWidth;
        private int _lastHeight;

        private readonly GuiCommandDispatcher _dispatcher;
        private readonly OutputPanel _outputPanel;
        private readonly InputPanel _inputPanel;

        public GuiContext(GuiCommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            // Panels
            _outputPanel = new OutputPanel(dispatcher.Console);
            _inputPanel = new InputPanel(dispatcher);

            // Content layout starts with the splash screen
            _contentLayout.SetPanel(new SplashPanel());

            // Root: Content + Output + Input, but Output is hidden by default
            _root.WithRows("Content", "Output", "Input");

            _root.RegisterLayout("Content", _contentLayout);
            _root.RegisterPanel("Output", _outputPanel);
            _root.RegisterPanel("Input", _inputPanel);

            _root.HideSlot("Output");
        }

        public void SetContent(IPanel panel) => _contentLayout.SetPanel(panel);

        public void ClearContent() => _contentLayout.ClearPanel();

        public void ShowSlot(string slotName) => _root.ShowSlot(slotName);

        public void HideSlot(string slotName) => _root.HideSlot(slotName);

        public bool IsSlotVisible(string slotName) => _root.IsSlotVisible(slotName);

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
                _root.Update(force: true);

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

                        _root.Update(force: true);
                    }
                    else
                    {
                        _root.Update();
                    }

                    Console.Write("\x1b[?2026h\x1b[H");
                    AnsiConsole.Write(_root.Layout);
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
