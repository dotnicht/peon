﻿using Peon.Domain;

namespace Peon.Persistence;

public interface IRepository<TItem> where TItem : class, IEntity, IAudit
{
    IQueryable<TItem> Content { get; }
    Task Add(TItem item);
    Task Update(TItem item);
    Task Remove(TItem item);
}