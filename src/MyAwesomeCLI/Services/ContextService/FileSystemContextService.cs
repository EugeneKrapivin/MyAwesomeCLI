using LanguageExt;

using System.Text.Json;

using static System.Environment;

namespace MyAwesomeCLI.Services.ContextService;

internal class FileSystemContextService : IContextService
{
    private readonly string _targetFolder;
    private readonly string _targetFile;
    private readonly ContextModel _context;
    private const string _cliFolder = ".awesome_cli";
    private const string _configFileName = ".awesome_cli.config";

    public FileSystemContextService(string folderPath = "")
    {
        _targetFolder = Path.Combine(CalculateTargetFolder(folderPath), _cliFolder);
        _targetFile = Path.Combine(_targetFolder, _configFileName);

        var dirInfo = Directory.CreateDirectory(_targetFolder);

        if (!dirInfo.Exists)
            throw new Exception($"Target directory \"{_targetFolder}\"does not exist");

        if (File.Exists(_targetFile))
        {
            var contents = File.ReadAllText(_targetFile);
            _context = JsonSerializer.Deserialize<ContextModel>(contents, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }) ?? new();
        }
        
        _context ??= new();
    }

    private static string CalculateTargetFolder(string folderPath) 
        => string.IsNullOrWhiteSpace(folderPath) 
            ? GetFolderPath(SpecialFolder.UserProfile) 
            : folderPath;

    private void WriteState()
    {
        var contents = JsonSerializer.Serialize(_context, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        });

        File.WriteAllText(_targetFile, contents);
    }

    public async Task<Either<(string error, string path), (CliContext ctx, string path)>> AddContextAsync(string name, string accessToken)
    {
        if (_context.DefinedContexts.Any(x => x.Name == name))
        {
            return ($"Context with name {name} is already defined.", _targetFile);
        }

        var ctx = new CliContext
        {
            Name = name,
            AccessToken = accessToken
        };

        _context.DefinedContexts.Add(ctx);
        if (string.IsNullOrEmpty(_context.CurrentContext))
        {
            _context.CurrentContext = name;
        }

        WriteState();

        return (ctx, _targetFile);
    }

    public Task<Option<CliContext>> DeleteContextAsync(string name)
    {
        if (_context.DefinedContexts.Any(x => x.Name == name))
        {
            var ctx = _context.DefinedContexts.Single(x => x.Name == name);
            _context.DefinedContexts.Remove(ctx);
            if (_context.CurrentContext == ctx.Name)
            {
                _context.CurrentContext = string.Empty;
            }
            WriteState();

            return Task.FromResult(Option<CliContext>.Some(ctx));
        }

        return Task.FromResult(Option<CliContext>.None);
    }

    public Task<ContextModel> GetAllContextsAsync() => Task.FromResult(_context);

    public Task<Option<CliContext>> GetContextAsync(string name)
        => _context.DefinedContexts.SingleOrDefault(x => x.Name == name) switch
        {
            null => Task.FromResult(Option<CliContext>.None),
            var ctx => Task.FromResult(Option<CliContext>.Some(ctx))
        };

    public Task<Option<CliContext>> GetCurrentContextAsync()
        => _context.DefinedContexts.SingleOrDefault(x => x.Name == _context.CurrentContext) switch
        {
            null => Task.FromResult(Option<CliContext>.None),
            var ctx => Task.FromResult(Option<CliContext>.Some(ctx))
        };

    public Task<Option<CliContext>> SetCurrentContextAsync(string name)
    {
        var ctx = _context.DefinedContexts.SingleOrDefault(x => x.Name == name);

        if (ctx is null)
        {
            return Task.FromResult(Option<CliContext>.None);
        };
        _context.CurrentContext = name;

        return Task.FromResult(Option<CliContext>.Some(ctx));
    }
}
