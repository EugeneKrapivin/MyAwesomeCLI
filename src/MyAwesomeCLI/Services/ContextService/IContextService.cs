using LanguageExt;

namespace MyAwesomeCLI.Services.ContextService;

internal interface IContextService
{
    public Task<Option<CliContext>> GetCurrentContextAsync();

    public Task<Option<CliContext>> GetContextAsync(string name);

    public Task<ContextModel> GetAllContextsAsync();

    public Task<Either<(string error, string path), (CliContext ctx, string path)>> AddContextAsync(string name, string accessToken);

    public Task<Option<CliContext>> SetCurrentContextAsync(string name);

    public Task<Option<CliContext>> DeleteContextAsync(string name);
}
