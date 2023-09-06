namespace Quiiiz.Peon.Configuration;

public record class Database
{
    public required string Name { get; init; }
    public required string Connection { get; init; }
}
