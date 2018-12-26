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
    [Inject] GridSystem _gridSystem;
    NativeArray<Size> _sizeCopy;
    NativeArray<Position> _positionsCopy;

    protected override void OnDestroyManager()
    {
        CleanUp();
    }

    void CleanUp()
    {
        if (_sizeCopy.IsCreated)
        {
            _sizeCopy.Dispose();
        }

        if (_positionsCopy.IsCreated)
        {
            _positionsCopy.Dispose();
        }
    }

    [BurstCompile]
    struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Position> Positions;
        [ReadOnly] public NativeMultiHashMap<int, int> Grid;
        [NativeDisableParallelForRestriction] public NativeArray<Size> Sizes;

        public float MaxPlayerSize;
        public float CellSize;

        public void Execute(int index)
        {
            float3 position = Positions[index].Value;
            int cellGridHash = Util.Hash(position, CellSize);

            int otherItem;
            var iterator = new NativeMultiHashMapIterator<int>();

            if (Grid.TryGetFirstValue(cellGridHash, out otherItem, out iterator))
            {
                do
                {
                    CheckCollision(index, otherItem);
                } while (Grid.TryGetNextValue(out otherItem, ref iterator));
            }
        }

        void CheckCollision(int indexA, int indexB)
        {
            // we skip if indexA < indexB to avoid checking the same collision twice
            if (indexA <= indexB || !IsColliding(indexA, indexB))
            {
                return;
            }

            Size sizeA = Sizes[indexA];
            Size sizeB = Sizes[indexB];

            float sizeAValue = sizeA.Value;
            float sizeBValue = sizeB.Value;

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
            float distance = math.distance(Positions[indexA].Value, Positions[indexB].Value);
            float maxRadius = math.max(Sizes[indexA].Value / 2.0f, Sizes[indexB].Value / 2.0f);
            return distance < maxRadius;
        }
    }

    [BurstCompile]
    struct CopyArrayToComponentData<T> : IJobParallelFor
        where T : struct, IComponentData
    {
        [ReadOnly] public NativeArray<T> Source;
        public ComponentDataArray<T> Results;

        public void Execute(int index)
        {
            Results[index] = Source[index];
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CleanUp();
        _sizeCopy = new NativeArray<Size>(m_Data.Length, Allocator.TempJob);
        _positionsCopy = new NativeArray<Position>(m_Data.Length, Allocator.TempJob);

        var copySizesJob = new CopyComponentData<Size>
        {
            Source = m_Data.Size,
            Results = _sizeCopy
        };

        var copyPositionsJob = new CopyComponentData<Position>
        {
            Source = m_Data.Position,
            Results = _positionsCopy
        };

        var copySizesJobHandle = copySizesJob.Schedule(m_Data.Length, 64, inputDeps);
        var copyPositionsJobHandle = copyPositionsJob.Schedule(m_Data.Length, 64, inputDeps);

        var copyBarrier = JobHandle.CombineDependencies(copyPositionsJobHandle, copySizesJobHandle);

        var collisionJob = new CollisionJob
        {
            Positions = _positionsCopy,
            Sizes = _sizeCopy,
            MaxPlayerSize = Bootstrap.Settings.PlayerMaxSize,
            Grid = _gridSystem.Grid,
            CellSize = Bootstrap.Settings.CellSize
        };

        var collisionJobHandle = collisionJob.Schedule(m_Data.Length, 64, copyBarrier);

        var copySizesBackJob = new CopyArrayToComponentData<Size>
        {
            Source = _sizeCopy,
            Results = m_Data.Size
        };

        return copySizesBackJob.Schedule(m_Data.Length, 64, collisionJobHandle);
    }
}
