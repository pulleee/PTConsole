using Spectre.Console.Rendering;

namespace PTConsole.UI;

public interface IPanel
{
    bool IsDirty { get; }
    IRenderable Render();
    int? Size => null;
}
