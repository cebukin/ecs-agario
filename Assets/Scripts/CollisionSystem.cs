using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(GridSystem))]
public class CollisionSystem : JobComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    [BurstCompile]
    struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Position> positions;
        public ComponentDataArray<Size> sizes;
        public float maxPlayerSize;

        public void Execute(int index)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }

                Size sizeA = sizes[index];
                Size sizeB = sizes[i];

                if (sizeB.Value > sizeA.Value)
                {
                    // we do this to avoid registering the collision twice
                    continue;
                }

                if (!IsColliding(index, i))
                {
                    continue;
                }

                // index grows bigger
                // i dies

                sizeA.Value = math.min(sizeA.Value + sizeB.Value, maxPlayerSize);
                sizeB.Value = 0.0f; // will be destroyed later by another system

                sizes[index] = sizeA;
                sizes[i] = sizeB;
            }
        }

        bool IsColliding(int indexA, int indexB)
        {
            float distance = math.distance(positions[indexA].Value, positions[indexB].Value);
            float maxRadius = math.max(sizes[indexA].Value / 2.0f, sizes[indexB].Value / 2.0f);
            return distance < maxRadius;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collisionJob = new CollisionJob
        {
            positions = m_Data.Position,
            sizes = m_Data.Size,
            maxPlayerSize = Bootstrap.Settings.PlayerMaxSize
        };

        return collisionJob.Schedule(m_Data.Length, 64, inputDeps);
    }
}
