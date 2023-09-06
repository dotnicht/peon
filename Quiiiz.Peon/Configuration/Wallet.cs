namespace Quiiiz.Peon.Configuration;

public record class Wallet
{
    public required string Seed { get; init; }
    public required string Password { get; init; }
}
