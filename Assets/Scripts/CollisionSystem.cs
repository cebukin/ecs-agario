using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

public class CollisionSystem : PostGridSystem
{
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

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Setup();

        var copySizesJob = new CopyComponentData<Size>
        {
            Source = _mSpatialData.Size,
            Results = _sizeCopy
        };

        var copyPositionsJob = new CopyComponentData<Position>
        {
            Source = _mSpatialData.Position,
            Results = _positionsCopy
        };

        var copySizesJobHandle = copySizesJob.Schedule(_mSpatialData.Length, 64, inputDeps);
        var copyPositionsJobHandle = copyPositionsJob.Schedule(_mSpatialData.Length, 64, inputDeps);

        var copyBarrier = JobHandle.CombineDependencies(copyPositionsJobHandle, copySizesJobHandle);

        var collisionJob = new CollisionJob
        {
            Positions = _positionsCopy,
            Sizes = _sizeCopy,
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
