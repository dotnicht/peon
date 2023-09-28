namespace Quiiiz.Peon.Configuration;

public sealed record class Database
{
    public required string Name { get; init; }
    public required string Connection { get; init; }
}
