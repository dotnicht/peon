using System.Numerics;

namespace Quiiiz.Peon.Domain;

public record class User : IEntity
{
    public required long Id { get; init; }
    public required string Address { get; init; }
    public required BigInteger Balance { get; init; }
    public required BigInteger TokenBalance { get; init; }
    public required BigInteger Approved { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
}