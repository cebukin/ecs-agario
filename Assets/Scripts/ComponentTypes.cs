﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct PlayerInput : IComponentData {}
public struct BotInput : IComponentData {}

public struct Destroy : IComponentData
{
    public Entity Entity;
}

public struct Heading : IComponentData
{
    public float3 Value;
}

public struct Size : IComponentData
{
    public int Value;
}

public struct Food : IComponentData {}

public struct Player : IComponentData {}
