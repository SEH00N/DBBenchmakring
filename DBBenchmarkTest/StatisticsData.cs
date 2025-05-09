[Serializable]
public class StatisticsData
{
    public EStatisticsActionType ActionType { get; set; }
    public double TotalElapsedTime { get; set; }
    public double GCAlloc { get; set; }

    public StatisticsData(EStatisticsActionType actionType, double totalElapsedTime, double gcAlloc)
    {
        ActionType = actionType;
        TotalElapsedTime = totalElapsedTime;
        GCAlloc = gcAlloc;
    }

    public override string ToString()
    {
        return $"[{ActionType}] : {TotalElapsedTime}ms, {GCAlloc}mb";
    }
}
