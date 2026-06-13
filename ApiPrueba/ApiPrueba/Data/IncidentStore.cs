using ApiPrueba.Models;

namespace ApiPrueba.Data;

public sealed class IncidentStore
{
    public object SyncRoot { get; } = new();
    public List<Incident> Incidents { get; } = [];
    public List<Technician> Technicians { get; } = [];
    public List<Site> Sites { get; } = [];

    public IncidentStore()
    {
        Sites.AddRange(
        [
            new Site { Name = "POP Guatemala", Department = "Guatemala", Type = "POP" },
            new Site { Name = "Nodo Jalapa", Department = "Jalapa", Type = "Nodo" },
            new Site { Name = "Antena Quetzaltenango", Department = "Quetzaltenango", Type = "Antena" }
        ]);

        Technicians.AddRange(
        [
            new Technician { Name = "Ana López", Specialty = Specialty.FiberOptic },
            new Technician { Name = "Carlos Méndez", Specialty = Specialty.Microwave },
            new Technician { Name = "Sofía Ramírez", Specialty = Specialty.ElectricalSystems }
        ]);
    }
}
