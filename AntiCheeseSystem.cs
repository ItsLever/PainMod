using System;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace PainMod;

public class AntiCheeseSystem : MonoBehaviour, IPseudoServerSystem
{
    private World serverWorld;
    public AntiCheeseSystem(IntPtr ptr) : base(ptr) { }
    public void OnServerStarted(World world)
    {
        serverWorld = world;
    }

    public void OnServerStopped()
    {
        serverWorld = null;
    }

    private void Update()
    {
        if (serverWorld == null)
            return;

        EntityQuery queryBoss = serverWorld.EntityManager.CreateEntityQuery(ComponentModule.ReadOnly<BossCD>(),
            ComponentModule.ReadOnly<SpawnPointCD>(),
            ComponentModule.ReadOnly<IsInCombatCD>(),
            ComponentModule.ReadOnly<ObjectDataCD>(),
            ComponentModule.ReadWrite<Translation>());
        NativeArray<Entity> bossEntities = queryBoss.ToEntityArray(Allocator.Temp);
        foreach (var e in bossEntities)
        {
            var bossID = serverWorld.EntityManager.GetModComponentData<ObjectDataCD>(e).objectID;
            //Plugin.logger.LogInfo("Boss ID is: " + bossID);
            bool isInCombat = serverWorld.EntityManager.GetModComponentData<IsInCombatCD>(e).isInCombat;
            double distanceFromPlayer = 
                math.distance(serverWorld.EntityManager.GetModComponentData<SpawnPointCD>(e).position,
                    Manager._instance.player.WorldPosition);
            switch (bossID)
            {
                case ObjectID.ShamanBoss:
                    //if the player is too far and boss is in combat, that player will take damage
                    //between 9 and 12 block
                    Plugin.logger.LogInfo("Distance from player is " + distanceFromPlayer);
                    if (!isInCombat)
                        return;
                    if(distanceFromPlayer < 9 || distanceFromPlayer > 12)
                        return;
                    Manager._instance.player.playerCommandSystem.AddOrRefreshCondition(Manager._instance.player.entity, ConditionID.AuraRadioactiveDamageOverTime, 5 ,10000);
                    return;
                case ObjectID.SlimeBoss:
                    return;
                default:
                    return;
            }
        }
    }
}