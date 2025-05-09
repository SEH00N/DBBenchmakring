using System.Diagnostics;

public static class Statistics
{
    private const int TEST_COUNT = 2;
    // private static int log_counter = 0;

    private static Stopwatch stopwatch = null;
    private static List<StatisticsData> dataList = null;

    public static void Start()
    {
        dataList ??= new List<StatisticsData>();
        dataList.Clear();

        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    public static void End(string testName)
    {
        stopwatch.Stop();
        
        string dirPath = $"F:\\Lab\\DBBenchmarkTest\\Results\\{testName}";
        Directory.CreateDirectory(dirPath);
        File.WriteAllText($"{dirPath}\\{testName}_{TEST_COUNT}.json", System.Text.Json.JsonSerializer.Serialize(dataList));
    }

    public static void Record(EStatisticsActionType actionType)
    {
        if(stopwatch == null)
            return;

        if(dataList == null)
            return;

        double totalElapsed = stopwatch.Elapsed.TotalMilliseconds;
        double gcAlloc = GC.GetTotalMemory(false) * 0.0009765625d * 0.0009765625d;
        StatisticsData data = new StatisticsData(actionType, totalElapsed, gcAlloc);
        dataList.Add(data);
        // Console.WriteLine(data.ToString() + $"log_counter : {log_counter++ / 4f}");
    }
}