using System.Text;
using System.Text.Json;

public static class ReportConverter
{
    public static string JSON2CSV(string json)
    {
        void Add(StringBuilder sb, double value)
        {
            if(sb.Length > 0)
                sb.Append(";");

            sb.Append(Math.Round(value, 3));
        }

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

    public static string JSON2HTMLTable(List<string> jsonList)
    {
        if(jsonList.Count == 0)
            return null;

        List<AnalyzedData> analyzedDataList = new List<AnalyzedData>();
        foreach(string json in jsonList)
        {
            AnalyzedData analyzedData = JsonSerializer.Deserialize<AnalyzedData>(json);
            if(analyzedData == null)
                return null;

            analyzedDataList.Add(analyzedData);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<table>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"6\" class=\"table-center\">Serialize</th>");
        sb.AppendLine($"    <th colspan=\"6\" class=\"table-center\">Deserialize</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">ProcessingTime (ms)</th>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">GC Alloc (mb)</th>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">ProcessingTime (ms)</th>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">GC Alloc (mb)</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"  </tr>");
        foreach(AnalyzedData analyzedData in analyzedDataList)
        {
            sb.AppendLine($"  <tr>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeProcessingTimeAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeProcessingTimeMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeProcessingTimeMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeGCAllocAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeGCAllocMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializeGCAllocMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeProcessingTimeAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeProcessingTimeMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeProcessingTimeMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeGCAllocAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeGCAllocMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.DeserializeGCAllocMax, 3)}</td>");
            sb.AppendLine($"  </tr>");
        }
        sb.AppendLine($"</table>");

        sb.AppendLine($"<table>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"11\" class=\"table-center\">Write</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"7\" class=\"table-center\">Response Time (ms)</th>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">GC Alloc (mb)</th>");
        sb.AppendLine($"    <th colspan=\"1\" class=\"table-center\">Throughput</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">Median</th>");
        sb.AppendLine($"    <th class=\"table-center\">90th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">95th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">99th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">req/s</th>");
        sb.AppendLine($"  </tr>");
        foreach(AnalyzedData analyzedData in analyzedDataList)
        {
            sb.AppendLine($"  <tr>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTimeAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTimeMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTimeMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTimeMedian, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTime90th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTime95th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteResponseTime99th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteGCAllocAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteGCAllocMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteGCAllocMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.WriteThroughputPerSecond, 3)}</td>");
            sb.AppendLine($"  </tr>");
        }
        sb.AppendLine($"</table>");

        sb.AppendLine($"<table>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"11\" class=\"table-center\">Read</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"7\" class=\"table-center\">Response Time (ms)</th>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">GC Alloc (mb)</th>");
        sb.AppendLine($"    <th colspan=\"1\" class=\"table-center\">Throughput</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">Median</th>");
        sb.AppendLine($"    <th class=\"table-center\">90th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">95th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">99th pct</th>");
        sb.AppendLine($"    <th class=\"table-center\">Average</th>");
        sb.AppendLine($"    <th class=\"table-center\">Min</th>");
        sb.AppendLine($"    <th class=\"table-center\">Max</th>");
        sb.AppendLine($"    <th class=\"table-center\">req/s</th>");
        sb.AppendLine($"  </tr>");
        foreach(AnalyzedData analyzedData in analyzedDataList)
        {
            sb.AppendLine($"  <tr>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTimeAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTimeMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTimeMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTimeMedian, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTime90th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTime95th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadResponseTime99th, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadGCAllocAverage, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadGCAllocMin, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadGCAllocMax, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.ReadThroughputPerSecond, 3)}</td>");
            sb.AppendLine($"  </tr>");
        }
        sb.AppendLine($"</table>");

        sb.AppendLine($"<table>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"3\" class=\"table-center\">ETC</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th colspan=\"1\" class=\"table-center\">Processing Time (ms)</th>");
        sb.AppendLine($"    <th colspan=\"1\" class=\"table-center\">GC Alloc (mb)</th>");
        sb.AppendLine($"    <th colspan=\"1\" class=\"table-center\">Serialized Data Size (kb)</th>");
        sb.AppendLine($"  </tr>");
        sb.AppendLine($"  <tr>");
        sb.AppendLine($"    <th class=\"table-center\">Total</th>");
        sb.AppendLine($"    <th class=\"table-center\">Total</th>");
        sb.AppendLine($"    <th class=\"table-center\">Single</th>");
        sb.AppendLine($"  </tr>");
        foreach(AnalyzedData analyzedData in analyzedDataList)
        {
            sb.AppendLine($"  <tr>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.TotalTime, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.TotalGCAlloc, 3)}</td>");
            sb.AppendLine($"    <td class=\"table-center\">{Math.Round(analyzedData.SerializedDataSize, 3)}</td>");
            sb.AppendLine($"  </tr>");
        }
        sb.AppendLine($"</table>");

        return sb.ToString();
    }
}