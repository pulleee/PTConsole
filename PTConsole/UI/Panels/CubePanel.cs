using Spectre.Console;
using Spectre.Console.Rendering;

namespace PTConsole.UI.Panels;

public class CubePanel : IRenderable, IHasDirtyState
{
    public BoxBorder Border { get; set; } = BoxBorder.Square;
    public Padding Padding { get; set; } = new Padding(0, 0, 0, 0);

    private readonly DateTime _startTime = DateTime.UtcNow;

    // Always dirty — it's animating
    public bool IsDirty => true;

    // Unit cube vertices centered at origin
    private static readonly (double x, double y, double z)[] Vertices =
    [
        (-1, -1, -1), ( 1, -1, -1), ( 1,  1, -1), (-1,  1, -1),
        (-1, -1,  1), ( 1, -1,  1), ( 1,  1,  1), (-1,  1,  1),
    ];

    // Edges as pairs of vertex indices
    private static readonly (int a, int b)[] Edges =
    [
        (0,1), (1,2), (2,3), (3,0),
        (4,5), (5,6), (6,7), (7,4),
        (0,4), (1,5), (2,6), (3,7),
    ];

    public Measurement Measure(RenderOptions options, int maxWidth)
    {
        return new Measurement(maxWidth, maxWidth);
    }

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var elapsed = (DateTime.UtcNow - _startTime).TotalSeconds;

        var angleX = elapsed * 1;
        var angleY = elapsed * 1;
        var angleZ = elapsed * 1;

        var innerWidth = Math.Max(1, maxWidth - 2 - Padding.Left - Padding.Right);
        var totalHeight = options.Height ?? Console.WindowHeight;
        var innerHeight = Math.Max(1, totalHeight - 2 - Padding.Top - Padding.Bottom);

        // Use half-width for aspect ratio (terminal chars are ~2:1)
        var scale = Math.Min(innerWidth / 2.0, innerHeight) * 0.35;
        var cx = innerWidth / 2.0;
        var cy = innerHeight / 2.0;

        // Project all vertices
        var projected = new (double px, double py)[Vertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            var (x, y, z) = Vertices[i];
            (x, y, z) = RotateX(x, y, z, angleX);
            (x, y, z) = RotateY(x, y, z, angleY);
            (x, y, z) = RotateZ(x, y, z, angleZ);

            // Simple perspective
            var depth = 4.0 + z;
            var px = (x / depth) * scale * 2.0 + cx; // *2 for aspect ratio
            var py = (y / depth) * scale + cy;
            projected[i] = (px, py);
        }

        // Rasterize edges into a char buffer
        var buffer = new char[innerHeight, innerWidth];
        for (int row = 0; row < innerHeight; row++)
            for (int col = 0; col < innerWidth; col++)
                buffer[row, col] = ' ';

        foreach (var (a, b) in Edges)
        {
            DrawLine(buffer, innerWidth, innerHeight,
                projected[a].px, projected[a].py,
                projected[b].px, projected[b].py);
        }

        // Mark vertices
        for (int i = 0; i < projected.Length; i++)
        {
            var col = (int)Math.Round(projected[i].px);
            var row = (int)Math.Round(projected[i].py);
            if (row >= 0 && row < innerHeight && col >= 0 && col < innerWidth)
                buffer[row, col] = 'o';
        }

        // Build rows of text
        var rows = new List<IRenderable>();
        for (int row = 0; row < innerHeight; row++)
        {
            var line = new string(Enumerable.Range(0, innerWidth)
                .Select(col => buffer[row, col]).ToArray());
            rows.Add(new Markup($"[cyan]{Markup.Escape(line)}[/]"));
        }

        var content = rows.Count > 0 ? new Rows(rows) : (IRenderable)new Markup("");

        var panel = new Panel(content);
        panel.Expand();
        panel.Border = Border;
        panel.Padding = Padding;
        panel.Header = new PanelHeader("[dim] 3D Cube [/]");

        return ((IRenderable)panel).Render(options, maxWidth);
    }

    private static void DrawLine(char[,] buf, int width, int height,
        double x0, double y0, double x1, double y1)
    {
        int steps = (int)(Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0)) * 1.5) + 1;
        for (int i = 0; i <= steps; i++)
        {
            double t = steps == 0 ? 0 : (double)i / steps;
            int col = (int)Math.Round(x0 + (x1 - x0) * t);
            int row = (int)Math.Round(y0 + (y1 - y0) * t);
            if (row >= 0 && row < height && col >= 0 && col < width)
            {
                // Pick char based on local slope
                if (buf[row, col] == ' ' || buf[row, col] == '.')
                {
                    double dx = Math.Abs(x1 - x0);
                    double dy = Math.Abs(y1 - y0);
                    buf[row, col] = dx > dy * 2 ? '-' : dy > dx * 2 ? '|' : (x1 > x0 == y1 > y0 ? '\\' : '/');
                }
            }
        }
    }

    private static (double x, double y, double z) RotateX(double x, double y, double z, double a)
    {
        var cos = Math.Cos(a);
        var sin = Math.Sin(a);
        
        return (x, y * cos - z * sin, y * sin + z * cos);
    }

    private static (double x, double y, double z) RotateY(double x, double y, double z, double a)
    {
        var cos = Math.Cos(a);
        var sin = Math.Sin(a);

        return (x * cos + z * sin, y, -x * sin + z * cos);
    }

    private static (double x, double y, double z) RotateZ(double x, double y, double z, double a)
    {
        var cos = Math.Cos(a);
        var sin = Math.Sin(a);

        return (x * cos - y * sin, x * sin + y * cos, z);
    }
}
