namespace ApiPrueba.Models;

public sealed class StatusHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IncidentStatus FromStatus { get; init; }
    public IncidentStatus ToStatus { get; init; }
    public DateTime ChangedAtUtc { get; init; } = DateTime.UtcNow;
    public required string ChangedBy { get; init; }
    public string? Comment { get; init; }
}
