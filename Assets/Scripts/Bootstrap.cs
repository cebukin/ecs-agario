﻿using System;
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
    public static EntityArchetype BotArchetype;

    public static MeshInstanceRenderer PlayerLook;
    public static MeshInstanceRenderer FoodLook;
    public static MeshInstanceRenderer BotLook;

    public static Settings Settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        PlayerArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Scale),
            typeof(PlayerInput),
            typeof(Size),
            typeof(Heading),
            typeof(Player)
        );

        FoodArchetype = entityManager.CreateArchetype(
            typeof(Position), typeof(Scale), typeof(Size), typeof(Food)
        );

        BotArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Scale),
            typeof(BotInput),
            typeof(Size),
            typeof(Heading),
            typeof(Player)
        );
    }

    public static void NewGame()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        Entity player = entityManager.CreateEntity(PlayerArchetype);

        entityManager.SetComponentData(player, new Position {Value = new float3(0.0f, 0.0f,0.0f)});
        entityManager.SetComponentData(player, new Size { Value = Settings.PlayerInitialSize });
        entityManager.SetComponentData(player, new Heading { Value = new float3(0.0f, 0.0f, 0.0f)} );
        entityManager.SetComponentData(player, new Scale
        {
            Value = new float3(Settings.PlayerInitialSize, Settings.PlayerInitialSize, Settings.PlayerInitialSize)
        });

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
        BotLook = GetLookFromPrototype("BotRenderPrototype");

        World.Active.GetOrCreateManager<CameraSystem>().SetupGameObjects();

        NewGame();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        InitializeWithScene();
    }

    static MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(proto);
        return result;
    }
}
