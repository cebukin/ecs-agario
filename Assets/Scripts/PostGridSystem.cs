using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(GridSystem))]
public class PostGridSystem : JobComponentSystem
{
    public struct SpatialData
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] protected SpatialData _mSpatialData;
    [Inject] protected GridSystem _gridSystem;

    protected NativeArray<Size> _sizeCopy;
    protected NativeArray<Position> _positionsCopy;
    protected NativeArray<Entity> _entitiesCopy;

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

        if (_entitiesCopy.IsCreated)
        {
            _entitiesCopy.Dispose();
        }
    }

    protected JobHandle Setup(JobHandle inputDeps)
    {
        CleanUp();
        _sizeCopy = new NativeArray<Size>(_mSpatialData.Length, Allocator.TempJob);
        _positionsCopy = new NativeArray<Position>(_mSpatialData.Length, Allocator.TempJob);
        _entitiesCopy = new NativeArray<Entity>(_mSpatialData.Length, Allocator.TempJob);

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

        var copyEntitiesJob = new CopyEntities
        {
            Source = _mSpatialData.Entities,
            Results = _entitiesCopy
        };

        var copySizesJobHandle = copySizesJob.Schedule(_mSpatialData.Length, 64, inputDeps);
        var copyPositionsJobHandle = copyPositionsJob.Schedule(_mSpatialData.Length, 64, inputDeps);
        var copyEntitiesJobHandle = copyEntitiesJob.Schedule(_mSpatialData.Length, 64, inputDeps);

        return JobHandle.CombineDependencies(copyPositionsJobHandle, copySizesJobHandle, copyEntitiesJobHandle);
    }
}
