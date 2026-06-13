using ApiPrueba.Contracts;
using ApiPrueba.Data;
using ApiPrueba.Models;
using ApiPrueba.Services;
using Xunit;

namespace ApiPrueba.Tests;

public sealed class IncidentServiceTests
{
    [Theory]
    [InlineData(IncidentSeverity.Critical, 4)]
    [InlineData(IncidentSeverity.Urgent, 8)]
    [InlineData(IncidentSeverity.Medium, 24)]
    [InlineData(IncidentSeverity.Low, 48)]
    public void CreateIncident_AssignsDeadlineAccordingToSeverity(
        IncidentSeverity severity,
        int expectedHours)
    {
        var (service, store) = CreateSystem();
        var createdAt = new DateTime(2026, 6, 13, 12, 0, 0, DateTimeKind.Utc);

        var incident = service.CreateIncident(Request(store, severity, createdAt: createdAt));

        Assert.Equal(createdAt.AddHours(expectedHours), incident.ResolutionDeadlineUtc);
    }

    [Fact]
    public void CreateIncident_AddsInitialHistory()
    {
        var (service, store) = CreateSystem();

        var incident = service.CreateIncident(Request(store));

        var history = Assert.Single(incident.StatusHistory);
        Assert.Equal(IncidentStatus.Registered, history.ToStatus);
    }

    [Fact]
    public void Assign_RejectsTechnicianWithDifferentSpecialty()
    {
        var (service, store) = CreateSystem();
        var incident = service.CreateIncident(Request(store, specialty: Specialty.FiberOptic));
        var microwaveTechnician = store.Technicians.Single(
            item => item.Specialty == Specialty.Microwave);

        var exception = Assert.Throws<BusinessRuleException>(() =>
            service.Assign(incident.Id, microwaveTechnician.Id, "Prueba"));

        Assert.Contains("especialidad", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assign_RejectsFourthActiveIncident()
    {
        var (service, store) = CreateSystem();
        var technician = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);

        for (var index = 0; index < 3; index++)
        {
            var existing = service.CreateIncident(Request(store));
            service.Assign(existing.Id, technician.Id, "Prueba");
        }

        var fourth = service.CreateIncident(Request(store));

        Assert.Throws<BusinessRuleException>(() =>
            service.Assign(fourth.Id, technician.Id, "Prueba"));
    }

    [Fact]
    public void Assign_FirstAssignmentAdvancesStatusToAssigned()
    {
        var (service, store) = CreateSystem();
        var incident = service.CreateIncident(Request(store));
        var technician = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);

        service.Assign(incident.Id, technician.Id, "Coordinador");

        Assert.Equal(IncidentStatus.Assigned, incident.Status);
        Assert.Equal(technician.Id, incident.TechnicianId);
    }

    [Fact]
    public void ChangeStatus_RejectsSkippingStates()
    {
        var (service, store) = CreateSystem();
        var incident = service.CreateIncident(Request(store));

        Assert.Throws<BusinessRuleException>(() =>
            service.ChangeStatus(incident.Id, IncidentStatus.InProgress, "Prueba", null));
    }

    [Fact]
    public void ChangeStatus_AllowsCompleteForwardFlowAndRecordsHistory()
    {
        var (service, store) = CreateSystem();
        var incident = service.CreateIncident(Request(store));
        var technician = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);

        service.Assign(incident.Id, technician.Id, "Coordinador");
        service.ChangeStatus(incident.Id, IncidentStatus.InProgress, "Técnico", null);
        service.ChangeStatus(incident.Id, IncidentStatus.Resolved, "Técnico", "Servicio restaurado");
        service.ChangeStatus(incident.Id, IncidentStatus.Closed, "Supervisor", "Validado");

        Assert.Equal(IncidentStatus.Closed, incident.Status);
        Assert.Equal(5, incident.StatusHistory.Count);
        Assert.NotNull(incident.ResolvedAtUtc);
        Assert.NotNull(incident.ClosedAtUtc);
    }

    [Fact]
    public void Release_RemovesTechnicianWithoutMovingStatusBackward()
    {
        var (service, store) = CreateSystem();
        var incident = service.CreateIncident(Request(store));
        var technician = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);
        service.Assign(incident.Id, technician.Id, "Coordinador");

        service.Release(incident.Id, "Técnico", "Fin de turno");

        Assert.Null(incident.TechnicianId);
        Assert.Equal(IncidentStatus.Assigned, incident.Status);
    }

    [Fact]
    public void Assign_CanReassignIncidentToAnotherMatchingTechnician()
    {
        var (service, store) = CreateSystem();
        var first = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);
        var second = new Technician { Name = "Otro técnico", Specialty = Specialty.FiberOptic };
        store.Technicians.Add(second);
        var incident = service.CreateIncident(Request(store));
        service.Assign(incident.Id, first.Id, "Coordinador");

        service.Assign(incident.Id, second.Id, "Coordinador");

        Assert.Equal(second.Id, incident.TechnicianId);
        Assert.Equal(IncidentStatus.Assigned, incident.Status);
    }

    [Theory]
    [InlineData(IncidentSeverity.Critical, true)]
    [InlineData(IncidentSeverity.Urgent, true)]
    [InlineData(IncidentSeverity.Medium, false)]
    [InlineData(IncidentSeverity.Low, false)]
    public void EscalateUnattended_OnlyEscalatesCriticalAndUrgent(
        IncidentSeverity severity,
        bool shouldEscalate)
    {
        var (service, store) = CreateSystem();
        var now = new DateTime(2026, 6, 13, 12, 0, 0, DateTimeKind.Utc);
        var incident = service.CreateIncident(Request(
            store,
            severity,
            createdAt: now.AddHours(-3)));

        service.EscalateUnattended(now);

        Assert.Equal(shouldEscalate, incident.IsEscalated);
    }

    [Fact]
    public void EscalateUnattended_DoesNotEscalateAssignedIncident()
    {
        var (service, store) = CreateSystem();
        var now = new DateTime(2026, 6, 13, 12, 0, 0, DateTimeKind.Utc);
        var incident = service.CreateIncident(Request(
            store,
            IncidentSeverity.Critical,
            createdAt: now.AddHours(-3)));
        var technician = store.Technicians.Single(item => item.Specialty == Specialty.FiberOptic);
        service.Assign(incident.Id, technician.Id, "Coordinador");

        service.EscalateUnattended(now);

        Assert.False(incident.IsEscalated);
    }

    [Fact]
    public void GetOverdue_ReturnsOnlyUnresolvedPastDeadline()
    {
        var (service, store) = CreateSystem();
        var now = new DateTime(2026, 6, 16, 12, 0, 0, DateTimeKind.Utc);
        var overdue = service.CreateIncident(Request(
            store,
            IncidentSeverity.Low,
            createdAt: now.AddHours(-49)));
        service.CreateIncident(Request(store, createdAt: now));

        var result = service.GetOverdue(now);

        Assert.Single(result);
        Assert.Equal(overdue.Id, result[0].Id);
    }

    private static (IncidentService Service, IncidentStore Store) CreateSystem()
    {
        var store = new IncidentStore();
        return (new IncidentService(store), store);
    }

    private static CreateIncidentRequest Request(
        IncidentStore store,
        IncidentSeverity severity = IncidentSeverity.Critical,
        Specialty specialty = Specialty.FiberOptic,
        DateTime? createdAt = null) =>
        new(
            "Falla de enlace principal",
            "El sitio reporta pérdida total de conectividad.",
            store.Sites[0].Id,
            specialty,
            severity,
            createdAt);
}
