using System.Text.Json;
using MemoryPack;
using StackExchange.Redis;

public class MemoryPack_Redis : TestCase
{
    private ConnectionMultiplexer redisConnection = null;
    private TestData testData = null;
    private byte[] binaryData = null;

    public MemoryPack_Redis()
    {
        redisConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions() {
            EndPoints = { "localhost:7777" },
            AbortOnConnectFail = false,
            ConnectTimeout = 30000
        });
    }

    ~MemoryPack_Redis()
    {
        redisConnection.Close();
        redisConnection.Dispose();
    }

    public override double GetSerializedDataSize()
    {
        return MemoryPackSerializer.Serialize(new TestData()).Length;
    }

    protected override void Initialize()
    {
        testData = new TestData();
        binaryData = null;
    }

    protected override Task SerializeInternal()
    {
        binaryData = MemoryPackSerializer.Serialize(testData);
        return Task.CompletedTask;
    }
 
    protected override Task DeserializeInternal()
    {
        testData = MemoryPackSerializer.Deserialize<TestData>(binaryData);
        return Task.CompletedTask;
    }

    protected override async Task ReadInternal()
    {
        var db = redisConnection.GetDatabase();
        binaryData = await db.HashGetAsync("test_memorypack", $"test_{counter}");
    }

    protected override async Task WriteInternal()
    {
        var db = redisConnection.GetDatabase();
        await db.HashSetAsync("test_memorypack", $"test_{counter}", binaryData);
    }
}
