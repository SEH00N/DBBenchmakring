[Serializable]
public class AnalyzedData
{
    public double SerializeProcessingTimeAverage { get; set; }
    public double SerializeProcessingTimeMin { get; set; }
    public double SerializeProcessingTimeMax { get; set; }

    public double DeserializeProcessingTimeAverage { get; set; }
    public double DeserializeProcessingTimeMin { get; set; }
    public double DeserializeProcessingTimeMax { get; set; }

    public double SerializeGCAllocAverage { get; set; }
    public double SerializeGCAllocMin { get; set; }
    public double SerializeGCAllocMax { get; set; }

    public double DeserializeGCAllocAverage { get; set; }
    public double DeserializeGCAllocMin { get; set; }
    public double DeserializeGCAllocMax { get; set; }

    public double WriteGCAllocAverage { get; set; }
    public double WriteGCAllocMin { get; set; }
    public double WriteGCAllocMax { get; set; }

    public double ReadGCAllocAverage { get; set; }
    public double ReadGCAllocMin { get; set; }
    public double ReadGCAllocMax { get; set; }

    public double WriteResponseTimeAverage { get; set; }
    public double WriteResponseTimeMin { get; set; }
    public double WriteResponseTimeMax { get; set; }
    public double WriteResponseTimeMedian { get; set; }
    public double WriteResponseTime90th { get; set; }
    public double WriteResponseTime95th { get; set; }
    public double WriteResponseTime99th { get; set; }

    public double ReadResponseTimeAverage { get; set; }
    public double ReadResponseTimeMin { get; set; }
    public double ReadResponseTimeMax { get; set; }
    public double ReadResponseTimeMedian { get; set; }
    public double ReadResponseTime90th { get; set; }
    public double ReadResponseTime95th { get; set; }
    public double ReadResponseTime99th { get; set; }
    
    public double WriteThroughputPerSecond { get; set; }
    public double ReadThroughputPerSecond { get; set; }

    public double TotalTime { get; set; }
    public double TotalGCAlloc { get; set; }
    public double SerializedDataSize { get; set; }
}
