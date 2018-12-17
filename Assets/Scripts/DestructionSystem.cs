using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(CollisionSystem))]
public class DestructionSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentDataArray<Size> Size;
    }

    [Inject] Data m_Data;

    protected override void OnUpdate()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            if (m_Data.Size[i].Value < float.Epsilon)
            {
                PostUpdateCommands.DestroyEntity(m_Data.Entities[i]);
            }
        }
    }
}
