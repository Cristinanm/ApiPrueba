using ApiPrueba.Contracts;
using ApiPrueba.Data;
using ApiPrueba.Models;

namespace ApiPrueba.Services;

public sealed class IncidentService(IncidentStore store)
{
    public const int MaximumActiveIncidentsPerTechnician = 3;

    private static readonly IReadOnlyDictionary<IncidentSeverity, TimeSpan> ResolutionTimes =
        new Dictionary<IncidentSeverity, TimeSpan>
        {
            [IncidentSeverity.Critical] = TimeSpan.FromHours(4),
            [IncidentSeverity.Urgent] = TimeSpan.FromHours(8),
            [IncidentSeverity.Medium] = TimeSpan.FromHours(24),
            [IncidentSeverity.Low] = TimeSpan.FromHours(48)
        };

    public IReadOnlyList<Incident> GetIncidents(
        IncidentStatus? status = null,
        IncidentSeverity? severity = null,
        bool? escalated = null)
    {
        lock (store.SyncRoot)
        {
            IEnumerable<Incident> query = store.Incidents;

            if (status.HasValue)
                query = query.Where(item => item.Status == status);
            if (severity.HasValue)
                query = query.Where(item => item.Severity == severity);
            if (escalated.HasValue)
                query = query.Where(item => item.IsEscalated == escalated);

            return query.OrderByDescending(item => item.CreatedAtUtc).ToList();
        }
    }

    public Incident GetIncident(Guid id)
    {
        lock (store.SyncRoot)
        {
            return FindIncident(id);
        }
    }

