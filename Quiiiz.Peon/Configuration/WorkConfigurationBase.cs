namespace Quiiiz.Peon.Configuration;

public abstract record class WorkConfigurationBase
{
    public TimeSpan Interval { get; init; }
    public TimeSpan FirstRunDelay { get; init; }
}
