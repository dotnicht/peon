using System.Numerics;

namespace Peon.Domain;

public record User : IEntity, IAudit
{
    public required long Id { get; init; }
    public required string Address { get; init; }
    public required BigInteger Gas { get; init; }
    public required BigInteger Token { get; init; }
    public required BigInteger Approved { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    public DateTime? Updated { get; init; }
}