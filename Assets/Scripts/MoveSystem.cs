using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

public class MoveSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Position> Position;
        public ComponentDataArray<Heading> Heading;
        public ComponentDataArray<Size> Size;
    }
    
    [Inject] private Data m_Data;
    
    protected override void OnUpdate()
    {
        Settings settings = Bootstrap.Settings;
        
        for (int i = 0; i < m_Data.Length; i++)
        {
            float speed = (settings.PlayerInitialSize / m_Data.Size[i].Value) * settings.PlayerMaxSpeed;
            
            float3 position = m_Data.Position[i].Value;
            position += Time.deltaTime * m_Data.Heading[i].Value * speed;
            
            m_Data.Position[i] = new Position {Value = position};
        }
    }
}
