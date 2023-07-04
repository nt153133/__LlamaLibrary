using System;
using System.Linq;
using System.Threading.Tasks;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.Helpers;

public class FishGuideHelper
{
    private static DateTime lastWindowCheck;

    public static FishGuide2Item[] CachedProgress;

    private static readonly TimeSpan CachePeriod = new(0, 0, 30);

    public static async Task<FishGuide2Item[]> CachedRead()
    {
        if (DateTime.Now - lastWindowCheck < CachePeriod)
        {
            return CachedProgress;
        }

        CachedProgress = await AgentFishGuide2.Instance.GetFishList();

        lastWindowCheck = DateTime.Now;

        return CachedProgress;
    }

    public static async Task<FishGuide2Item> GetFish(int fishId)
    {
        return (await CachedRead()).FirstOrDefault(i => i.FishItem == fishId);
    }

    public static FishGuide2Item GetFishSync(int fishId)
    {
        return CachedProgress.FirstOrDefault(i => i.FishItem == fishId);
    }
}