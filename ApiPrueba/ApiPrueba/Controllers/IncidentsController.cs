using ApiPrueba.Contracts;
using ApiPrueba.Models;
using ApiPrueba.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiPrueba.Controllers;

[ApiController]
[Route("api/incidents")]
public sealed class IncidentsController(IncidentService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Incident>> GetAll(
        [FromQuery] IncidentStatus? status,
        [FromQuery] IncidentSeverity? severity,
        [FromQuery] bool? escalated) =>
        Ok(service.GetIncidents(status, severity, escalated));

    [HttpGet("{id:guid}")]
    public ActionResult<Incident> GetById(Guid id) => Ok(service.GetIncident(id));

    [HttpPost]
    public ActionResult<Incident> Create(CreateIncidentRequest request)
    {
        var incident = service.CreateIncident(request);
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    [HttpPut("{id:guid}/assignment")]
    public ActionResult<Incident> Assign(Guid id, AssignIncidentRequest request) =>
        Ok(service.Assign(id, request.TechnicianId, request.ChangedBy ?? "Coordinador"));

    [HttpDelete("{id:guid}/assignment")]
    public ActionResult<Incident> Release(Guid id, [FromBody] ReleaseIncidentRequest request) =>
        Ok(service.Release(id, request.ChangedBy, request.Comment));

    [HttpPut("{id:guid}/status")]
    public ActionResult<Incident> ChangeStatus(Guid id, ChangeStatusRequest request) =>
        Ok(service.ChangeStatus(id, request.NewStatus, request.ChangedBy, request.Comment));

    [HttpGet("{id:guid}/history")]
    public ActionResult<IReadOnlyList<StatusHistory>> GetHistory(Guid id) =>
        Ok(service.GetIncident(id).StatusHistory.OrderBy(item => item.ChangedAtUtc));

    [HttpPost("escalations/run")]
    public ActionResult RunEscalation()
    {
        var count = service.EscalateUnattended(DateTime.UtcNow);
        return Ok(new { escalatedIncidents = count, executedAtUtc = DateTime.UtcNow });
    }
}
