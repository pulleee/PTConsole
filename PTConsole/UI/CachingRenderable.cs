using Spectre.Console.Rendering;

namespace PTConsole.UI;

/// <summary>
/// Wraps an IRenderable that implements IHasDirtyState.
/// Caches the last Render() output and replays it when the inner renderable is clean.
/// </summary>
public class CachingRenderable : IRenderable
{
    private readonly IRenderable _inner;
    private readonly IHasDirtyState _dirtyState;

    private List<Segment>? _cachedSegments;
    private int _cachedMaxWidth;
    private int? _cachedHeight;

    public CachingRenderable(IRenderable inner)
    {
        _inner = inner;
        _dirtyState = inner as IHasDirtyState
            ?? throw new ArgumentException("Inner renderable must implement IHasDirtyState", nameof(inner));
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return _inner.Measure(options, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var height = options.Height;
        bool sizeChanged = maxWidth != _cachedMaxWidth || height != _cachedHeight;

        if (_cachedSegments == null || sizeChanged || _dirtyState.IsDirty)
        {
            _cachedSegments = _inner.Render(options, maxWidth).ToList();
            _cachedMaxWidth = maxWidth;
            _cachedHeight = height;
        }

        return _cachedSegments;
    }
}
