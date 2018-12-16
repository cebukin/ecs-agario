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
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;


    protected override void OnUpdate()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            for (int j = i + 1; j < m_Data.Length; j++)
            {
                if (IsColliding(m_Data.Position[i], m_Data.Position[j], m_Data.Size[i], m_Data.Size[j]))
                {
                    Debug.LogError("IsColliding");
                }
            }
        }
    }

    bool IsColliding(Position pA, Position pB, Size sA, Size sB)
    {
        float distance = Mathf.Sqrt(
            (pA.Value.x - pB.Value.x) * (pA.Value.x - pB.Value.x) +
            (pA.Value.y - pB.Value.y) * (pA.Value.y - pB.Value.y)
        );

        float maxRadius = Math.Max(sA.Value / 2.0f, sB.Value / 2.0f);

        return distance < maxRadius;
    }
}
