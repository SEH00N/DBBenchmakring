using System.Text;
using System.Text.Json;
using StackExchange.Redis;

public class JSON_Redis : TestCase
{
    private ConnectionMultiplexer redisConnection = null;
    private TestData testData = null;
    private string json = null;

    public JSON_Redis()
    {
        redisConnection = ConnectionMultiplexer.Connect(new ConfigurationOptions() {
            EndPoints = { "localhost:7777" },
            AbortOnConnectFail = false,
            ConnectTimeout = 30000
        });
    }

    ~JSON_Redis()
    {
        redisConnection.Close();
        redisConnection.Dispose();
    }

    public override double GetSerializedDataSize()
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TestData())).Length;
    }

    protected override void Initialize()
    {
        testData = new TestData();
        json = null;
    }

    protected override Task SerializeInternal()
    {
        json = JsonSerializer.Serialize(testData);
        return Task.CompletedTask;
    }
 
    protected override Task DeserializeInternal()
    {
        testData = JsonSerializer.Deserialize<TestData>(json);
        return Task.CompletedTask;
    }

    protected override async Task ReadInternal()
    {
        var db = redisConnection.GetDatabase();
        json = await db.HashGetAsync("test_json", $"test_{counter}");
    }

    protected override async Task WriteInternal()
    {
        var db = redisConnection.GetDatabase();
        await db.HashSetAsync("test_json", $"test_{counter}", json);
    }
}
