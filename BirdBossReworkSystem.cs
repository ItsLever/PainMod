using System;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PainMod;

public class BirdBossReworkSystem : MonoBehaviour, IPseudoServerSystem, IStateRequester
{
    private World serverWorld;
    public const string BIRD_BOSS_REWORKED_BEAM_STATE = "PainMod:BirdBossReworkedBeamState";
    public static StateID birdBossReworkState = SystemModule.GetModStateId(BIRD_BOSS_REWORKED_BEAM_STATE);
    public const string STATE_ID_PING_PONG = "PainMod:BirdPingPongState";
    public static StateID birdPingPongState = SystemModule.GetModStateId(STATE_ID_PING_PONG);
    private ModComponentDataFromEntity<BirdBossReworkCD> birdModStateGroup;
    public void OnServerStarted(World world)
    {
        serverWorld = world;
    }

    public void OnServerStopped()
    {
        serverWorld = null;
    }

    public void OnCreate(World world)
    {
        birdModStateGroup = new ModComponentDataFromEntity<BirdBossReworkCD>(world.EntityManager);
    }

    public bool ShouldUpdate(Entity entity, ref StateRequestData data, ref StateRequestContainers containers)
    {
        return birdModStateGroup.HasComponent(entity);
    }

    public bool OnUpdate(Entity entity, EntityCommandBuffer ecb, ref StateRequestData data, ref StateRequestContainers containers,
        ref StateInfoCD stateInfo)
    {
        return false;
    }

    private bool lastFrameHadFullHealth = true;
    private static float range;
    private void FixedUpdate()
    {
        if (serverWorld == null) return;
        EntityQuery birdQuery =
            Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentModule.ReadOnly<BirdBossReworkCD>());
        foreach (var ent in birdQuery.ToEntityArray(Allocator.Temp))
        {
            StateInfoCD stateInfo = serverWorld.EntityManager.GetModComponentData<StateInfoCD>(ent);
            BirdBossReworkCD reworkCd = serverWorld.EntityManager.GetModComponentData<BirdBossReworkCD>(ent);
            range = reworkCd.radiusToBorder - 1;
            HealthCD healthCd = serverWorld.EntityManager.GetModComponentData<HealthCD>(ent);
            if (!healthCd.HasFullHealth && lastFrameHadFullHealth)
            {
                TryDeleteBirdBossBarriers();
                CreateBirdBossBarriers(reworkCd);
            }
            else if(healthCd.HasFullHealth)
                TryDeleteBirdBossBarriers();
            RunThisEveryUpdate(ent);
            if (stateInfo.currentState == StateID.Teleport)
            {
                TeleportStateCD teleportStateCd = serverWorld.EntityManager.GetModComponentData<TeleportStateCD>(ent);
                if (GetDistance(Constants.BirdMarkerPos, teleportStateCd.targetDestination) >
                    reworkCd.radiusToBorder - 1)
                {
                    teleportStateCd.targetDestination = Constants.BirdMarkerPos + GetRandomInCirclePlace(reworkCd.radiusToBorder-1);
                    serverWorld.EntityManager.SetModComponentData(ent, teleportStateCd);
                }
            }

            lastFrameHadFullHealth = healthCd.HasFullHealth;
        }
        EntityQuery stoneQuery = Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentModule.ReadOnly<BirdStoneCD>(), ComponentModule.ReadWrite<Translation>());
        foreach (var stone in stoneQuery.ToEntityArray(Allocator.Temp))
        {
            Translation translation = serverWorld.EntityManager.GetModComponentData<Translation>(stone);
            if(GetDistance(Constants.BirdMarkerPos, translation.Value) > range)
                serverWorld.EntityManager.DestroyEntity(stone);
        }
    }

    private void CreateBirdBossBarriers(BirdBossReworkCD reworkCd)
    {
        EntityQuery query2 = Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentModule.ReadOnly<PugDatabase.DatabaseBankCD>());
        BlobAssetReference<PugDatabase.PugDatabaseBank> bank = query2.GetSingleton<PugDatabase.DatabaseBankCD>()
            .databaseBankBlob;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        float x = 0;
        for (int i = 0; i < reworkCd.amountOfShockBarriers; i++)
        {
            x += 2 * (float)Math.PI / reworkCd.amountOfShockBarriers;
            Entity shock = EntityUtility.CreateEntity(ecb, Constants.BirdMarkerPos + new float3(reworkCd.radiusToBorder*(float)Math.Cos(x),0, reworkCd.radiusToBorder*(float)Math.Sin(x))
                , Plugin.birdBossBarrier, 1, bank, 0);
            ecb.AddModComponent<BirdBarrierCD>(shock);
        }
        
        ecb.Playback(serverWorld.EntityManager);
        ecb.Dispose();
    }

    private void RunThisEveryUpdate(Entity owner)
    {
        EntityQuery barrierQuery = serverWorld.EntityManager.CreateEntityQuery(
            ComponentModule.ReadOnly<FactionCD>(),
            ComponentModule.ReadOnly<BirdBarrierCD>(),
            ComponentModule.ReadOnly<OwnerCD>(),
            ComponentModule.ReadWrite<Translation>());
        foreach (var barrier in barrierQuery.ToEntityArray(Allocator.Temp))
        {
            OwnerCD ownerCd = serverWorld.EntityManager.GetModComponentData<OwnerCD>(barrier);
            ownerCd.owner = owner;
            serverWorld.EntityManager.SetModComponentData(barrier, ownerCd);
            //FactionCD factionCd = serverWorld.EntityManager.GetModComponentData<FactionCD>(barrier);
            //factionCd.originalFaction = FactionID.AttacksAllButNotAttacked;
            //factionCd.faction = FactionID.AttacksAllButNotAttacked;
            //serverWorld.EntityManager.SetModComponentData(barrier, factionCd);
            AttackContinuouslyCD attackContinuouslyCd =
                serverWorld.EntityManager.GetModComponentData<AttackContinuouslyCD>(barrier);
            attackContinuouslyCd.ignoreDamageReduction = true;
            attackContinuouslyCd.requiresElectricity = false;
            attackContinuouslyCd.damage = 900;
            serverWorld.EntityManager.SetModComponentData(barrier, attackContinuouslyCd);
            EntityUtility.InheritFaction(serverWorld, owner, barrier);
        }
    }

    private void TryDeleteBirdBossBarriers()
    {
        EntityQuery birdBossBarrierQuery =
            Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentModule.ReadOnly<BirdBarrierCD>());
        foreach (var ent in birdBossBarrierQuery.ToEntityArray(Allocator.Temp))
        {
            serverWorld.EntityManager.DestroyEntity(ent);
        }
    }

    private float3 GetRandomInCirclePlace(float maxRange)
    {
        float r1 = Random.value * (float)Math.PI*2;
        float r2 = Random.RandomRange(0, maxRange);
        return new float3(r2 * (float) Math.Cos(r1), 0, r2 * (float) Math.Sin(r1));
    }

    private float GetDistance(float3 first, float3 second)
    {
        double xComp = second.x - first.x;
        double yComp = second.y - first.y;
        return (float) Math.Sqrt(Math.Pow(xComp, 2) + Math.Pow(yComp, 2));
    }

    public int priority => SystemModule.HIGHER_PRIORITY;
}