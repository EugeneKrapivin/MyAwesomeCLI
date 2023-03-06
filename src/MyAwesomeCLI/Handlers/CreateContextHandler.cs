using LanguageExt;

using MediatR;

using MyAwesomeCLI.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using static System.Environment;

namespace MyAwesomeCLI.Handlers;

public record CreateContextSuccessResponse(CliContext Context, string CreatedAt);
public record CreateContextErrorResponse(string Error, string Path);
public record CreateContextRequest(string Name, string UserKey, string Secret) :
    IRequest<Either<CreateContextErrorResponse, CreateContextSuccessResponse>>;

internal class CreateContextHandler 
    : IRequestHandler<CreateContextRequest, Either<CreateContextErrorResponse, CreateContextSuccessResponse>>
{
    public async Task<Either<CreateContextErrorResponse, CreateContextSuccessResponse>> Handle(CreateContextRequest request, CancellationToken cancellationToken)
    {
        const string cliFolder = ".awesome_cli";
        const string configFileName = ".awesome_cli.config";
        var targetFolder = Path.Combine(GetFolderPath(SpecialFolder.UserProfile), cliFolder);
        var targetFile = Path.Combine(targetFolder, configFileName);

        var dirInfo = Directory.CreateDirectory(targetFolder); // ensure the directory exists
        if (!dirInfo.Exists)
            return new CreateContextErrorResponse("Target directory does not exist", targetFolder);

        var cliContext = new CliContext
        {
            Name = request.Name,
            UserKey = request.UserKey,
            Secret = request.Secret
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

        // probably should add some protection here as well
        await File.WriteAllTextAsync(targetFile, content, cancellationToken);

        return new CreateContextSuccessResponse(cliContext, targetFile);
    }
}
