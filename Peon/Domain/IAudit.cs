namespace Peon.Domain;

public interface IAudit
{
    DateTime Created { get; init; }
    DateTime? Updated { get; init; }
}
