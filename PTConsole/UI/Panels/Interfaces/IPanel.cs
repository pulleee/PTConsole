using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels.Interfaces;

public interface IPanel
{
    BoxBorder Border { get; }
    Padding Padding { get; }
    bool IsDirty { get; }
    IRenderable Render();
    int? Size => null;
}
