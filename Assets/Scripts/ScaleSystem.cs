using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(CollisionSystem))]
public class ScaleSystem : JobComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Size> Size;
        public ComponentDataArray<Scale> Scale;
        public ComponentDataArray<Player> Player;
    }

    [BurstCompile]
    public struct UpdateScaleJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Size> sizes;
        public ComponentDataArray<Scale> scales;

        public void Execute(int index)
        {
            float size = sizes[index].Value;
            scales[index] = new Scale { Value = new float3(size, size, size) };
        }
    }

    [Inject] Data m_Data;
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var updateScaleJob = new UpdateScaleJob
        {
            sizes = m_Data.Size,
            scales = m_Data.Scale
        };

        return updateScaleJob.Schedule(m_Data.Length, 64, inputDeps);
    }
}
