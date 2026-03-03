using Spectre.Console;

namespace PTConsole
{
    public class GuiContext
    {
        public Layout Layout { get; private set; }

        private Layout _contentLayout;
        private Layout _inputLayout;

        private string _input = "[blue]>[/] ";
        private string _userInput = "";

        public GuiContext()
        {
            // Build Main Layout
            _contentLayout = new Layout("Content");
            _inputLayout = new Layout("Input");

            Layout = new Layout("Root")
                .SplitRows(
                    _contentLayout,
                    _inputLayout);
        }

        private int GetInputPanelHeight()
        {
            // "> " prefix (2 visible chars) + user text, wrapped to panel inner width (terminal - 2 border chars)
            var innerWidth = Math.Max(1, Console.WindowWidth - 2);
            var visibleLength = 2 + _userInput.Length;
            var lines = (int)Math.Ceiling((double)visibleLength / innerWidth) + 1;
            return Math.Max(1, lines) + 2; // +2 for top/bottom border
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
            var panel = new Panel(Align.Left(new Markup(_input), VerticalAlignment.Middle));
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(0, 0, 0, 0);

            return panel;
        }

        public async Task Draw()
        {
            // TODO: currently redraws after the read key, so it gets cleared and redrawn by the live context
            Task.Run(() =>
            {
                while (true)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (_userInput.Length > 0)
                            _userInput = _userInput[..^1];
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        _userInput += key.KeyChar;
                    }

                    _input = $"[blue]>[/] {_userInput}";
                }
            });
            
            await AnsiConsole.Live(Layout)
                .AutoClear(false)
                .Cropping(VerticalOverflowCropping.Bottom)
                .Overflow(VerticalOverflow.Ellipsis)
                .StartAsync(async ctx =>
                {
                    while (true)
                    {
                        _inputLayout.Size(GetInputPanelHeight());
                        _inputLayout.Update(CreateInputPanel());
                        _contentLayout.Update(CreateContentPanel());
                        ctx.Refresh();
                        
                        //Console.SetCursorPosition(3, 24);
                        //await Task.Delay(1000);
                    }
                });
        }
    }
}
