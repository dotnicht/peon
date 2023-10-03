namespace Peon.Configuration;

public sealed record class Works
{
    public required bool Loop {  get; init; }
    public required bool Exceptions { get; init; }
    public required TimeSpan Timeout { get; init; }
}
