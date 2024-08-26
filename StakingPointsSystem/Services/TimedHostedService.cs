namespace StakingPointsSystem.Services;

public class TimedHostedService : BackgroundService
{
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

        await DoWork();

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    // Could also be a async method, that can be awaited in ExecuteAsync above
    private async Task DoWork()
    {
        _logger.LogInformation("Run background job");
        using (var scope = _serviceProvider.CreateScope())
        {
            var scoreCalculator = scope.ServiceProvider.GetRequiredService<ScoreCalculator>();
            await scoreCalculator.Calculate(DateTime.Now);
        }
    }
}