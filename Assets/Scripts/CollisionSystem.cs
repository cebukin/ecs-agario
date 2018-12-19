using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
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
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<int> CandidatesA;
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<int> CandidatesB;

        [NativeDisableParallelForRestriction]
        public ComponentDataArray<Size> Sizes;

        public float MaxPlayerSize;

        public void Execute(int index)
        {
            int indexA = CandidatesA[index];
            int indexB = CandidatesB[index];
            Size sizeA = Sizes[indexA];
            Size sizeB = Sizes[indexB];

            float sizeAValue = sizeA.Value;
            float sizeBValue = sizeB.Value;

            if (!IsColliding(indexA, indexB))
            {
                return;
            }

            if (sizeAValue > sizeBValue)
            {
                sizeA.Value = math.min(sizeAValue + sizeBValue, MaxPlayerSize);
                sizeB.Value = 0.0f; // will be destroyed later by another system
            }
            else
            {
                sizeA.Value = 0.0f; // will be destroyed later by another system
                sizeB.Value = math.min(sizeAValue + sizeBValue, MaxPlayerSize);
            }

            Sizes[indexA] = sizeA;
            Sizes[indexB] = sizeB;
        }

        bool IsColliding(int indexA, int indexB)
        {
            float distance = Distance(indexA, indexB);
            float maxRadius = math.max(Sizes[indexA].Value / 2.0f, Sizes[indexB].Value / 2.0f);
            return distance < maxRadius;
        }

        float Distance(int indexA, int indexB)
        {
            return math.distance(Positions[indexA].Value, Positions[indexB].Value);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var candidatePairs = _candidatesSystem.CandidatePairs;
        int nPairs = candidatePairs.Length;
        NativeArray<int> candidatesA = new NativeArray<int>(nPairs, Allocator.TempJob);
        NativeArray<int> candidatesB = new NativeArray<int>(nPairs, Allocator.TempJob);

        for (int i = 0; i < nPairs; i++)
        {
            candidatesA[i] = candidatePairs[i].x;
            candidatesB[i] = candidatePairs[i].y;
        }

        var collisionJob = new CollisionJob
        {
            Positions = m_Data.Position,
            Sizes = m_Data.Size,
            MaxPlayerSize = Bootstrap.Settings.PlayerMaxSize,
            CandidatesA = candidatesA,
            CandidatesB = candidatesB
        };

        return collisionJob.Schedule(nPairs, 64, inputDeps);
    }
}
