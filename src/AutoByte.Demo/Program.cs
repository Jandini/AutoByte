// Created with JandaBox http://github.com/Jandini/JandaBox
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

using var provider = new ServiceCollection()
    .AddLogging(builder => builder.AddSerilog(new LoggerConfiguration()
        .Enrich.WithMachineName()
        .WriteTo.Console(
            theme: AnsiConsoleTheme.Code,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] [{MachineName}] [{SourceContext}] {Message}{NewLine}{Exception}")
        .CreateLogger()))
    .AddTransient<Main>()
    .BuildServiceProvider();

try
{
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    provider.GetRequiredService<Main>().Run();
}
catch (Exception ex)
{
    provider.GetService<ILogger<Program>>()?
        .LogCritical(ex, "Unhandled exception");
}