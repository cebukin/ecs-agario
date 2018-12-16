using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

[UpdateBefore(typeof(MoveSystem))]
public class PlayerInputSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<PlayerInput> PlayerInput;
        public ComponentDataArray<Heading> Heading;
    }

    [Inject] Data m_Data;
    protected override void OnUpdate()
    {
        for (int i = 0; i < m_Data.Length; i++)
        {
            float3 heading = new float3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
            m_Data.Heading[i] = new Heading { Value = heading };
        }
    }
}
