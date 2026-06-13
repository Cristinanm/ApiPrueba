namespace ApiPrueba.Models;

public sealed class Technician
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public Specialty Specialty { get; init; }
    public bool IsActive { get; set; } = true;
}
