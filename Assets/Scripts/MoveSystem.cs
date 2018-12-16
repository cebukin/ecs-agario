using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

public class MoveSystem : JobComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Heading> Heading;
        public ComponentDataArray<Size> Size;
    }
    
    [Inject] private Data m_Data;

    [BurstCompile]
    struct CalculatePosition : IJobParallelFor
    {
        public ComponentDataArray<Position> positions;
        [ReadOnly] public ComponentDataArray<Heading> headings;
        [ReadOnly] public ComponentDataArray<Size> sizes;

        public float initialPlayerSize;
        public float playerMaxSpeed;
        public float dt;

        public void Execute(int index)
        {
            float speed = (initialPlayerSize / sizes[index].Value) * playerMaxSpeed;
            
            float3 position = positions[index].Value;
            position += dt * headings[index].Value * speed;
            
           positions[index] = new Position {Value = position};
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Settings settings = Bootstrap.Settings;

        var calculatePositionsJob = new CalculatePosition
        {
            positions = m_Data.Position,
            headings = m_Data.Heading,
            sizes = m_Data.Size,
            initialPlayerSize = settings.PlayerInitialSize,
            playerMaxSpeed = settings.PlayerMaxSpeed,
            dt = Time.deltaTime
        };

        return calculatePositionsJob.Schedule(m_Data.Length, 64, inputDeps);
    }
}
