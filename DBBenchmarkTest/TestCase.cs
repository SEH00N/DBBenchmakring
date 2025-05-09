using System.Diagnostics;

public abstract class TestCase
{
    public Stopwatch stopwatch = null;
    protected int counter = 0;

    public abstract double GetSerializedDataSize();

    public TestCase()
    {
        stopwatch = new Stopwatch();
        counter = 0;
    }

    public async Task StartTest(ETestType testType, int loopCount)
    {
        stopwatch.Start();
        counter = 0;
        
        for(int i = 0; i < loopCount; i++)
        {
            Initialize();

            if (!GC.TryStartNoGCRegion(1024 * 1024 * 5)) // 5MB 할당
                throw new Exception("No GC Region을 시작할 수 없습니다.");

            try {
                if(testType == ETestType.Read)
                {
                    await Read();
                    await Deserialize();
                }
                else if(testType == ETestType.Write)
                {
                    await Serialize();
                    await Write();
                }
                else if(testType == ETestType.Alternately)
                {
                    await Read();
                    await Deserialize();
                    await Serialize();
                    await Write();
                }
            }
            finally {
                GC.EndNoGCRegion();
            }

            counter++;
        }
    }

    protected virtual void Initialize()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    protected abstract Task SerializeInternal();
    public async Task Serialize()
    {
        Statistics.Record(EStatisticsActionType.SerializeStart);
        await SerializeInternal();
        Statistics.Record(EStatisticsActionType.SerializeEnd);
    }

    protected abstract Task DeserializeInternal();
    public async Task Deserialize()
    {
        Statistics.Record(EStatisticsActionType.DeserializeStart);
        await DeserializeInternal();
        Statistics.Record(EStatisticsActionType.DeserializeEnd);
    }

    protected abstract Task WriteInternal();
    public async Task Write()
    {
        Statistics.Record(EStatisticsActionType.WriteStart);
        await WriteInternal();
        Statistics.Record(EStatisticsActionType.WriteEnd);
    }

    protected abstract Task ReadInternal();
    public async Task Read()
    {   
        Statistics.Record(EStatisticsActionType.ReadStart);
        await ReadInternal();
        Statistics.Record(EStatisticsActionType.ReadEnd);
    }
}