﻿using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Random = UnityEngine.Random;

[AlwaysUpdateSystem]
public class FoodSpawnSystem : ComponentSystem
{
    public struct Data
    {
        public readonly int Length;
        public ComponentDataArray<Food> Food;
    }

    [Inject] Data m_Data;

    protected override void OnUpdate()
    {
        Settings settings = Bootstrap.Settings;
        int quantityToSpawn = settings.FoodCount - m_Data.Length;

        for (int i = 0; i < quantityToSpawn; i++)
        {
            PostUpdateCommands.CreateEntity(Bootstrap.FoodArchetype);

            float possibleSize = settings.ArenaSize * 10 * 0.9f;

            PostUpdateCommands.SetComponent(new Position
            {
                Value = new float3(
                    Random.Range(- possibleSize / 2.0f, possibleSize / 2.0f),
                    Random.Range(- possibleSize / 2.0f, possibleSize / 2.0f),
                    0.0f
                )
            });
            PostUpdateCommands.SetComponent(new Scale { Value = new float3(1.0f, 1.0f, 1.0f) });
            PostUpdateCommands.SetComponent(new Size { Value = settings.FoodSize });
            PostUpdateCommands.SetComponent(
                new Scale {
                    Value = new float3(settings.FoodSize,
                        settings.FoodSize,
                        settings.FoodSize)
                }
            );
            PostUpdateCommands.AddSharedComponent(Bootstrap.FoodLook);
        }
    }
}
