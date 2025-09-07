using Spectre.Console;

namespace PTConsole
{
    public class GuiContext
    {
        public Layout Layout { get; private set; }

        private Layout _contentLayout;
        private Layout _inputLayout;

        private string _input = "[blue]>[/] ";

        public GuiContext()
        {
            // Build Main Layout
            _contentLayout = new Layout("Content");
            _inputLayout = new Layout("Input");

            // Set Size
            _inputLayout.Size(3);

            Layout = new Layout("Root")
                .SplitRows(
                    _contentLayout,
                    _inputLayout);
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
                var prefixLength = _input.Length;
                while (true)
                {                    
                    var key = Console.ReadKey();

                    //if(key.Key == ConsoleKey.Backspace && _input.Length <= prefixLength)
                    //{
                    //    _input.Replace("\b", "");
                    //}

                    _input += key.KeyChar;
                }
            });
            
            await AnsiConsole.Live(Layout)
                .AutoClear(false)
                .Cropping(VerticalOverflowCropping.Bottom)
                .Overflow(VerticalOverflow.Crop)
                .StartAsync(async ctx =>
                {
                    while (true)
                    {
                        _inputLayout.Update(CreateInputPanel());
                        _contentLayout.Update(CreateContentPanel());
                        ctx.Refresh();
                        
                        Console.SetCursorPosition(3, 24);
                        //await Task.Delay(1000);
                    }
                });
        }
    }
}
