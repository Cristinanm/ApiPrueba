namespace ApiPrueba.Services;

public sealed class IncidentEscalationWorker(
    IncidentService service,
    ILogger<IncidentEscalationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        do
        {
            var escalated = service.EscalateUnattended(DateTime.UtcNow);
            if (escalated > 0)
                logger.LogWarning("Se escalaron automáticamente {Count} incidentes.", escalated);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
