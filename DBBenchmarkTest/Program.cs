using System.Text;
using System.Text.Json;

public class Program
{
    public const string ROOT_REPORT_PATH = "F:\\Lab\\DBBenchmarking\\DBBenchmarkTest\\Results";

    public static async Task Main(string[] args)
    {
        // string testCase = args[0];
        // string testNumber = args[1];

        string testCase = "BSON_MongoDB";
        string testNumber = "7";

        if(string.IsNullOrEmpty(testCase))
            return;

        if(string.IsNullOrEmpty(testNumber))
            return;
            
        Func<TestCase> tcFactory = testCase switch {
            "JSON_Redis" => () => new JSON_Redis(),
            "MemoryPack_Redis" => () => new MemoryPack_Redis(),
            "JSON_MySQL" => () => new JSON_MySQL(),
            "MemoryPack_MySQL" => () => new MemoryPack_MySQL(),
            "BSON_MongoDB" => () => new BSON_MongoDB(),
            _ => null
        };

        if (tcFactory == null)
            return;

        Statistics.TEST_NUMBER = testNumber;

        await DBBenchmarking(tcFactory);
        AnalyzeBenchmarkingResult(tcFactory);
        ExportReport(tcFactory);
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

        string analyticsPath = $"{Program.ROOT_REPORT_PATH}\\Analytics\\{testName}";
        Directory.CreateDirectory(analyticsPath);

        string resultPath = $"{Program.ROOT_REPORT_PATH}\\Results\\{testName}";
        string[] resultFilePaths = Directory.GetFiles(resultPath);

        List<AnalyzedData> analyzedDataList = new List<AnalyzedData>();
        foreach(string filePath in resultFilePaths)
        {
            AnalyzedData analyzedData = Analytics.AnalyzeSingleTest(JsonSerializer.Deserialize<List<StatisticsData>>(File.ReadAllText(filePath)), tcFactory.Invoke().GetSerializedDataSize());
            analyzedDataList.Add(analyzedData);
            File.WriteAllText($"{analyticsPath}\\{Path.GetFileNameWithoutExtension(filePath)}.json", JsonSerializer.Serialize(analyzedData));
        }

        AnalyzedData totalAnalyzedData = Analytics.AnalyzeTotalTest(analyzedDataList);
        File.WriteAllText($"{analyticsPath}\\average.json", JsonSerializer.Serialize(totalAnalyzedData));
    }

    // #######################################################################################

    private static void ExportReport(Func<TestCase> tcFactory)
    {
        string testName = tcFactory.Invoke().GetType().Name;
        
        string analyticsPath = $"{Program.ROOT_REPORT_PATH}\\Analytics\\{testName}";
        string[] analyticsFilePaths = Directory.GetFiles(analyticsPath);
        
        StringBuilder sb = new StringBuilder();
        foreach(string filePath in analyticsFilePaths)
            sb.AppendLine($"{Path.GetFileNameWithoutExtension(filePath)} : {ReportConverter.JSON2CSV(File.ReadAllText(filePath))}");

        string reportPath = $"{Program.ROOT_REPORT_PATH}\\Report";
        Directory.CreateDirectory(reportPath);
        File.WriteAllText($"{reportPath}\\{testName}_report.txt", sb.ToString());
    }
}