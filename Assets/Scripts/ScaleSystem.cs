using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[UpdateAfter(typeof(DestructionSystem))]
public class ScaleSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateScaleJob : IJobProcessComponentData<Size, Scale>
    {
        public float dt;

        public void Execute([ReadOnly] ref Size size, ref Scale scale)
        {
            float progress = 10*dt;
            float3 newScale = new float3
            {
                x = math.lerp(scale.Value.x, size.Value, progress),
                y = math.lerp(scale.Value.y, size.Value, progress),
                z = math.lerp(scale.Value.z, size.Value, progress)
            };

            scale = new Scale { Value = newScale };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var updateScaleJob = new UpdateScaleJob
        {
            dt = Time.deltaTime
        };
        return updateScaleJob.Schedule(this, inputDeps);
    }
}
