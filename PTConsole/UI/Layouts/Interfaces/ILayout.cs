using Spectre.Console;

namespace PTConsole.UI.Layouts.Interfaces
{
    public interface ILayout
    {
        bool IsDirty { get; }
        Layout Render();
        int? Size => null;
    }
}
