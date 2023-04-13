using AutoByte;
using Microsoft.Extensions.Logging;

internal class Main
{
    private readonly ILogger<Main> _logger;

    public Main(ILogger<Main> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        var demo = new ByteSlide().GetStructure<DemoStructure>();

        _logger.LogInformation("Hello, World!");
    }
}