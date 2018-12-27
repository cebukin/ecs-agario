using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

[UpdateAfter(typeof(GridSystem))]
public class PostGridSystem : JobComponentSystem
{
    public struct SpatialData
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] protected SpatialData _mSpatialData;
    [Inject] protected GridSystem _gridSystem;
    protected NativeArray<Size> _sizeCopy;
    protected NativeArray<Position> _positionsCopy;

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

    protected void Setup()
    {
        CleanUp();
        _sizeCopy = new NativeArray<Size>(_mSpatialData.Length, Allocator.TempJob);
        _positionsCopy = new NativeArray<Position>(_mSpatialData.Length, Allocator.TempJob);
    }
}
