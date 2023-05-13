﻿using System;
using System.Collections.Generic;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using CoreLib.Util.Extensions;
using PugTilemap;
using Rewired;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PainMod;

public class CustomStateSystem : MonoBehaviour, IPseudoServerSystem, IStateRequester
{
    public static CustomStateSystem instance;
    private World serverWorld;
    private EntityQuery entityQuery;
    private EntityQuery markerQuery;
    //private EntityQuery shamanQuery;
    public const string STATE_ID = "PainMod:OctopusState1";
    public static StateID octopusFirstState = SystemModule.GetModStateId(STATE_ID);
    private static NativeArray<Entity> entitiesArray;
    private ModComponentDataFromEntity<OctopusModdedStateCD> octopusModStateGroup;
    private void Awake()
    {
        instance = this;
    }
    public void OnServerStarted(World world)
    {
        serverWorld = world;
        entityQuery = serverWorld.EntityManager.CreateEntityQuery(
            ComponentModule.ReadOnly<OctopusModdedStateCD>(),
            ComponentModule.ReadWrite<Translation>());
        markerQuery = serverWorld.EntityManager.CreateEntityQuery(
            new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentModule.ReadOnly<BossSpawnLocationCD>()
                },
                Any = Array.Empty<ComponentType>(),
                None = new[] { ComponentModule.ReadOnly<EntityDestroyedCD>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
        //shamanQuery = serverWorld.EntityManager.CreateEntity()
        
        foreach (Entity ent in markerQuery.ToEntityArray(Allocator.Temp))
        {
            Plugin.logger.LogInfo("Position of the enemy is " + serverWorld.EntityManager.GetModComponentData<BossSpawnLocationCD>(ent).bossID +
                                  " at " + serverWorld.EntityManager.GetModComponentData<Translation>(ent).Value);
            if(serverWorld.EntityManager.GetModComponentData<BossSpawnLocationCD>(ent).bossID == ObjectID.OctopusBoss)
                Constants.OctopusMarkerPos = serverWorld.EntityManager.GetModComponentData<Translation>(ent).Value;
        }

        hasSpawnedInEnemies = false;
    }

    public void OnServerStopped()
    {
        serverWorld = null;
    }

    public void OnCreate(World world)
    {
        octopusModStateGroup = new ModComponentDataFromEntity<OctopusModdedStateCD>(world.EntityManager);
        //for real
    }

    public bool ShouldUpdate(Entity entity, ref StateRequestData data, ref StateRequestContainers containers)
    {
        return octopusModStateGroup.HasComponent(entity);
    }

    private EntityQuery query;
    public bool OnUpdate(Entity entity, EntityCommandBuffer ecb, ref StateRequestData data, ref StateRequestContainers containers,
        ref StateInfoCD stateInfo)
    {
        // Does this entity have our custom component?
        if (!octopusModStateGroup.HasComponent(entity)) return false;

        // Get needed data
        HealthCD healthCd = containers._healthGroup[entity];
        OctopusModdedStateCD OctopusStateCd = octopusModStateGroup[entity];
        float healthPercent = healthCd.health / (float) healthCd.maxHealth;
        Plugin.logger.LogInfo("Health percent is " + healthPercent + " and ratio to enter state is " + OctopusStateCd.HpRatioToEnterState);
        // If the entity has too low HP enter flee state
        if (stateInfo.currentState == octopusFirstState)
        {
            stateInfo.newState = octopusFirstState;
            
            
            
            if (allEnemiesDead)
            {
                stateInfo.newState = StateID.Teleport;
                return true;
            }
            // By returning true here we signal that the 'stateInfo' field has changed
            return false;
        }
        if (stateInfo.currentState != octopusFirstState &&
            healthPercent < OctopusStateCd.HpRatioToEnterState)
        {
            stateInfo.newState = octopusFirstState;
            // By returning true here we signal that the 'stateInfo' field has changed
            return true;
        }
        
        
        // Nothing changed
        return false;   
    }

    private float3[] positionsRelative = {new float3(4,0,1), new float3(12,0, 1), new float3(12,0,10), new float3(10,0, -9), new float3(6,0,5),
        new float3(-3, 0, 3), new float3(-9, 0, 1), new float3(1, 0, -5), new float3(-12, 0, 8), new float3(-6, 0, 6), new float3(-3, 0, -9),
        new float3(-3, 0, -14), new float3(-10, 0, -12), new float3(6, 0, -6), new float3(5, 0, -12)};
    private bool hasSpawnedInEnemies = false;
    private float2 octopusBossLurkSpot;
    private bool hasGottenLurkSpot = false;
    private EntityCommandBuffer EntityCommandBuffer;
    private bool allEnemiesDead;
    private void FixedUpdate()
    {
        if (serverWorld == null) return;
        // Execute our query and itterate the entities
        entitiesArray = entityQuery.ToEntityArray(Allocator.Temp);
        foreach (Entity e in entitiesArray)
        {
            StateInfoCD stateInfo = serverWorld.EntityManager.GetModComponentData<StateInfoCD>(e);
            
            //Plugin.logger.LogInfo("Current state is " + stateInfo.currentState);
            if (stateInfo.currentState == octopusFirstState)
            {
                if (!hasSpawnedInEnemies)
                {
                    Plugin.logger.LogInfo("IN OCTOPUS MODDED STATE");
                    /*EntityQuery query2 = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PugDatabase.DatabaseBankCD>());
                    
                    Entity entity = EntityUtility.CreateEntity(EntityCommandBuffer,Constants.OctopusMarkerPos + positionsRelative[0] - Manager._instance.player.WorldPosition.ToFloat3()
                        , ObjectID.CrabEnemy, 1, query2.GetSingleton<PugDatabase.DatabaseBankCD>().databaseBankBlob, 0);
                    EntityCommandBuffer.AddModComponent<MustBeDestroyedForOctopusLeaveStateCD>(entity);*/
                    //spawn in the enemies
                    
                    SpawnInvincibilityEntities(4);
                    /*EntityCommandBuffer ecb2 = new EntityCommandBuffer(Allocator.Temp);
                    ecb2.AddModComponent<CantBeAttackedCD>(e);
                    ecb2.Playback(Manager.ecs.ServerWorld.EntityManager);
                    ecb2.Dispose();*/
                    /* for (int i = 0; i < positionsRelative.Length; i++)
                     {
                         Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.CrabEnemy,
                             (Constants.OctopusMarkerPos + positionsRelative[i]) - Manager._instance.player.WorldPosition.ToFloat3());
                     }*/
                    Plugin.logger.LogInfo("Relative to player is " + ((Constants.OctopusMarkerPos + positionsRelative[0]) - Manager._instance.player.WorldPosition.ToFloat3()).ToString());
                    hasSpawnedInEnemies = true;
                }
                query = serverWorld.EntityManager.CreateEntityQuery(
                    ComponentModule.ReadOnly<MustBeDestroyedForOctopusLeaveStateCD>(),
                    ComponentModule.ReadWrite<Translation>());
                int enemiesLeft = 0;
                foreach (var ent in query.ToEntityArray(Allocator.Temp))
                { 
                    if (serverWorld.EntityManager.GetModComponentData<HealthCD>(ent).health > 0)
                        enemiesLeft++;
                } 
                Plugin.logger.LogInfo("Amount of entities with this thing is " + enemiesLeft);
                allEnemiesDead = enemiesLeft == 0;
            }
            else if (stateInfo.currentState == StateID.OctopusBossLurkingBelow)
            {
                if (!hasGottenLurkSpot)
                {
                    //hasSpawnedInEnemies = false;
                }
                //+10 and -5
                //octopus boss asleep spawn
            }
        }
    }

    private unsafe void SpawnInvincibilityEntities(int amount)
    {
        Plugin.logger.LogInfo("Testing that this works");
        EntityQuery query2 = Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PugDatabase.DatabaseBankCD>());
        BlobAssetReference<PugDatabase.PugDatabaseBank> bank = query2.GetSingleton<PugDatabase.DatabaseBankCD>()
            .databaseBankBlob;
        EntityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Plugin.logger.LogInfo("Entity command buffer data null: " + (EntityCommandBuffer.m_Data == null));
        List<float3> posLeft = new List<float3>(positionsRelative);
        for (int i = 0; i < amount; i++)
        {
            //rand gen
            int rand = Random.Range(0, posLeft.Count);
            /*Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.CrabEnemy,
                (Constants.OctopusMarkerPos + posLeft[rand]) - Manager._instance.player.WorldPosition.ToFloat3());*/
            
            //testing with server side version (not working)
            Entity crab = EntityUtility.CreateEntity(EntityCommandBuffer,Constants.OctopusMarkerPos + posLeft[rand]
                , ObjectID.CrabEnemy, 1, bank, 0);
            EntityCommandBuffer.AddModComponent<MustBeDestroyedForOctopusLeaveStateCD>(crab);
            posLeft.Remove(posLeft[rand]);
        }
        EntityCommandBuffer.Playback(Manager.ecs.ServerWorld.EntityManager);
        EntityCommandBuffer.Dispose();
    }

    public int priority => SystemModule.HIGHEST_PRIORITY;
    //public void SetWorld(World world) {serverWorld = world;}

    //public void RemoveWorld() { serverWorld = null; }
}