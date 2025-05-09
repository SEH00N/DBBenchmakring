public class Analyze
{
    public List<double> SerializeTimeList;
    public List<double> SerializeGCAllocList;

    public List<double> DeserializeTimeList;
    public List<double> DeserializeGCAllocList;

    public List<double> WriteTimeList;
    public List<double> WriteGCAllocList;

    public List<double> ReadTimeList;
    public List<double> ReadGCAllocList;

    public Analyze(List<StatisticsData> testResultList)
    {
        double startGCAlloc = 0;
        double startTime = 0;

        SerializeTimeList = new List<double>();
        SerializeGCAllocList = new List<double>();

        DeserializeTimeList = new List<double>();
        DeserializeGCAllocList = new List<double>();

        WriteTimeList = new List<double>();
        WriteGCAllocList = new List<double>();

        ReadTimeList = new List<double>();
        ReadGCAllocList = new List<double>();

        foreach(StatisticsData statistics in testResultList)
        {
            switch(statistics.ActionType)
            {
                case EStatisticsActionType.SerializeStart:
                    startTime = statistics.TotalElapsedTime;
                    startGCAlloc = statistics.GCAlloc;
                    break;
                case EStatisticsActionType.DeserializeStart:
                    startTime = statistics.TotalElapsedTime;
                    startGCAlloc = statistics.GCAlloc;
                    break;
                case EStatisticsActionType.WriteStart:
                    startTime = statistics.TotalElapsedTime;
                    startGCAlloc = statistics.GCAlloc;
                    break;
                case EStatisticsActionType.ReadStart:
                    startTime = statistics.TotalElapsedTime;
                    startGCAlloc = statistics.GCAlloc;
                    break;

                case EStatisticsActionType.SerializeEnd:
                    SerializeTimeList.Add(statistics.TotalElapsedTime - startTime);
                    SerializeGCAllocList.Add(statistics.GCAlloc - startGCAlloc);
                    break;
                case EStatisticsActionType.DeserializeEnd:
                    DeserializeTimeList.Add(statistics.TotalElapsedTime - startTime);
                    DeserializeGCAllocList.Add(statistics.GCAlloc - startGCAlloc);
                    break;
                case EStatisticsActionType.WriteEnd:
                    WriteTimeList.Add(statistics.TotalElapsedTime - startTime);
                    WriteGCAllocList.Add(statistics.GCAlloc - startGCAlloc);
                    break;
                case EStatisticsActionType.ReadEnd:
                    ReadTimeList.Add(statistics.TotalElapsedTime - startTime);
                    ReadGCAllocList.Add(statistics.GCAlloc - startGCAlloc);
                    break;
            }
        }
    }
}