using System.Text;
using PTConsole.UI.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PTConsole.UI;

public class GuiCommandDispatcher
{
    private readonly CapturingConsole _console;
    private readonly ICommandApp _app;
    private bool _isRunning;
    private bool _exitRequested;

    public CapturingConsole Console => _console;
    public bool IsRunning => _isRunning;
    public bool ExitRequested => _exitRequested;

    public GuiCommandDispatcher(ICommandApp app, CapturingConsole console)
    {
        _app = app;
        _console = console;
    }

    public async Task<int> DispatchAsync(string input)
    {
        if (_isRunning) return -1;
        _isRunning = true;

        try
        {
            var args = SplitArgs(input.Trim());
            if (args.Length == 0) return 0;

            if (args[0].Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                args[0].Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                _exitRequested = true;
                return 0;
            }

            // Fall through to CLI commands
            return await _app.RunAsync(args);
        }
        catch (Exception ex)
        {
            _console.Write(new Markup(
                $"[red]Error: {Markup.Escape(ex.Message)}[/]\n"));
            return -1;
        }
        finally
        {
            _isRunning = false;
        }
    }

    private static string[] SplitArgs(string input)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuote = false;
        char quoteChar = '"';

        foreach (var c in input)
        {
            if (inQuote)
            {
                if (c == quoteChar) inQuote = false;
                else current.Append(c);
            }
            else if (c == '"' || c == '\'')
            {
                inQuote = true;
                quoteChar = c;
            }
            else if (c == ' ')
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0) args.Add(current.ToString());
        return args.ToArray();
    }
}
