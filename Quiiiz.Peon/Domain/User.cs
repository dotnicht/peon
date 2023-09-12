﻿using System.Numerics;

namespace Quiiiz.Peon.Domain;

public record User : IEntity
{
    public required long Id { get; init; }
    public required string Address { get; init; }
    public required BigInteger Balance { get; init; }
    
    public DateTime Created { get; init; } = DateTime.UtcNow;
}