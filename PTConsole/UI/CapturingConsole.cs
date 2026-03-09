using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI;

public class CapturingConsole : IAnsiConsole
{
    private readonly IAnsiConsole _inner;
    private readonly List<IRenderable> _captured = new();
    private readonly object _lock = new();

    public CapturingConsole()
    {
        _inner = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Yes,
            ColorSystem = ColorSystemSupport.TrueColor,
            Out = new AnsiConsoleOutput(TextWriter.Null),
            Interactive = InteractionSupport.Yes,
        });
    }

    public Profile Profile => _inner.Profile;
    public IAnsiConsoleCursor Cursor => _inner.Cursor;
    public IAnsiConsoleInput Input => _inner.Input;
    public IExclusivityMode ExclusivityMode => _inner.ExclusivityMode;
    public RenderPipeline Pipeline => _inner.Pipeline;

    public void Clear(bool home)
    {
        lock (_lock)
        {
            _captured.Clear();
        }
    }

    public void Write(IRenderable renderable)
    {
        lock (_lock)
        {
            _captured.Add(renderable);
        }
    }

    public List<IRenderable> GetCaptured()
    {
        lock (_lock)
        {
            return new List<IRenderable>(_captured);
        }
    }
}
