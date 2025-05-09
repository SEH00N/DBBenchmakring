public static class Analytics
{
    public static AnalyzedData AnalyzeSingleTest(List<StatisticsData> testResultList, double serializedDataSize)
    {
        Analyze analyze = new Analyze(testResultList);
        AnalyzedData analyzedData = new AnalyzedData();

        analyzedData.TotalTime = analyze.SerializeTimeList.Sum() + analyze.DeserializeTimeList.Sum() + analyze.WriteTimeList.Sum() + analyze.ReadTimeList.Sum();
        analyzedData.TotalGCAlloc = analyze.SerializeGCAllocList.Sum() + analyze.DeserializeGCAllocList.Sum() + analyze.WriteGCAllocList.Sum() + analyze.ReadGCAllocList.Sum();
        analyzedData.SerializedDataSize = serializedDataSize;

        analyzedData.SerializeProcessingTimeAverage = analyze.SerializeTimeList.Average();
        analyzedData.SerializeProcessingTimeMin = analyze.SerializeTimeList.Min();
        analyzedData.SerializeProcessingTimeMax = analyze.SerializeTimeList.Max();
        analyzedData.SerializeGCAllocAverage = analyze.SerializeGCAllocList.Average();
        analyzedData.SerializeGCAllocMin = analyze.SerializeGCAllocList.Min();
        analyzedData.SerializeGCAllocMax = analyze.SerializeGCAllocList.Max();

        analyzedData.DeserializeProcessingTimeAverage = analyze.DeserializeTimeList.Average();
        analyzedData.DeserializeProcessingTimeMin = analyze.DeserializeTimeList.Min();
        analyzedData.DeserializeProcessingTimeMax = analyze.DeserializeTimeList.Max();
        analyzedData.DeserializeGCAllocAverage = analyze.DeserializeGCAllocList.Average();
        analyzedData.DeserializeGCAllocMin = analyze.DeserializeGCAllocList.Min();
        analyzedData.DeserializeGCAllocMax = analyze.DeserializeGCAllocList.Max();

        analyzedData.WriteResponseTimeAverage = analyze.WriteTimeList.Average();
        analyzedData.WriteResponseTimeMin = analyze.WriteTimeList.Min();
        analyzedData.WriteResponseTimeMax = analyze.WriteTimeList.Max();
        analyzedData.WriteResponseTimeMedian = analyze.WriteTimeList.Percentile(0.5f);
        analyzedData.WriteResponseTime90th = analyze.WriteTimeList.Percentile(0.9f);
        analyzedData.WriteResponseTime95th = analyze.WriteTimeList.Percentile(0.95f);
        analyzedData.WriteResponseTime99th = analyze.WriteTimeList.Percentile(0.99f);
        analyzedData.WriteGCAllocAverage = analyze.WriteGCAllocList.Average();
        analyzedData.WriteGCAllocMin = analyze.WriteGCAllocList.Min();
        analyzedData.WriteGCAllocMax = analyze.WriteGCAllocList.Max();

        analyzedData.ReadResponseTimeAverage = analyze.ReadTimeList.Average();
        analyzedData.ReadResponseTimeMin = analyze.ReadTimeList.Min();
        analyzedData.ReadResponseTimeMax = analyze.ReadTimeList.Max();
        analyzedData.ReadResponseTimeMedian = analyze.ReadTimeList.Percentile(0.5f);
        analyzedData.ReadResponseTime90th = analyze.ReadTimeList.Percentile(0.9f);
        analyzedData.ReadResponseTime95th = analyze.ReadTimeList.Percentile(0.95f);
        analyzedData.ReadResponseTime99th = analyze.ReadTimeList.Percentile(0.99f);
        analyzedData.ReadGCAllocAverage = analyze.ReadGCAllocList.Average();
        analyzedData.ReadGCAllocMin = analyze.ReadGCAllocList.Min();
        analyzedData.ReadGCAllocMax = analyze.ReadGCAllocList.Max();

        analyzedData.WriteThroughputPerSecond = analyze.WriteTimeList.Count / analyzedData.TotalTime;
        analyzedData.ReadThroughputPerSecond = analyze.ReadTimeList.Count / analyzedData.TotalTime;

        return analyzedData;
    }

    public static AnalyzedData AnalyzeTotalTest(List<AnalyzedData> analyzedDataList)
    {
        AnalyzedData analyzedData = new AnalyzedData();

        analyzedData.TotalTime = analyzedDataList.Average(i => i.TotalTime);
        analyzedData.TotalGCAlloc = analyzedDataList.Average(i => i.TotalGCAlloc);
        analyzedData.SerializedDataSize = analyzedDataList.Average(i => i.SerializedDataSize);

        analyzedData.SerializeProcessingTimeAverage = analyzedDataList.Average(i => i.SerializeProcessingTimeAverage);
        analyzedData.SerializeProcessingTimeMin = analyzedDataList.Average(i => i.SerializeProcessingTimeMin);
        analyzedData.SerializeProcessingTimeMax = analyzedDataList.Average(i => i.SerializeProcessingTimeMax);
        analyzedData.SerializeGCAllocAverage = analyzedDataList.Average(i => i.SerializeGCAllocAverage);
        analyzedData.SerializeGCAllocMin = analyzedDataList.Average(i => i.SerializeGCAllocMin);
        analyzedData.SerializeGCAllocMax = analyzedDataList.Average(i => i.SerializeGCAllocMax);

        analyzedData.DeserializeProcessingTimeAverage = analyzedDataList.Average(i => i.DeserializeProcessingTimeAverage);
        analyzedData.DeserializeProcessingTimeMin = analyzedDataList.Average(i => i.DeserializeProcessingTimeMin);
        analyzedData.DeserializeProcessingTimeMax = analyzedDataList.Average(i => i.DeserializeProcessingTimeMax);
        analyzedData.DeserializeGCAllocAverage = analyzedDataList.Average(i => i.DeserializeGCAllocAverage);
        analyzedData.DeserializeGCAllocMin = analyzedDataList.Average(i => i.DeserializeGCAllocMin);
        analyzedData.DeserializeGCAllocMax = analyzedDataList.Average(i => i.DeserializeGCAllocMax);

        analyzedData.WriteResponseTimeAverage = analyzedDataList.Average(i => i.WriteResponseTimeAverage);
        analyzedData.WriteResponseTimeMin = analyzedDataList.Average(i => i.WriteResponseTimeMin);
        analyzedData.WriteResponseTimeMax = analyzedDataList.Average(i => i.WriteResponseTimeMax);
        analyzedData.WriteResponseTimeMedian = analyzedDataList.Average(i => i.WriteResponseTimeMedian);
        analyzedData.WriteResponseTime90th = analyzedDataList.Average(i => i.WriteResponseTime90th);
        analyzedData.WriteResponseTime95th = analyzedDataList.Average(i => i.WriteResponseTime95th);
        analyzedData.WriteResponseTime99th = analyzedDataList.Average(i => i.WriteResponseTime99th);
        analyzedData.WriteGCAllocAverage = analyzedDataList.Average(i => i.WriteGCAllocAverage);
        analyzedData.WriteGCAllocMin = analyzedDataList.Average(i => i.WriteGCAllocMin);
        analyzedData.WriteGCAllocMax = analyzedDataList.Average(i => i.WriteGCAllocMax);

        analyzedData.ReadResponseTimeAverage = analyzedDataList.Average(i => i.ReadResponseTimeAverage);
        analyzedData.ReadResponseTimeMin = analyzedDataList.Average(i => i.ReadResponseTimeMin);
        analyzedData.ReadResponseTimeMax = analyzedDataList.Average(i => i.ReadResponseTimeMax);
        analyzedData.ReadResponseTimeMedian = analyzedDataList.Average(i => i.ReadResponseTimeMedian);
        analyzedData.ReadResponseTime90th = analyzedDataList.Average(i => i.ReadResponseTime90th);
        analyzedData.ReadResponseTime95th = analyzedDataList.Average(i => i.ReadResponseTime95th);
        analyzedData.ReadResponseTime99th = analyzedDataList.Average(i => i.ReadResponseTime99th);
        analyzedData.ReadGCAllocAverage = analyzedDataList.Average(i => i.ReadGCAllocAverage);
        analyzedData.ReadGCAllocMin = analyzedDataList.Average(i => i.ReadGCAllocMin);
        analyzedData.ReadGCAllocMax = analyzedDataList.Average(i => i.ReadGCAllocMax);

        analyzedData.WriteThroughputPerSecond = analyzedDataList.Average(i => i.WriteThroughputPerSecond);
        analyzedData.ReadThroughputPerSecond = analyzedDataList.Average(i => i.ReadThroughputPerSecond);

        return analyzedData;
    }
}