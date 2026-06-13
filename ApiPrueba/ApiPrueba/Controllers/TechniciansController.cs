using ApiPrueba.Contracts;
using ApiPrueba.Data;
using ApiPrueba.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiPrueba.Controllers;

[ApiController]
[Route("api/technicians")]
public sealed class TechniciansController(IncidentStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<object>> GetAll()
    {
        lock (store.SyncRoot)
        {
            var result = store.Technicians.Select(technician => new
            {
                technician.Id,
                technician.Name,
                technician.Specialty,
                technician.IsActive,
                activeIncidents = store.Incidents.Count(incident =>
                    incident.TechnicianId == technician.Id &&
                    incident.Status != IncidentStatus.Closed)
            });
            return Ok(result);
        }
    }

    [HttpPost]
    public ActionResult<Technician> Create(CreateTechnicianRequest request)
    {
        var technician = new Technician
        {
            Name = request.Name.Trim(),
            Specialty = request.Specialty
        };

        lock (store.SyncRoot)
        {
            store.Technicians.Add(technician);
        }

        return CreatedAtAction(nameof(GetAll), new { id = technician.Id }, technician);
    }
}
