using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class BSON_MongoDB : TestCase
{
    private MongoClient mongoDBConnection = null;
    private TestData testData = null;
    private BsonDocument bson = null;

    public BSON_MongoDB()
    {
        mongoDBConnection = new MongoClient("mongodb://localhost:7777");
    }

    ~BSON_MongoDB()
    {
        mongoDBConnection.Dispose();
    }

    public override double GetSerializedDataSize() 
    {
        using (var stream = new MemoryStream())
        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, new TestData().ToBsonDocument());
            writer.Flush();
            return stream.Length;
        }
    }

    protected override void Initialize()
    {
        testData = new TestData();
        bson = null;
    }

    protected override Task SerializeInternal()
    {
        bson = testData.ToBsonDocument();
        return Task.CompletedTask;
    }
 
    protected override Task DeserializeInternal()
    {
        testData = BsonSerializer.Deserialize<TestData>(bson);
        return Task.CompletedTask;
    }

    protected override async Task ReadInternal()
    {
        var db = mongoDBConnection.GetDatabase("test");
        var collection = db.GetCollection<BsonDocument>("test_bson");

        var filter = Builders<BsonDocument>.Filter.Eq("_id", $"test_{counter}");
        var doc = await collection.Find(filter).FirstOrDefaultAsync();
        bson = doc["value"].AsBsonDocument;
    }

    protected override async Task WriteInternal()
    {
        var db = mongoDBConnection.GetDatabase("test");
        var collection = db.GetCollection<BsonDocument>("test_bson");

        var doc = new BsonDocument {
            { "_id", $"test_{counter}" },
            { "value", bson }
        };

        var filter = Builders<BsonDocument>.Filter.Eq("_id", $"test_{counter}");
        await collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true });
    }
}
