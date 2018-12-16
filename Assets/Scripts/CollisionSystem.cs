using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(MoveSystem))]
public class CollisionSystem : ComponentSystem
{
    public struct PlayerData
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
        public ComponentDataArray<Player> Player;
    }

    [Inject] PlayerData _mPlayerData;

    public struct FoodData
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
        public ComponentDataArray<Food> Food;
        public EntityArray Entities;
    }

    [Inject] FoodData _mFoodData;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _mPlayerData.Length; i++)
        {
            for (int j = 0; j < _mFoodData.Length; j++)
            {
                if (!IsColliding(_mPlayerData.Position[i], _mFoodData.Position[j], _mPlayerData.Size[i],
                    _mFoodData.Size[j]))
                {
                    continue;
                }

                float size = _mPlayerData.Size[i].Value + _mFoodData.Size[j].Value;
                _mPlayerData.Size[i] = new Size { Value = size };
                PostUpdateCommands.DestroyEntity(_mFoodData.Entities[j]);
            }
        }
    }

    bool IsColliding(Position pA, Position pB, Size sA, Size sB)
    {
        float distance = math.distance(pA.Value, pB.Value);
        float maxRadius = math.max(sA.Value / 2.0f, sB.Value / 2.0f);
        return distance < maxRadius;
    }
}
