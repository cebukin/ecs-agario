using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct PlayerInput : IComponentData {}

public struct Heading : IComponentData
{
    public float3 Value;
}
    
public struct Size : IComponentData
{
    public float Value;
}
