using System;
using System.Collections.Generic;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using CoreLib.Util.Extensions;
using PugTilemap;
using Rewired;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Color = System.Drawing.Color;
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
        hasTeleportOutOfState = false;
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
    public static bool hasTeleportOutOfState = false;
    public static bool hasStartedSecondState = false;
    public bool OnUpdate(Entity entity, EntityCommandBuffer ecb, ref StateRequestData data, ref StateRequestContainers containers,
        ref StateInfoCD stateInfo)
    {
        // Does this entity have our custom component?
        if (!octopusModStateGroup.HasComponent(entity)) return false;

        // Get needed data
        HealthCD healthCd = containers._healthGroup[entity];
        OctopusModdedStateCD OctopusStateCd = octopusModStateGroup[entity];
        DamageReductionCD damageReductionCd = containers._damageReductionGroup[entity];
        float healthPercent = healthCd.health / (float) healthCd.maxHealth;
        //Plugin.logger.LogInfo("Health percent is " + healthPercent + " and ratio to enter state is " + OctopusStateCd.HpRatioToEnterState);
        // If the entity has too low HP enter flee state
        if (stateInfo.currentState == octopusFirstState)
        {
            stateInfo.newState = octopusFirstState;
            if (allEnemiesDead)
            {
                ChatWindow_Patch.SendMessage("All enemies are dead now!", UnityEngine.Color.red);
                //stateInfo.newState = StateID.Teleport;
                damageReductionCd.reduction = 0;
                serverWorld.EntityManager.SetModComponentData(entity, damageReductionCd);
                stateInfo.LeaveState();
                stateInfo.EnterState(StateID.RangeAttack);
                return true;
            }
            // By returning true here we signal that the 'stateInfo' field has changed
            return true;
        }
        if (stateInfo.currentState != octopusFirstState &&
            healthPercent < OctopusStateCd.HpRatioToEnterState && !hasSpawnedInEnemies)
        {
            stateInfo.newState = octopusFirstState;
            // By returning true here we signal that the 'stateInfo' field has changed
            return true;
        }
        if (stateInfo.newState == StateID.OctopusBossSpawnTentacles && stateInfo.currentState!=StateID.OctopusBossSpawnTentacles)
        {
            if (existingTenticleCount >= OctopusStateCd.maxTentacleCap)
            {
                Plugin.logger.LogInfo("There are " + existingTenticleCount + " tentacles");
                float v = Random.value;
                if (v >= 0.5f)
                    stateInfo.newState = stateInfo.currentState;
                else
                    stateInfo.newState = StateID.Teleport;
                return true;
            }
            return false;
        }

        if (stateInfo.currentState != octopusFirstState && healthPercent < OctopusStateCd.HpRatioToEnterState2 && !hasStartedSecondState)
        {
            hasSpawnedInEnemies = false;
            OctopusStateCd.iteration = 2;
            serverWorld.EntityManager.SetModComponentData(entity, OctopusStateCd);
            hasStartedSecondState = true;
        }

        // Nothing changed
        return false;   
    }

    private float3[] positionsRelative = {new float3(4,0,1), new float3(12,0, 2), new float3(13,0,9), new float3(11,0, -8), new float3(6,0,5),
        new float3(-3, 0, 3), new float3(-10, 0, 1), new float3(-1, 0, -4), new float3(-13, 0, 8), new float3(-6, 0, 7), new float3(-4, 0, -8),
        new float3(-4, 0, -14), new float3(-11, 0, -11), new float3(6, 0, -5), new float3(4, 0, -12)};
    private bool hasSpawnedInEnemies = false;
    private float2 octopusBossLurkSpot;
    private bool hasGottenLurkSpot = false;
    private EntityCommandBuffer EntityCommandBuffer;
    private bool allEnemiesDead;
    private static int existingTenticleCount = 0;
    private void FixedUpdate()
    {
        if (serverWorld == null) return;
        // Execute our query and itterate the entities
        entitiesArray = entityQuery.ToEntityArray(Allocator.Temp);
        foreach (Entity e in entitiesArray)
        {
            StateInfoCD stateInfo = serverWorld.EntityManager.GetModComponentData<StateInfoCD>(e);
            OctopusModdedStateCD moddedStateCd = serverWorld.EntityManager.GetModComponentData<OctopusModdedStateCD>(e);
            DamageReductionCD reductionCd = serverWorld.EntityManager.GetModComponentData<DamageReductionCD>(e);
            
            //Plugin.logger.LogInfo("Current state is " + stateInfo.currentState);
            if (stateInfo.currentState == octopusFirstState)
            {
                if (!hasSpawnedInEnemies)
                {
                    Plugin.logger.LogInfo("IN OCTOPUS MODDED STATE");
                    if(moddedStateCd.iteration==1)
                        SpawnInvincibilityEntities(4);
                    else if (moddedStateCd.iteration==2)
                        SpawnInvincibilityEntities(7);
                    //do invincible stuff
                    reductionCd.reduction = 1000000;
                    serverWorld.EntityManager.SetModComponentData(e, reductionCd);
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
                EntityQuery q2 = serverWorld.EntityManager.CreateEntityQuery(
                    ComponentModule.ReadOnly<OctopusTenticleCounterCD>(),
                    ComponentModule.ReadWrite<Translation>());
                existingTenticleCount = 0;
                foreach (var ent2 in q2.ToEntityArray(Allocator.Temp))
                { 
                    if (serverWorld.EntityManager.GetModComponentData<HealthCD>(ent2).health > 0)
                        existingTenticleCount++;
                }
                Plugin.logger.LogInfo("Amount of tenticles with this thing is " + existingTenticleCount);
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
            Entity orb = EntityUtility.CreateEntity(EntityCommandBuffer,Constants.OctopusMarkerPos + posLeft[rand]
                , Plugin.octopusOrb, 1, bank, 0);
            EntityCommandBuffer.AddModComponent<MustBeDestroyedForOctopusLeaveStateCD>(orb);
            posLeft.Remove(posLeft[rand]);
        }
        EntityCommandBuffer.Playback(Manager.ecs.ServerWorld.EntityManager);
        EntityCommandBuffer.Dispose();
    }

    public int priority => SystemModule.HIGHEST_PRIORITY;
    //public void SetWorld(World world) {serverWorld = world;}

    //public void RemoveWorld() { serverWorld = null; }
}