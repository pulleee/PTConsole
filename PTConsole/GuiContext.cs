using Spectre.Console;
using PTConsole.UI;

namespace PTConsole
{
    public class GuiContext
    {
        public Layout Layout { get; private set; }

        private Layout _contentLayout;
        private Layout _outputLayout;
        private Layout _inputLayout;

        private int _lastWidth;
        private int _lastHeight;

        private readonly GuiCommandDispatcher _dispatcher;
        private readonly ContentPanel _contentPanel;
        private readonly OutputPanel _outputPanel;
        private readonly InputPanel _inputPanel;

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

            _contentPanel = new ContentPanel();
            _inputPanel = new InputPanel(dispatcher);
            _outputPanel = new OutputPanel(dispatcher.Console, GetOutputPanelInnerHeight);
        }

        private int GetOutputPanelInnerHeight()
        {
            var contentHeight = _contentLayout.Size ?? 0;
            var inputHeight = _inputLayout.Size ?? _inputPanel.GetPanelHeight();
            return Math.Max(1, Console.WindowHeight - contentHeight - inputHeight - 2);
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

                        switch (key.Key)
                        {
                            case ConsoleKey.PageUp:
                                _outputPanel.ScrollUp();
                                break;
                            case ConsoleKey.PageDown:
                                _outputPanel.ScrollDown();
                                break;
                            default:
                                _inputPanel.HandleKey(key);
                                break;
                        }
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

                    _inputLayout.Size(_inputPanel.GetPanelHeight());
                    _inputLayout.Update(_inputPanel.Render());
                    _contentLayout.Update(_contentPanel.Render());
                    _outputLayout.Update(_outputPanel.Render());

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
