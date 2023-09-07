namespace Quiiiz.Peon.Domain;

public record Account : IEntity
{
    public required long Id { get; init; }
    public required string Public { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
}
