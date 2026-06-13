namespace ApiPrueba.Models;

public sealed class Site
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Department { get; init; }
    public required string Type { get; init; }
}
