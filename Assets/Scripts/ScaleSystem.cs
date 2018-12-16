using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[UpdateAfter(typeof(CollisionSystem))]
public class ScaleSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Size> Size;
        public ComponentDataArray<Scale> Scale;
        public ComponentDataArray<Player> Player;
    }

    [Inject] Data m_Data;
    protected override void OnUpdate()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            float size = m_Data.Size[i].Value;
            m_Data.Scale[i] = new Scale { Value = new float3(size, size, size) };
        }
    }
}
