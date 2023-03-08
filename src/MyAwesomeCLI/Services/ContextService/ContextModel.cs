namespace MyAwesomeCLI.Services.ContextService;

public class ContextModel
{
    public string CurrentContext { get; set; } = string.Empty;
    public List<CliContext> DefinedContexts { get; set; } = new();
}
