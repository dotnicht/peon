using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Quiiiz.Peon.Configuration;
using Quiiiz.Peon.Domain;

namespace Quiiiz.Peon.Persistence;

internal class MongoRepository<TItem> : IRepository<TItem> where TItem : class, IEntity
{
    private readonly IMongoCollection<TItem> collection;

    public IQueryable<TItem> Content => collection.AsQueryable();

    public MongoRepository(IOptions<Database> options)
    {
        var client = new MongoClient(options.Value.Connection);
        var database = client.GetDatabase(options.Value.Name);
        collection = database.GetCollection<TItem>(typeof(TItem).Name);
    }

    public async Task Add(TItem item) => await collection.InsertOneAsync(item);

    public async Task Remove(TItem item) => await collection.DeleteOneAsync(x => x.Id == item.Id);
}
