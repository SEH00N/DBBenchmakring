using System.Data;
using System.Text;
using System.Text.Json;
using Dapper;
using MySql.Data.MySqlClient;

public class JSON_MySQL : TestCase
{
    private TestData testData = null;
    private string json = null;

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
        string connectionString = "Server=localhost;Port=7777;Database=test;Uid=test;Pwd=test;";
        using (IDbConnection db = new MySqlConnection(connectionString))
        {
            db.Open();

            string selectQuery = "SELECT `value` FROM `test_json` where `key` = @key";
            IEnumerable<string> result = await db.QueryAsync<string>(selectQuery, new { key = $"test_{counter}" });

            foreach (var value in result)
            {
                json = value;
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

            string insertQuery = "INSERT INTO `test_json` (`key`, `value`) VALUES (@key, @value) ON DUPLICATE KEY UPDATE `value` = @value";
            await db.ExecuteAsync(insertQuery, new { key = $"test_{counter}", value = json });
        }
    }
}
