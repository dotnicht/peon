namespace Quiiiz.Peon.Domain;

public record Address : IEntity
{
    public required long Id { get; init; }
    public required string Public { get; init; }
    public required string Hash { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
}
