using System.Text.Json;

public class Program
{
    public static async Task Main(string[] args)
    {
        Func<TestCase> tcFactory = () => {
            return new JSON_Redis();
            // return new MemoryPack_Redis();
            // return new JSON_MySQL();
            // return new MemoryPack_MySQL();
            // return new BSON_MongoDB();
        };

        await DBBenchmarking(tcFactory);
        AnalyzeBenchmarkingResult(tcFactory);
    }

    // #######################################################################################

    private static async Task DBBenchmarking(Func<TestCase> tcFactory)
    {
        await StartTest(tcFactory.Invoke(), 5);

        Statistics.Start();
        await StartTest(tcFactory.Invoke(), 10000);
        Statistics.End(tcFactory.Invoke().GetType().Name);
    }

    private static async Task StartTest(TestCase tc, int count)
    {
        await tc.StartTest(ETestType.Write, count);
        await tc.StartTest(ETestType.Read, count);
        await tc.StartTest(ETestType.Alternately, count);
    }

    // #######################################################################################

    private static void AnalyzeBenchmarkingResult(Func<TestCase> tcFactory)
    {
        string testName = tcFactory.Invoke().GetType().Name;

        string analyticsPath = $"F:\\Lab\\DBBenchmarkTest\\Analytics\\{testName}";
        Directory.CreateDirectory(analyticsPath);

        string resultPath = $"F:\\Lab\\DBBenchmarkTest\\Results\\{testName}";
        string[] resultFilePaths = Directory.GetFiles(resultPath);

        List<AnalyzedData> analyzedDataList = new List<AnalyzedData>();
        foreach(string filePath in resultFilePaths)
        {
            AnalyzedData analyzedData = Analytics.AnalyzeSingleTest(JsonSerializer.Deserialize<List<StatisticsData>>(File.ReadAllText(filePath)), tcFactory.Invoke().GetSerializedDataSize());
            analyzedDataList.Add(analyzedData);
            File.WriteAllText($"{analyticsPath}\\{Path.GetFileNameWithoutExtension(filePath)}.json", JsonSerializer.Serialize(analyzedData));
        }

        AnalyzedData totalAnalyzedData = Analytics.AnalyzeTotalTest(analyzedDataList);
        File.WriteAllText($"{analyticsPath}\\_report.json", JsonSerializer.Serialize(totalAnalyzedData));
    }
}