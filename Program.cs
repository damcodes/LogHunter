using LogHunter;
using Microsoft.Extensions.Configuration;

try
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
        .Build();

    bool run;
    do
    {
        var app = new ConsoleApp(isInteractiveMode: args.Length == 0, configuration: config);
        run = await app.Run(args);
    }
    while (run);
}
catch (System.Exception e)
{
    Console.WriteLine($"\n{e.Message}\n");
}