    public Incident CreateIncident(CreateIncidentRequest request)
    {
        lock (store.SyncRoot)
        {
            if (request.SiteId == Guid.Empty || store.Sites.All(item => item.Id != request.SiteId))
                throw new BusinessRuleException("El sitio indicado no existe.");

            var createdAt = request.CreatedAtUtc?.ToUniversalTime() ?? DateTime.UtcNow;
            var incident = new Incident
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                SiteId = request.SiteId,
                RequiredSpecialty = request.RequiredSpecialty,
                Severity = request.Severity,
                CreatedAtUtc = createdAt,
                ResolutionDeadlineUtc = createdAt.Add(ResolutionTimes[request.Severity])
            };

            incident.StatusHistory.Add(new StatusHistory
            {
                FromStatus = IncidentStatus.Registered,
                ToStatus = IncidentStatus.Registered,
                ChangedBy = "Sistema",
                Comment = "Incidente registrado"
            });

            store.Incidents.Add(incident);
            return incident;
        }
    }

    public Incident Assign(Guid incidentId, Guid technicianId, string changedBy)
    {
        lock (store.SyncRoot)
        {
            var incident = FindIncident(incidentId);
            var technician = store.Technicians.FirstOrDefault(item => item.Id == technicianId)
                ?? throw new EntityNotFoundException("Técnico no encontrado.");

            if (!technician.IsActive)
                throw new BusinessRuleException("El técnico está inactivo.");
            if (technician.Specialty != incident.RequiredSpecialty)
                throw new BusinessRuleException("La especialidad del técnico no coincide con el incidente.");
            if (incident.Status == IncidentStatus.Closed)
                throw new BusinessRuleException("No se puede asignar un incidente cerrado.");

            var activeCount = store.Incidents.Count(item =>
                item.TechnicianId == technicianId &&
                item.Status != IncidentStatus.Closed &&
                item.Id != incidentId);

            if (activeCount >= MaximumActiveIncidentsPerTechnician)
                throw new BusinessRuleException("El técnico ya tiene 3 incidentes activos.");

            var isFirstAssignment = incident.Status == IncidentStatus.Registered;
            incident.TechnicianId = technicianId;

            if (isFirstAssignment)
                AdvanceStatus(incident, IncidentStatus.Assigned, changedBy, "Asignación inicial");

            return incident;
        }
    }

    public Incident Release(Guid incidentId, string changedBy, string? comment)
    {
        lock (store.SyncRoot)
        {
            var incident = FindIncident(incidentId);
            if (incident.TechnicianId is null)
                throw new BusinessRuleException("El incidente no tiene un técnico asignado.");
            if (incident.Status == IncidentStatus.Closed)
                throw new BusinessRuleException("No se puede liberar un incidente cerrado.");

            incident.TechnicianId = null;
            incident.StatusHistory.Add(new StatusHistory
            {
                FromStatus = incident.Status,
                ToStatus = incident.Status,
                ChangedBy = changedBy,
                Comment = comment ?? "Técnico liberó el incidente"
            });
            return incident;
        }
    }

    public Incident ChangeStatus(Guid incidentId, IncidentStatus newStatus, string changedBy, string? comment)
    {
        lock (store.SyncRoot)
        {
            var incident = FindIncident(incidentId);
            AdvanceStatus(incident, newStatus, changedBy, comment);
            return incident;
        }
    }

    public int EscalateUnattended(DateTime utcNow)
    {
        lock (store.SyncRoot)
        {
            var candidates = store.Incidents.Where(item =>
                !item.IsEscalated &&
                item.Status == IncidentStatus.Registered &&
                item.CreatedAtUtc <= utcNow.AddHours(-2) &&
                item.Severity is IncidentSeverity.Critical or IncidentSeverity.Urgent);

            var count = 0;
            foreach (var incident in candidates)
            {
                incident.IsEscalated = true;
                incident.EscalatedAtUtc = utcNow;
                count++;
            }

            return count;
        }
    }

    public object GetSummary(DateTime utcNow)
    {
        lock (store.SyncRoot)
        {
            return new
            {
                total = store.Incidents.Count,
                active = store.Incidents.Count(item => item.Status != IncidentStatus.Closed),
                escalated = store.Incidents.Count(item => item.IsEscalated),
                overdue = store.Incidents.Count(item =>
                    item.Status is not IncidentStatus.Resolved and not IncidentStatus.Closed &&
                    item.ResolutionDeadlineUtc < utcNow),
                byStatus = Enum.GetValues<IncidentStatus>().Select(value => new
                {
                    status = value,
                    count = store.Incidents.Count(item => item.Status == value)
                }),
                bySeverity = Enum.GetValues<IncidentSeverity>().Select(value => new
                {
                    severity = value,
                    count = store.Incidents.Count(item => item.Severity == value)
                }),
                technicianWorkload = store.Technicians.Select(technician => new
                {
                    technician.Id,
                    technician.Name,
                    technician.Specialty,
                    activeIncidents = store.Incidents.Count(item =>
                        item.TechnicianId == technician.Id &&
                        item.Status != IncidentStatus.Closed)
                })
            };
        }
    }

    public IReadOnlyList<Incident> GetOverdue(DateTime utcNow)
    {
        lock (store.SyncRoot)
        {
            return store.Incidents
                .Where(item =>
                    item.Status is not IncidentStatus.Resolved and not IncidentStatus.Closed &&
                    item.ResolutionDeadlineUtc < utcNow)
                .OrderBy(item => item.ResolutionDeadlineUtc)
                .ToList();
        }
    }

    private Incident FindIncident(Guid id) =>
        store.Incidents.FirstOrDefault(item => item.Id == id)
        ?? throw new EntityNotFoundException("Incidente no encontrado.");

    private static void AdvanceStatus(
        Incident incident,
        IncidentStatus newStatus,
        string changedBy,
        string? comment)
    {
        if ((int)newStatus != (int)incident.Status + 1)
            throw new BusinessRuleException(
                $"Transición inválida. El estado {incident.Status} sólo puede avanzar al siguiente estado.");

        if (newStatus is IncidentStatus.Assigned or IncidentStatus.InProgress && incident.TechnicianId is null)
            throw new BusinessRuleException("El incidente debe tener un técnico asignado.");

        var previous = incident.Status;
        incident.Status = newStatus;

        if (newStatus == IncidentStatus.Resolved)
            incident.ResolvedAtUtc = DateTime.UtcNow;
        if (newStatus == IncidentStatus.Closed)
            incident.ClosedAtUtc = DateTime.UtcNow;

        incident.StatusHistory.Add(new StatusHistory
        {
            FromStatus = previous,
            ToStatus = newStatus,
            ChangedBy = changedBy,
            Comment = comment
        });
    }
}
