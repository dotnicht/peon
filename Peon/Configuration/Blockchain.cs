namespace Peon.Configuration;

public sealed record class Blockchain
{
    public required Uri Node { get; init; }
    public required long ChainId { get; init; }
    public required string TokenAddress { get; init; }
    public required string SpenderAddress { get; init; }
    public required int MasterIndex { get; init; }
    public required Credentials Users { get; init; }
    public required Credentials Master { get; init; }

    public sealed record class Credentials
    {
        public required string Seed { get; init; }
        public required string Password { get; init; }
    }
}

