using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Random = UnityEngine.Random;

public class FoodSpawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Settings settings = Bootstrap.Settings;
        var foodEntities = GetEntities<Food>();
        int quantityToSpawn = settings.FoodCount - foodEntities.Length;

        for (int i = 0; i < quantityToSpawn; i++)
        {
            PostUpdateCommands.CreateEntity(Bootstrap.FoodArchetype);
            PostUpdateCommands.SetComponent(new Position
            {
                Value = new float3(
                    Random.Range(- settings.ArenaSize * 10 / 2.0f, settings.ArenaSize * 10 / 2.0f),
                    Random.Range(- settings.ArenaSize * 10 / 2.0f, settings.ArenaSize * 10 / 2.0f),
                    0.0f
                )
            });
            PostUpdateCommands.SetComponent(new Scale { Value = new float3(1.0f, 1.0f, 1.0f) });
            PostUpdateCommands.AddSharedComponent(Bootstrap.FoodLook);
        }
    }
}
