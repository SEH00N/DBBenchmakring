using System.Data;
using Dapper;
using MemoryPack;
using MySql.Data.MySqlClient;

public class MemoryPack_MySQL : TestCase
{
    private TestData testData = null;
    private byte[] binary = null;

    public override double GetSerializedDataSize()
    {
        return MemoryPackSerializer.Serialize(new TestData()).Length;
    }

    protected override void Initialize()
    {
        testData = new TestData();
        binary = null;
    }

    protected override Task SerializeInternal()
    {
        binary = MemoryPackSerializer.Serialize(testData);
        return Task.CompletedTask;
    }
 
    protected override Task DeserializeInternal()
    {
        testData = MemoryPackSerializer.Deserialize<TestData>(binary);
        return Task.CompletedTask;
    }

    protected override async Task ReadInternal()
    {
        string connectionString = "Server=localhost;Port=7777;Database=test;Uid=test;Pwd=test;";
        using (IDbConnection db = new MySqlConnection(connectionString))
        {
            db.Open();

            string selectQuery = "SELECT `value` FROM `test_memorypack` where `key` = @key";
            IEnumerable<byte[]> result = await db.QueryAsync<byte[]>(selectQuery, new { key = $"test_{counter}" });

            foreach (var value in result)
            {
                binary = value;
                break;
            }
        }
    }

    protected override async Task WriteInternal()
    {
        string connectionString = "Server=localhost;Port=7777;Database=test;Uid=test;Pwd=test;";
        using (IDbConnection db = new MySqlConnection(connectionString))
        {
            db.Open();

            string insertQuery = "INSERT INTO `test_memorypack` (`key`, `value`) VALUES (@key, @value) ON DUPLICATE KEY UPDATE `value` = @value";
            await db.ExecuteAsync(insertQuery, new { key = $"test_{counter}", value = binary });
        }
    }
}
