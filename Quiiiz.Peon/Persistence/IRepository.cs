using Quiiiz.Peon.Domain;

namespace Quiiiz.Peon.Persistence;

public interface IRepository<TItem> where TItem : class, IEntity
{
    IQueryable<TItem> Content { get; }
    Task Add(TItem item);
    Task Update(TItem item);
    Task Remove(TItem item);
}