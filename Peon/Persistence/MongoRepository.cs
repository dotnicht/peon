using System.Numerics;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Peon.Domain;

namespace Peon.Persistence;

internal sealed class MongoRepository<TItem> : IRepository<TItem> where TItem : class, IEntity, IAudit
{
    private readonly IMongoCollection<TItem> collection;

    public IQueryable<TItem> Content => collection.AsQueryable();

    public MongoRepository(IOptions<Configuration.Database> options)
    {
        BsonSerializer.TryRegisterSerializer(new BigIntegerSerializer());
        collection = new MongoClient(options.Value.Connection).GetDatabase(options.Value.Name).GetCollection<TItem>(typeof(TItem).Name.ToLower());
    }

    public async Task Add(TItem item) => await collection.InsertOneAsync(item);
    public async Task Update(TItem item) => await collection.ReplaceOneAsync(x => x.Id == item.Id, item);
    public async Task Remove(TItem item) => await collection.DeleteOneAsync(x => x.Id == item.Id);

    private sealed class BigIntegerSerializer : SerializerBase<BigInteger>
    {
        public override BigInteger Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => BigInteger.Parse(context.Reader.ReadString());
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, BigInteger value)
            => context.Writer.WriteString(value.ToString());
    }
}
