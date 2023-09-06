namespace Quiiiz.Peon.Domain;

public record Address : IEntity
{
    public required long Id { get; init; }
    public required string Public { get; init; }
    public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;
}
