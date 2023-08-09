using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builtConfig = null as IConfigurationRoot;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(configuration =>
    {
        var config = configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

        builtConfig = config.Build();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton(builtConfig.GetRequiredSection("AppConfig").Get<AppConfig>());
    })
    .Build();

host.Run();
