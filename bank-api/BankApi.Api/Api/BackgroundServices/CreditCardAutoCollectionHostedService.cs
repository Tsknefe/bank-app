using BankApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BankApi.Api.BackgroundServices;

public class CreditCardAutoCollectionHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CreditCardAutoCollectionHostedService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    public CreditCardAutoCollectionHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CreditCardAutoCollectionHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CreditCardAutoCollectionHostedService started.");

        await RunOnce(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                await RunOnce(stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto-collection loop error.");
            }
        }

        _logger.LogInformation("CreditCardAutoCollectionHostedService stopped.");
    }

    private async Task RunOnce(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<CreditCardAutoCollectionService>();

            var nowUtc = DateTime.UtcNow;
            var count = await svc.CollectDuePaymentsAsync(nowUtc, ct);

            if (count > 0)
                _logger.LogInformation("Auto-collection executed. Count={Count}", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-collection run error.");
        }
    }
}
