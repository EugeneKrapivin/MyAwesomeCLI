using Spectre.Console.Cli;
using static System.Environment;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Spectre.Console;

namespace MyAwesomeCLI;

public class ContextModel
{
    public string CurrentContext { get; set; } = string.Empty;
    public List<CliContext> DefinedContexts { get; set; } = new();
}

public class CliContext
{
    public required string Name { get; set; }
    public required string UserKey { get; set; }
    public required string Secret { get; set; }
}

public class CreateContextCommand : AsyncCommand<CreateContextCommand.CreateContextArgument>
{
    public override Task<int> ExecuteAsync(CommandContext commandContext, CreateContextArgument settings)
    {
        const string cliFolder = ".awesome_cli";
        const string configFileName = ".awesome_cli.config";
        var targetFolder = Path.Combine(GetFolderPath(SpecialFolder.UserProfile), cliFolder);
        var targetFile = Path.Combine(targetFolder, configFileName);
        
        Directory.CreateDirectory(targetFolder); // ensure the directory exists

        var cliContext = new CliContext
        {
            Name = settings.Name,
            UserKey = settings.UserKey,
            Secret = settings.Secret
        };

        var cliContextCollection = new ContextModel
        {
            CurrentContext = cliContext.Name,
            DefinedContexts = { { cliContext } }
        };

        var content = JsonSerializer.Serialize(cliContextCollection, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        });

        File.WriteAllText(targetFile, content);

        AnsiConsole.MarkupLineInterpolated($"Created a new CLI context [green bold]{cliContext.Name}[/]");
        AnsiConsole.Write(new Table()
            .AddColumns("property", "value")
            .AddRow("userkey", settings.UserKey)
            .AddRow("secret", settings.Secret)); // probably shouldn't write it back
        AnsiConsole.Write("created at: ");
        AnsiConsole.Write(new TextPath(targetFile).RootStyle(new Style(foreground: Color.Red))
            .SeparatorStyle(new Style(foreground: Color.Green))
            .StemStyle(new Style(foreground: Color.Blue))
            .LeafStyle(new Style(foreground: Color.Yellow)));


        return Task.FromResult(0);
    }

    public class CreateContextArgument : CommandSettings
    {
        [Required]
        [CommandOption("-n|--name")]
        public string Name { get; set; }

        [Required]
        [CommandOption("-u|--user-key")]
        public string UserKey { get; set; }

        [Required]
        [CommandOption("-s|--user-secret")]
        public string Secret { get; set; }
    }
}