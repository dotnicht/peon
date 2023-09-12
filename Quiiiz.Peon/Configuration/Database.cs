namespace Quiiiz.Peon.Configuration;

public record Database
{
    public required string Name { get; init; }
    public required string Connection { get; init; }
}
