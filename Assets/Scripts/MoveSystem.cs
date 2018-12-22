using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

public class MoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct CalculatePosition : IJobProcessComponentData<Position, Heading, Size>
    {
        public float InitialPlayerSize;
        public float PlayerMaxSpeed;
        public float dt;
        public float ArenaSize;

        public void Execute(ref Position position, [ReadOnly] ref Heading heading, [ReadOnly] ref Size size)
        {
            float sizeRatio = InitialPlayerSize / size.Value;
            float speed = math.sqrt(sizeRatio) * PlayerMaxSpeed;

            float3 positionValue = position.Value;
            positionValue += dt * heading.Value * speed;

            float minValue = -ArenaSize / 2.0f + size.Value / 2.0f;
            float maxValue = ArenaSize / 2.0f - size.Value / 2.0f;

            positionValue.x = Mathf.Clamp(positionValue.x, minValue, maxValue);
            positionValue.y = Mathf.Clamp(positionValue.y, minValue, maxValue);

            position = new Position {Value = positionValue};
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Settings settings = Bootstrap.Settings;

        var calculatePositionsJob = new CalculatePosition
        {
            InitialPlayerSize = settings.PlayerInitialSize,
            PlayerMaxSpeed = settings.PlayerMaxSpeed,
            dt = Time.deltaTime,
            ArenaSize = settings.ArenaSize * 10
        };

        return calculatePositionsJob.Schedule(this, inputDeps);
    }
}
