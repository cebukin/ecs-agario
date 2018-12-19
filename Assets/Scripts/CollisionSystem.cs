using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(CandidatesSystem))]
public class CollisionSystem : JobComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    [Inject] CandidatesSystem _candidatesSystem;

    [BurstCompile]
    struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Position> Positions;
        public ComponentDataArray<Size> Sizes;
        public float MaxPlayerSize;

        public void Execute(int index)
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }

                Size sizeA = Sizes[index];
                Size sizeB = Sizes[i];

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

                sizeA.Value = math.min(sizeA.Value + sizeB.Value, MaxPlayerSize);
                sizeB.Value = 0.0f; // will be destroyed later by another system

                Sizes[index] = sizeA;
                Sizes[i] = sizeB;
            }
        }

        bool IsColliding(int indexA, int indexB)
        {
            float distance = math.distance(Positions[indexA].Value, Positions[indexB].Value);
            float maxRadius = math.max(Sizes[indexA].Value / 2.0f, Sizes[indexB].Value / 2.0f);
            return distance < maxRadius;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collisionJob = new CollisionJob
        {
            Positions = m_Data.Position,
            Sizes = m_Data.Size,
            MaxPlayerSize = Bootstrap.Settings.PlayerMaxSize
        };

        return collisionJob.Schedule(m_Data.Length, 64, inputDeps);
    }
}
