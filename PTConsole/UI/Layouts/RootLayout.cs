using PTConsole.UI.Layouts.Interfaces;
using PTConsole.UI.Panels.Interfaces;
using Spectre.Console;

namespace PTConsole.UI.Layouts;

public class RootLayout
{
    public Layout Layout { get; private set; }

    private readonly List<string> _slotOrder = new();
    private readonly Dictionary<string, Layout> _slots = new();
    private readonly Dictionary<string, int?> _slotSizes = new();
    private readonly HashSet<string> _visibleSlots = new();

    private readonly Dictionary<string, IPanel> _panels = new();
    private readonly Dictionary<string, ILayout> _layouts = new();

    private bool _structureDirty;

    public RootLayout()
    {
        Layout = new Layout("Root");
    }

    public RootLayout WithRows(params string[] names)
    {
        foreach (var name in names)
        {
            _slotOrder.Add(name);
            _visibleSlots.Add(name);
        }

        RebuildStructure();
        return this;
    }

    public void SetSize(string slotName, int size)
    {
        _slotSizes[slotName] = size;
        if (_slots.TryGetValue(slotName, out var slot))
            slot.Size(size);
    }

    public void ShowSlot(string slotName)
    {
        if (_slotOrder.Contains(slotName) && _visibleSlots.Add(slotName))
            _structureDirty = true;
    }

    public void HideSlot(string slotName)
    {
        if (_visibleSlots.Remove(slotName))
            _structureDirty = true;
    }

    public bool IsSlotVisible(string slotName) => _visibleSlots.Contains(slotName);

    public void RegisterPanel(string slotName, IPanel panel)
    {
        _panels[slotName] = panel;
    }

    public void RegisterLayout(string slotName, ILayout layout)
    {
        _layouts[slotName] = layout;
    }

    /// <summary>
    /// Updates only dirty panels and layouts. Returns true if anything was updated.
    /// </summary>
    public bool Update(bool force = false)
    {
        if (_structureDirty)
        {
            RebuildStructure();
            _structureDirty = false;
            force = true;
        }

        bool anyUpdated = false;

        foreach (var (name, panel) in _panels)
        {
            if (!_visibleSlots.Contains(name)) continue;

            if (force || panel.IsDirty)
            {
                if (_slots.TryGetValue(name, out var slot))
                {
                    slot.Update(panel.Render());
                    anyUpdated = true;
                }
            }
        }

        foreach (var (name, layout) in _layouts)
        {
            if (!_visibleSlots.Contains(name)) continue;

            if (force || layout.IsDirty)
            {
                if (_slots.TryGetValue(name, out var slot))
                {
                    slot.Update(layout.Render());
                    anyUpdated = true;
                }
            }
        }

        return anyUpdated;
    }

    private void RebuildStructure()
    {
        // Recreate all Layout instances since Spectre doesn't allow re-splitting
        _slots.Clear();

        var visible = _slotOrder
            .Where(n => _visibleSlots.Contains(n))
            .Select(n =>
            {
                var layout = new Layout(n);
                _slots[n] = layout;
                if (_slotSizes.TryGetValue(n, out var size) && size.HasValue)
                    layout.Size(size.Value);
                return layout;
            })
            .ToArray();

        Layout = new Layout("Root");

        if (visible.Length > 0)
            Layout.SplitRows(visible);
    }
}
