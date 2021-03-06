﻿using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

public class CollisionSystem : PostGridSystem
{
    [Inject] CollisionBarrierSystem _barrierSystem;

    //[BurstCompile]
    struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Position> Positions;
        [ReadOnly] public NativeMultiHashMap<int, int> Grid;
        [NativeDisableParallelForRestriction] public NativeArray<Size> Sizes;
        [ReadOnly] public NativeArray<Entity> Entities;
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public int MaxPlayerSize;
        public float CellSize;

        public void Execute(int index)
        {
            float3 position = Positions[index].Value;
            int cellGridHash = Util.Hash(position, CellSize);

            int otherItem;
            NativeMultiHashMapIterator<int> iterator;

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
                sizeA.Value = (int) math.min(sizeAValue + sizeBValue, MaxPlayerSize);
                CommandBuffer.CreateEntity(indexA);
                CommandBuffer.AddComponent(indexA, new Destroy { Entity = Entities[indexB]});
            }
            else
            {
                sizeB.Value = (int) math.min(sizeAValue + sizeBValue, MaxPlayerSize);
                CommandBuffer.CreateEntity(indexA);
                CommandBuffer.AddComponent(indexA, new Destroy { Entity = Entities[indexA]});
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

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var copyBarrier = Setup(inputDeps);

        var collisionJob = new CollisionJob
        {
            Positions = _positionsCopy,
            Sizes = _sizeCopy,
            Entities = _entitiesCopy,
            CommandBuffer = _barrierSystem.CreateCommandBuffer().ToConcurrent(),
            MaxPlayerSize = Bootstrap.Settings.PlayerMaxSize,
            Grid = _gridSystem.Grid,
            CellSize = Bootstrap.Settings.CellSize
        };

        var collisionJobHandle = collisionJob.Schedule(_mSpatialData.Length, 64, copyBarrier);

        var copySizesBackJob = new CopyArrayToComponentData<Size>
        {
            Source = _sizeCopy,
            Results = _mSpatialData.Size
        };

        return copySizesBackJob.Schedule(_mSpatialData.Length, 64, collisionJobHandle);
    }
}
