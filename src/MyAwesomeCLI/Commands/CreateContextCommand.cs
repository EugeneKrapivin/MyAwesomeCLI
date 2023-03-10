using Spectre.Console.Cli;
using static System.Environment;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Spectre.Console;
using MediatR;
using MyAwesomeCLI.Handlers;
using ValidationResult = Spectre.Console.ValidationResult;

namespace MyAwesomeCLI.Commands;

public class CreateContextCommand : AsyncCommand<CreateContextCommand.CreateContextArgument>
{
    private readonly IMediator _mediator;

    public CreateContextCommand(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<int> ExecuteAsync(CommandContext commandContext, CreateContextArgument settings)
        => await _mediator.Send(new CreateContextRequest(settings.Name!, settings.UserKey!, settings.Secret!))
            .ToAsync()
            .Match(
                success =>
                {
                    var (cliContext, targetFile) = success;

                    AnsiConsole.MarkupLineInterpolated($"Created a new CLI context [green bold]{cliContext.Name}[/]");
                    AnsiConsole.Write(new Table()
                        .AddColumns("property", "value")
                        .AddRow("name", settings.Name!)
                        .AddRow("userkey", settings.UserKey!)
                        .AddRow("at", cliContext.AccessToken));
                    AnsiConsole.Write("created at: ");
                    AnsiConsole.Write(new TextPath(targetFile).RootStyle(new Style(foreground: Color.Red))
                        .SeparatorStyle(new Style(foreground: Color.Green))
                        .StemStyle(new Style(foreground: Color.Blue))
                        .LeafStyle(new Style(foreground: Color.Yellow)));

                    return 0;
                },
                error =>
                {
                    var (msg, targetFile) = error;

                    AnsiConsole.MarkupLine($"[red]Failed to create context at path:[/]");
                    AnsiConsole.Write(new TextPath(targetFile).RootStyle(new Style(foreground: Color.Red))
                        .SeparatorStyle(new Style(foreground: Color.Green))
                        .StemStyle(new Style(foreground: Color.Blue))
                        .LeafStyle(new Style(foreground: Color.Yellow)));
                    AnsiConsole.MarkupLineInterpolated($"[yellow]{msg}[/]");

                    return -1;
                });

    public override ValidationResult Validate(CommandContext context, CreateContextArgument settings)
    {
        if (settings.Name == "this-is-a-test-string-to-fail-the-validation")
            return ValidationResult.Error("context name must not be \"this-is-a-test-string-to-fail-the-validation\"");
        if (string.IsNullOrWhiteSpace(settings.Name))
            return ValidationResult.Error("context name must not be null or empty");
        if (string.IsNullOrWhiteSpace(settings.UserKey))
            return ValidationResult.Error("context user key must not be null or empty");
        if (string.IsNullOrWhiteSpace(settings.Secret))
            return ValidationResult.Error("context user secret must not be null or empty");

        return ValidationResult.Success();
    }

    public class CreateContextArgument : CommandSettings
    {
        [Required]
        [CommandOption("-n|--name")]
        public string? Name { get; set; }

        [Required]
        [CommandOption("-u|--user-key")]
        public string? UserKey { get; set; }

        [Required]
        [CommandOption("-s|--user-secret")]
        public string? Secret { get; set; }
    }
}