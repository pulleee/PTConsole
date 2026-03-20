using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels.Interfaces;

public interface IDirtyRenderable : IRenderable, IHasDirtyState
{
    IRenderable Render();
}
