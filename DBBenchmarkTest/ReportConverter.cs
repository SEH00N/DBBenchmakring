using System.Text;
using System.Text.Json;

public static class ReportConverter
{
    public static string JSON2CSV(string json)
    {
        AnalyzedData analyzedData = JsonSerializer.Deserialize<AnalyzedData>(json);
        if(analyzedData == null)
            return null;

        StringBuilder sb = new StringBuilder();

        Add(sb, analyzedData.SerializeProcessingTimeAverage);
        Add(sb, analyzedData.SerializeProcessingTimeMin);
        Add(sb, analyzedData.SerializeProcessingTimeMax);
        Add(sb, analyzedData.SerializeGCAllocAverage);
        Add(sb, analyzedData.SerializeGCAllocMin);
        Add(sb, analyzedData.SerializeGCAllocMax);

        Add(sb, analyzedData.DeserializeProcessingTimeAverage);
        Add(sb, analyzedData.DeserializeProcessingTimeMin);
        Add(sb, analyzedData.DeserializeProcessingTimeMax);
        Add(sb, analyzedData.DeserializeGCAllocAverage);
        Add(sb, analyzedData.DeserializeGCAllocMin);
        Add(sb, analyzedData.DeserializeGCAllocMax);

        Add(sb, analyzedData.WriteResponseTimeAverage);
        Add(sb, analyzedData.WriteResponseTimeMin);
        Add(sb, analyzedData.WriteResponseTimeMax);
        Add(sb, analyzedData.WriteResponseTimeMedian);
        Add(sb, analyzedData.WriteResponseTime90th);
        Add(sb, analyzedData.WriteResponseTime95th);
        Add(sb, analyzedData.WriteResponseTime99th);
        Add(sb, analyzedData.WriteGCAllocAverage);
        Add(sb, analyzedData.WriteGCAllocMin);
        Add(sb, analyzedData.WriteGCAllocMax);
        Add(sb, analyzedData.WriteThroughputPerSecond);

        Add(sb, analyzedData.ReadResponseTimeAverage);
        Add(sb, analyzedData.ReadResponseTimeMin);
        Add(sb, analyzedData.ReadResponseTimeMax);
        Add(sb, analyzedData.ReadResponseTimeMedian);
        Add(sb, analyzedData.ReadResponseTime90th);
        Add(sb, analyzedData.ReadResponseTime95th);
        Add(sb, analyzedData.ReadResponseTime99th);
        Add(sb, analyzedData.ReadGCAllocAverage);
        Add(sb, analyzedData.ReadGCAllocMin);
        Add(sb, analyzedData.ReadGCAllocMax);
        Add(sb, analyzedData.ReadThroughputPerSecond);

        Add(sb, analyzedData.TotalTime);
        Add(sb, analyzedData.TotalGCAlloc);
        Add(sb, analyzedData.SerializedDataSize);

        return sb.ToString();
    }

    private static void Add(StringBuilder sb, double value)
    {
        if(sb.Length > 0)
            sb.Append(";");

        sb.Append(Math.Round(value, 3));
    }
}