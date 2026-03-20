using PTConsole.UI.Layouts.Interfaces;
using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;

namespace PTConsole.UI.Layouts;

public class ContentLayout : ILayout
{
    //TODO: For now we only handle one panel in the ContentLayout for splicity
    //      A layout structure with add/remove mechanics needs to be created
    private AbstractRenderable? _content;
    private bool _panelChanged;

    public bool IsDirty => _panelChanged || (_content?.IsDirty ?? false);

    public void SetContent(AbstractRenderable panel)
    {
        _content = panel;
        _panelChanged = true;
    }

    public void ClearContent()
    {
        _content = null;
        _panelChanged = true;
    }

    public Layout Render()
    {
        _panelChanged = false;

        var layout = new Layout("ContentInner");

        if (_content != null)
            layout.Update(_content.Render());
        else
            layout.Update(new Markup(""));

        return layout;
    }
}
