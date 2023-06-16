using System;
using System.Collections.Generic;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using CoreLib.Util.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PainMod;

public class CustomSecondaryUseSystem : MonoBehaviour, IPseudoServerSystem
{
    private World serverWorld;
    public void OnServerStarted(World world)
    {
        serverWorld = world;
    }

    public void OnServerStopped()
    {
        serverWorld = null;
    }

    public static Entity[] tornados = new Entity[4];
    private static float timer;
    private void FixedUpdate()
    {
        if (SecondaryUseCustomPatch.justStarted)
        {
        }
        else if(SecondaryUseCustomPatch.tierIsDifferentToLastFrame)
        {
            if(SecondaryUseCustomPatch.currentTier!=4)
                SpawnTornado(SecondaryUseCustomPatch.currentTier);
        }
        if(SecondaryUseCustomPatch.isStarting)
            MoveTornados();
    }

    private void MoveTornados()
    {
        timer += Time.deltaTime;
        for (int i = 0; i < 4; i++)
        {
            if(!serverWorld.EntityManager.Exists(tornados[i]))
                continue;
            Translation translation = serverWorld.EntityManager.GetModComponentData<Translation>(tornados[i]);
            double factor = Math.PI / 2 * i;
            translation.Value = new float3(Manager._instance.player.WorldPosition.x + math.cos(timer + (float) factor), 0, Manager._instance.player.WorldPosition.z + math.sin(timer + (float) factor));
            serverWorld.EntityManager.SetModComponentData(tornados[i], translation);
            Plugin.logger.LogInfo("Player is at " + Manager._instance.player.WorldPosition + " and tornado " + i + " is at " + translation.Value);
        }
    }

    private void SpawnTornado(int index)
    {
        Plugin.logger.LogInfo("Spawning tornado " + index);
        EntityQuery query2 =
            Manager.ecs.ServerWorld.EntityManager.CreateEntityQuery(ComponentType
                .ReadOnly<PugDatabase.DatabaseBankCD>());
        BlobAssetReference<PugDatabase.PugDatabaseBank> bank = query2.GetSingleton<PugDatabase.DatabaseBankCD>()
            .databaseBankBlob;
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
            
        tornados[index] = EntityUtility.CreateEntity(entityCommandBuffer,
            Manager._instance.player.WorldPosition.ToFloat3() + new float3(100, 0, 0)
            , ObjectID.OctopusBossPlayerProjectile, 1, bank, 0);
            
        ComponentType componentType = ComponentType.FromTypeIndex(ComponentModule.GetModTypeIndex<DestroyTimerCD>());
        serverWorld.EntityManager.RemoveComponent(tornados[index], componentType);

        entityCommandBuffer.Playback(Manager.ecs.ServerWorld.EntityManager);
        entityCommandBuffer.Dispose();
    }
}