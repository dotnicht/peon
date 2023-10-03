namespace Peon.Works;

public interface IWork
{
    Task Work(CancellationToken cancellationToken);
}
