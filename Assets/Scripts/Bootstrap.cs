using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class Bootstrap
{
    public static EntityArchetype PlayerArchetype;
    public static EntityArchetype FoodArchetype;
    
    public static MeshInstanceRenderer PlayerLook;
    public static MeshInstanceRenderer FoodLook;
    
    public static Settings Settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        // This method creates archetypes for entities we will spawn frequently in this game.
        // Archetypes are optional but can speed up entity spawning substantially.

        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        // Create player archetype
        PlayerArchetype = entityManager.CreateArchetype(
            typeof(Position), typeof(PlayerInput), typeof(Size), typeof(Heading)
        );
        
        // Create player archetype
        FoodArchetype = entityManager.CreateArchetype(
            typeof(Position), typeof(Size)
        );
    }
    
    public static void NewGame()
    {
        // Access the ECS entity manager
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        // Create an entity based on the player archetype. It will get default-constructed
        // defaults for all the component types we listed.
        Entity player = entityManager.CreateEntity(PlayerArchetype);

        // We can tweak a few components to make more sense like this.
        entityManager.SetComponentData(player, new Position {Value = new float3(0.0f, 0.0f,0.0f)});
        //entityManager.SetComponentData(player, new Rotation {Value = quaternion.identity});
        entityManager.SetComponentData(player, new Size { Value = Settings.PlayerInitialSize });
        entityManager.SetComponentData(player, new Heading { Value = new float3(0.0f, 0.0f, 0.0f)} );

        // Finally we add a shared component which dictates the rendered look
        entityManager.AddSharedComponentData(player, PlayerLook);
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeAfterSceneLoad()
    {
        var settingsGO = GameObject.Find("Settings");
        if (settingsGO == null)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            return;
        }

        InitializeWithScene();
    }
    
    public static void InitializeWithScene()
    {
        var settingsGO = GameObject.Find("Settings");
        if (settingsGO == null)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            return;
        }
        
        Settings = settingsGO?.GetComponent<Settings>();
        if (!Settings)
        {
            throw new Exception("Missing Settings");
        }

        PlayerLook = GetLookFromPrototype("PlayerRenderPrototype");
        FoodLook = GetLookFromPrototype("FoodRenderPrototype");

        //EnemySpawnSystem.SetupComponentData(World.Active.GetOrCreateManager<EntityManager>());
        //World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();

        /*var sceneSwitcher = GameObject.Find("SceneSwitcher");
        if (sceneSwitcher != null)
        {*/
            NewGame();
        //}
    }
    
    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        InitializeWithScene();
    }

    private static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }
}
