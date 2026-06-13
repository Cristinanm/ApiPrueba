using ApiPrueba.Models;
using ApiPrueba.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiPrueba.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IncidentService service) : ControllerBase
{
    [HttpGet("summary")]
    public ActionResult GetSummary() => Ok(service.GetSummary(DateTime.UtcNow));

    [HttpGet("overdue")]
    public ActionResult<IReadOnlyList<Incident>> GetOverdue() =>
        Ok(service.GetOverdue(DateTime.UtcNow));
}
