using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

[UpdateAfter(typeof(CollisionBarrierSystem))]
public class DestructionSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Destroy> Destroy;
        public EntityArray Entities;
    }

    [Inject] Data m_Data;

    protected override void OnUpdate()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            PostUpdateCommands.DestroyEntity(m_Data.Entities[i]);
            PostUpdateCommands.DestroyEntity(m_Data.Destroy[i].Entity);
        }
    }
}
