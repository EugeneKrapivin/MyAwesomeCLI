using LanguageExt;

using MediatR;
using MyAwesomeCLI.Services.ContextService;

namespace MyAwesomeCLI.Handlers;

public record CreateContextSuccessResponse(CliContext Context, string CreatedAt);
public record CreateContextErrorResponse(string Error, string Path);
public record CreateContextRequest(string Name, string UserKey, string Secret) :
    IRequest<Either<CreateContextErrorResponse, CreateContextSuccessResponse>>;

internal class CreateContextHandler 
    : IRequestHandler<CreateContextRequest, Either<CreateContextErrorResponse, CreateContextSuccessResponse>>
{
    private readonly IContextService _contextService;
    private readonly IExchangeCredentials _exchangeCredentials;

    public CreateContextHandler(IContextService contextService, IExchangeCredentials exchangeCredentials)
    {
        _contextService = contextService;
        _exchangeCredentials = exchangeCredentials;
    }

    public async Task<Either<CreateContextErrorResponse, CreateContextSuccessResponse>> Handle(CreateContextRequest request, CancellationToken cancellationToken)
    {
        var creds = await _exchangeCredentials.GetCredentialsAsync(request.UserKey, request.Secret);

        return await _contextService.AddContextAsync(request.Name, creds)
            .ToAsync()
            .Case switch
            {
                (CliContext ctx, string path) => new CreateContextSuccessResponse(ctx, path),
                (string error, string path) => new CreateContextErrorResponse(error, path),
                _ => throw new NotImplementedException()
            };
    }
}
