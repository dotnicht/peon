namespace Quiiiz.Peon.Configuration;

public record class Credentials
{
    public required string Seed { get; init; }
    public required string Password { get; init; }
}
