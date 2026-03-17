using PTConsole.UI.Layouts.Interfaces;
using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;

namespace PTConsole.UI.Layouts;

public class ContentLayout : ILayout
{
    //TODO: For now we only handle one panel in the ContentLayout for splicity
    //      A layout structure with add/remove mechanics needs to be created
    private IPanel? _panel;
    private bool _panelChanged;

    public bool IsDirty => _panelChanged || (_panel?.IsDirty ?? false);

    public void SetPanel(IPanel panel)
    {
        _panel = panel;
        _panelChanged = true;
    }

    public void ClearPanel()
    {
        _panel = null;
        _panelChanged = true;
    }

    public Layout Render()
    {
        _panelChanged = false;

        var layout = new Layout("ContentInner");

        if (_panel != null)
            layout.Update(_panel.Render());
        else
            layout.Update(new Markup(""));

        return layout;
    }
}
