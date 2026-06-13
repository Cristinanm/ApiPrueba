using System.ComponentModel.DataAnnotations;
using ApiPrueba.Models;

namespace ApiPrueba.Contracts;

public sealed record CreateIncidentRequest(
    [Required, StringLength(120, MinimumLength = 5)] string Title,
    [Required, StringLength(1000, MinimumLength = 10)] string Description,
    Guid SiteId,
    Specialty RequiredSpecialty,
    IncidentSeverity Severity,
    DateTime? CreatedAtUtc = null);

public sealed record AssignIncidentRequest(Guid TechnicianId, string? ChangedBy = null);

public sealed record ChangeStatusRequest(
    IncidentStatus NewStatus,
    [Required] string ChangedBy,
    string? Comment = null);

public sealed record ReleaseIncidentRequest(
    [Required] string ChangedBy,
    string? Comment = null);

public sealed record CreateTechnicianRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Name,
    Specialty Specialty);
