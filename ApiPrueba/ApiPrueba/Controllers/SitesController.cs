using ApiPrueba.Data;
using ApiPrueba.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiPrueba.Controllers;

[ApiController]
[Route("api/sites")]
public sealed class SitesController(IncidentStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Site>> GetAll()
    {
        lock (store.SyncRoot)
        {
            return Ok(store.Sites.OrderBy(item => item.Name).ToList());
        }
    }
}
