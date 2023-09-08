using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Quiiiz.Peon.Domain;

namespace Quiiiz.Peon.Persistence;

internal class MongoRepository<TItem> : IRepository<TItem> where TItem : class, IEntity
{
    private readonly IMongoCollection<TItem> collection;

    public IQueryable<TItem> Content => collection.AsQueryable();

    public MongoRepository(IOptions<Configuration.Database> options) 
        => collection = new MongoClient(options.Value.Connection)
            .GetDatabase(options.Value.Name)
            .GetCollection<TItem>(typeof(TItem).Name.ToLower());

    public async Task Add(TItem item) => await collection.InsertOneAsync(item);
    public async Task Update(TItem item) => await collection.ReplaceOneAsync(x => x.Id == item.Id, item);
    public async Task Remove(TItem item) => await collection.DeleteOneAsync(x => x.Id == item.Id);
}
