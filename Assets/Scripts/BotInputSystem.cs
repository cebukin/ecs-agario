using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

public class BotInputSystem : PostGridSystem
{
    public struct BotData
    {
        public readonly int Length;
        public ComponentDataArray<BotInput> BotInput;
        public ComponentDataArray<Heading> Heading;
    }

    [Inject] BotData m_BotData;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
    }
}
