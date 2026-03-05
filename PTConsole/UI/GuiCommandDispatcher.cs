using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PTConsole.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PTConsole.UI;

public class GuiCommandDispatcher
{
    private readonly CapturingConsole _console;
    private readonly ICommandApp _app;
    private bool _isRunning;

    public CapturingConsole Console => _console;
    public bool IsRunning => _isRunning;

    public GuiCommandDispatcher(IConfiguration configuration)
    {
        _console = new CapturingConsole();

        var startup = new Startup(configuration);
        var services = new ServiceCollection();
        startup.ConfigureServices(services);
        startup.ConfigureGuiServices(services, _console);

        _app = new CommandApp(new TypeRegistrar(services));
        startup.ConfigureGuiCommands(_app, _console);
    }

    public async Task<int> DispatchAsync(string input)
    {
        if (_isRunning) return -1;
        _isRunning = true;

        try
        {
            var args = SplitArgs(input.Trim());
            if (args.Length == 0) return 0;

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
