using Microsoft.Extensions.DependencyInjection;

using MyAwesomeCLI.Commands;
using MyAwesomeCLI.Infra;

using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

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