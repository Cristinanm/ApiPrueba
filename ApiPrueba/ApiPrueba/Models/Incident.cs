namespace ApiPrueba.Models;

public sealed class Incident
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public Guid SiteId { get; init; }
    public Specialty RequiredSpecialty { get; init; }
    public IncidentSeverity Severity { get; init; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Registered;
    public Guid? TechnicianId { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime ResolutionDeadlineUtc { get; init; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedAtUtc { get; set; }
    public List<StatusHistory> StatusHistory { get; } = [];
}
