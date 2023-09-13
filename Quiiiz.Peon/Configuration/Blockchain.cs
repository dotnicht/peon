﻿namespace Quiiiz.Peon.Configuration;

public record Blockchain
{
    public required string Seed { get; init; }
    public required string Password { get; init; }
    public required Uri Node { get; init; }
    public required long ChainId { get; init; }
    public required string TokenAddress { get; init; }
    public required string SpenderAddress { get; init;}
}
