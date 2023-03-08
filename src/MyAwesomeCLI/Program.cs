using Microsoft.Extensions.DependencyInjection;

using MyAwesomeCLI.Commands;
using MyAwesomeCLI.Handlers;
using MyAwesomeCLI.Infra;
using MyAwesomeCLI.Services.ContextService;

using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

services.AddSingleton<IContextService, FileSystemContextService>();
services.AddSingleton<IExchangeCredentials, CredentialsExchangeService>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(app =>
{
    app.AddBranch("context", context =>
    {
        context.AddCommand<CreateContextCommand>("create")
            .WithDescription("this commands create a new context for our awesome app")
            .WithExample(new[] { "context", "create", "--name", "test-context", "--user-key", "test-user", "--user-secret", "test-secret" });
    });
});

await app.RunAsync(args);