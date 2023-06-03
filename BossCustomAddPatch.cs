using System;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Submodules.ModEntity;
using HarmonyLib;
using PugTilemap;
using PugWorldGen;
using Rewired;
//using UnhollowerBaseLib;
//using UnhollowerRuntimeLib;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;
using Unity.Physics;
using Unity.Transforms;
using IntPtr = Il2CppSystem.IntPtr;
using Math = System.Math;

namespace PainMod;

[HarmonyPatch]
internal class BossCustomAddPatch
{
    private static bool phase2ShamanTransitionHasHappened = false;
    private static bool phase3ShamanTransitionHasHappened = false;
    private static bool phase3firing = false;
    private static bool shamanCanDoPhase3Move = false;
    private static bool isAlreadyDead = false;
    private static int turnsOfAttack = 0;
    private static bool firstTime = true;
   // private static bool ShamanDowned = false;
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.ManagedLateUpdate))]
    [HarmonyPostfix]
    public static void ShamanPhase3(ShamanBoss __instance)
    {
     //   return;
        if(__instance==null)
            return;
       // if(__instance.GetIl2CppType()==il2cpptypeof<ShamanBoss>)
       if(!__instance.entityExist)
           return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID == ObjectID.ShamanBoss)
        {
            bool isOwner = Manager._instance._ecsManager.ServerWorld!=null;
            if (isOwner)
            {
                //Plugin.logger.LogInfo("Hello " + Players.GetCurrentPlayer().playerName + "! You are the owner of this server");
            }

            var origo = Manager._instance._cameraManager.OrigoTransform;
            PrefabInfo refInfo = new PrefabInfo();
            foreach (var prefabInfo in __instance.objectInfo.prefabInfos)
            {
                refInfo = prefabInfo;
            }
            
            //__instance.previousHealth = __instance.GetMaxHealth();
               // Plugin.logger.LogInfo("Malugaz health is now " +
               //                       ((float) ((float) __instance.currentHealth / (float) __instance.GetMaxHealth()) *
               //                        100f) * Players.GetAllPlayers().Count + "!");
               // Plugin.logger.LogInfo("Malugaz full health is " + __instance.GetMaxHealth() +
               //                       " and the current health is " + __instance.currentHealth + " and previous health is " + __instance.previousHealth + ". Due to this malugaz phase 2 started is " + phase2ShamanTransitionHasHappened + "and phase 3 started is " + phase3ShamanTransitionHasHappened);
                if (__instance.currentHealth  * Players.GetAllPlayers().Count == __instance.GetMaxHealth())
                {
                    isAlreadyDead = false;
                    ResetValues(__instance);
                    if (firstTime)
                    {
                        SetValidBossShamans();
                        firstTime = false;
                    }
                }

                if (__instance.currentHealth * Players.GetAllPlayers().Count <= __instance.GetMaxHealth() / 2)
            {
                if (!isAlreadyDead)
                {
                    if (!phase2ShamanTransitionHasHappened)
                    {
                        Plugin.logger.LogInfo("Malugaz under 50% health init phase 2!");
                        __instance.optionalHealthBar.resurrectionHealthRatioThreshold = 0.25f / Players.GetAllPlayers().Count;
                        phase2ShamanTransitionHasHappened = true;
                    }
                }
            }

            if (__instance.currentHealth * Players.GetAllPlayers().Count <= __instance.GetMaxHealth() / 4)
            {
                if (!isAlreadyDead)
                {
                    if (!phase3ShamanTransitionHasHappened)
                    {
                        Plugin.logger.LogInfo("Malugaz under 25% health init phase 3!");
                        __instance.AE_PhaseTransition();
                        __instance.AE_MagicBuildUpSFX();
                        __instance.AE_DeathBurst();
                        //__instance.optionalHealthBar.background.color = Color.yellow;
                        phase3ShamanTransitionHasHappened = true;
                        shamanCanDoPhase3Move = true;
                    }
                }

                if (shamanCanDoPhase3Move == true)
                {
                    shamanCanDoPhase3Move = false;
                    phase3firing = true;
                    if (isOwner)
                    {
                        __instance.AE_AttackEffects();
                        int rand = Mathf.CeilToInt(Random.value * 3);
                        int rand2 = Mathf.CeilToInt(Random.value * 3);
                        for (int sCount = 0; sCount < rand; sCount++)
                            Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.CavelingShaman, 2);
                        for (int bCount = 0; bCount < rand2; bCount++)
                            Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.CavelingBrute, 3);
                        shamanCanDoPhase3Move = false;
                    }
                    if (isOwner)
                    {
                        List<GameObject> validBossShaman = new List<GameObject>();
                        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
                        {
                            try
                            {
                               
                                if (childtransform.gameObject.name == "ShamanBoss(Clone)")
                                {
                                    var sb = childtransform.gameObject.GetComponent<ShamanBoss>();
                                    if (childtransform.gameObject.active)
                                        validBossShaman.Add(childtransform.gameObject);
                                    Plugin.logger.LogInfo("A shaman boss has been registered!");
                                }
                            }
                            catch
                            {
                                Plugin.logger.LogInfo("something went wrong!");
                            }
                        }
                        GameObject shamanBossInstance = new GameObject();
                        if(validBossShaman.Count>0)
                            shamanBossInstance = validBossShaman[0];
                        //var instanceThing = shamanBossInstance.TryGetComponent<ShamanBoss>(out var shamanBossEntityInstance);
                        var shamanBossEntityInstance = shamanBossInstance.GetComponent<ShamanBoss>();
                        if (shamanBossEntityInstance != null)
                        {
                            Plugin.logger.LogInfo( "XYZ data stuff: " + Manager._instance.player.world.EntityManager.GetComponentData<Translation>(shamanBossEntityInstance.entity).Value.ToString());
                            Translation translation = new Translation
                            {
                                Value = new float3(validBossShamans[0].GetComponent<ShamanBoss>().WorldPosition.x - 5,0,validBossShamans[0].GetComponent<ShamanBoss>().WorldPosition.z)
                            };
                            Manager._instance.player.netEcbSystem.SetComponent(shamanBossEntityInstance.entity, translation);
                            Manager._instance.player.world.EntityManager.SetComponentData<Translation>(
                                shamanBossEntityInstance.entity, translation);
                            Plugin.logger.LogInfo( "XYZ data stuff after change: " + Manager._instance.player.world.EntityManager.GetComponentData<Translation>(shamanBossEntityInstance.entity).Value.ToString());
                        }
                    }
                }

                /*if (__instance.currentHealth < refInfo.ecsPrefab.GetComponent<HealthCDAuthoring>().maxHealth / 2)
                {
                    if (!finalPartHasHappened)
                    {
                        int rand3 = Mathf.CeilToInt(Random.value * 3);
                        int rand4 = Mathf.CeilToInt(Random.value * 3);
                        adds1 += rand3;
                        adds0 += rand4;
                        __instance.AE_PhaseTransition();
                        __instance.AE_AnticipationSound();
                        __instance.AE_MagicBuildUpSFX();
                        shamanCanDoPhase3Move = true;
                        finalPartHasHappened = true;
                    }
                }*/
                
            }

            if (__instance.currentHealth * Players.GetAllPlayers().Count <= __instance.GetMaxHealth() * 0.25)
            {
                
            }

            if (__instance.currentHealth * Players.GetAllPlayers().Count <= __instance.GetMaxHealth() * 0.20)
            {
                if (phase3firing)
                {
                    phase3firing = false;
                    if (isOwner)
                    {
                        //ShamanBoss shamanBoss1;
                        //shamanBoss1 = null;
                        List<GameObject> validBossShaman = new List<GameObject>();
                        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
                        {
                            try
                            {
                               
                                if (childtransform.gameObject.name == "ShamanBoss(Clone)")
                                {
                                    var sb = childtransform.gameObject.GetComponent<ShamanBoss>();
                                    if (childtransform.gameObject.active)
                                        validBossShaman.Add(childtransform.gameObject);
                                    Plugin.logger.LogInfo("A shaman boss has been registered!");
                                }
                            }
                            catch
                            {
                                Plugin.logger.LogInfo("something went wrong!");
                            }
                        }
                        GameObject shamanBossInstance = new GameObject();
                        if(validBossShaman.Count>0)
                            shamanBossInstance = validBossShaman[0];
                        //var instanceThing = shamanBossInstance.TryGetComponent<ShamanBoss>(out var shamanBossEntityInstance);
                        var shamanBossEntityInstance = shamanBossInstance.GetComponent<ShamanBoss>();
                        if (shamanBossEntityInstance != null)
                        {
                            float speed = 0.5f;
                            float3 velo;
                            float rad;
                            float3 relativepos;
                            FireProjectileAtPlayer(Manager._instance.player.WorldPosition,
                                shamanBossEntityInstance.WorldPosition, shamanBossEntityInstance.entity,
                                ObjectID.FireballProjectile, 950, speed, out velo, out relativepos, false);
                            rad = Mathf.Atan2(velo.x, velo.z);
                            for (int i = 0; i < 11; i++)
                            {
                                rad += (Mathf.PI / 6);
                                float3 newVelo = new float3((Mathf.Cos(rad) * 1),0 , (Mathf.Sin(rad) * 1));
                                velo = newVelo * speed;
                                Manager._instance.player.playerCommandSystem.SpawnProjectile(ObjectID.FireballProjectile, relativepos * -1, velo * -1, shamanBossEntityInstance.entity,
                                    500, false, false);
                            }

                        }

                        phase3firing = false;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnDeath))]
    [HarmonyPostfix]
    public static void ShamanDed(ShamanBoss __instance)
    {
        //we reset values cuz things will get fucked up if we dont (trust me things WILL get fucked up if we dont)
        Plugin.logger.LogInfo("Shaman has died");
        if (__instance != null)
        {
            if (__instance.objectInfo.objectID == ObjectID.ShamanBoss)
            {
                isAlreadyDead = true;
                ResetValues(__instance);
            }
        }
    }
   // [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.))]
    //[HarmonyPostfix]
    /*public static void ShamanIdles(/ShamanBoss __instance/)
    {
        //we reset values cuz things will get fucked up if we dont (trust me things WILL get fucked up if we dont)
        //if (__instance != null)
        {
            Plugin.logger.LogInfo("init!");
           // ResetValues(__instance);
        }
    }  */

    public static void ResetValues(ShamanBoss shamanBoss)
    {
        if (shamanBoss.optionalHealthBar != null)
        {
            
            //shamanBoss.previousHealth = shamanBoss.GetMaxHealth();
            shamanBoss.optionalHealthBar.resurrectionHealthRatioThreshold = 0.5f / Players.GetAllPlayers().Count;
            phase2ShamanTransitionHasHappened = false;
            phase3ShamanTransitionHasHappened = false;
            shamanCanDoPhase3Move = false;
            phase3firing = false;
        }
    }
    
    public static void ThunderBeamAtPlayer(Vector3 player, Vector3 enemypos, Entity ent)
    {
        Unity.Mathematics.float3 playerpos = new Unity.Mathematics.float3(player.x, 0, player.z);
        Unity.Mathematics.float3 ffpos = new Unity.Mathematics.float3(enemypos.x, 0, enemypos.z);
        Unity.Mathematics.float3 relativepos = playerpos - ffpos;
        Unity.Mathematics.float3 bearing = ffpos - playerpos;
        //convert vector to unit vector
        float magnitude = (float)Math.Sqrt(bearing.x * bearing.x + bearing.z * bearing.z);
        bearing = new Unity.Mathematics.float3(bearing.x / magnitude, 0, (bearing.z / magnitude));
        bearing *= 2;//multiply unit vector by speed
        relativepos += bearing;
        Plugin.logger.LogInfo($"({relativepos.x},{relativepos.y},{relativepos.z})");
        Manager._instance.player.playerCommandSystem.SpawnThunderBeam(relativepos * -1, bearing * -1, ent, -10);
        
    }

    public static void FireProjectileAtPlayer(Vector3 player, Vector3 enemypos, Entity ent, ObjectID projectileID, int damage, float speed, out float3 bearing, out float3 relativepos, bool fakeFire)
    {
        Unity.Mathematics.float3 playerpos = new Unity.Mathematics.float3(player.x, 0, player.z);
        Unity.Mathematics.float3 ffpos = new Unity.Mathematics.float3(enemypos.x, 0, enemypos.z);
        relativepos = playerpos - ffpos;
        bearing = ffpos - playerpos;
        //ObjectID.CartographyTable;//4023
        //EnvironmentSpawnObjectStruct os;
        //os.
        //EnvironmentSpawnObject spawn;
        //spawn.
        //convert vector to unit vector
        float magnitude = (float) Math.Sqrt(bearing.x * bearing.x + bearing.z * bearing.z);
        bearing = new Unity.Mathematics.float3(bearing.x / magnitude, 0, (bearing.z / magnitude));
        bearing *= speed; //multiply unit vector by speed
        relativepos += bearing;
        Plugin.logger.LogInfo($"({relativepos.x},{relativepos.y},{relativepos.z})");
        if(!fakeFire)
            Manager._instance.player.playerCommandSystem.SpawnProjectile(projectileID, relativepos * -1, bearing * -1, ent,
            damage, false, false);
        bearing /= speed;
    }

    private static Vector3 GetToRelativePosition(Vector3 player, Vector3 targetWorldPos, float speed = 0.5f)
    {
        Unity.Mathematics.float3 playerpos = new Unity.Mathematics.float3(player.x, 0, player.z);
        Unity.Mathematics.float3 ffpos = new Unity.Mathematics.float3(targetWorldPos.x, 0, targetWorldPos.z);
        var relativepos = playerpos - ffpos;
        var bearing = ffpos - playerpos;
        //ObjectID.CartographyTable;//4023
        //EnvironmentSpawnObjectStruct os;
        //os.
        //EnvironmentSpawnObject spawn;
        //spawn.
        //convert vector to unit vector
        float magnitude = (float) Math.Sqrt(bearing.x * bearing.x + bearing.z * bearing.z);
        bearing = new Unity.Mathematics.float3(bearing.x / magnitude, 0, (bearing.z / magnitude));
        bearing *= speed; //multiply unit vector by speed
        relativepos += bearing;
        Plugin.logger.LogInfo($"({relativepos.x},{relativepos.y},{relativepos.z})");
        return relativepos * -1;
    }

    private static List<GameObject> validBossShamans = new List<GameObject>();
    private static void SetValidBossShamans()
    {
        var origo = Manager._instance._cameraManager.OrigoTransform;
        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
        {
            try
            {

                if (childtransform.gameObject.name == "ShamanBoss(Clone)")
                {
                    var sb = childtransform.gameObject.GetComponent<ShamanBoss>();
                    if (childtransform.gameObject.active)
                        validBossShamans.Add(childtransform.gameObject);
                    Plugin.logger.LogInfo("A shaman boss has been registered!");
                }
            }
            catch
            {
                Plugin.logger.LogInfo("something went wrong!");
            }
        }
    }

    [HarmonyPatch(typeof(ShamanBoss), nameof(ShamanBoss.AE_AttackEffects))]
    [HarmonyPostfix]
    public static void OnShamanAtk(ShamanBoss __instance)
    {
        Plugin.logger.LogInfo("Shaman sort of ATTACKING");
        if(__instance==null)
            return;
        if(!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID == ObjectID.ShamanBoss)
        {
            bool isOwner = Manager._instance._ecsManager.ServerWorld!=null;
            Plugin.logger.LogInfo("Shaman ATTACKING");
            if (__instance.currentHealth <= __instance.GetMaxHealth() * 0.25)
            {
                turnsOfAttack++;
                if (turnsOfAttack % 5 == 0)
                {
                    if (isOwner)
                    {
                        //ShamanBoss shamanBoss1;
                        //shamanBoss1 = null;
                        

                        GameObject shamanBossInstance = new GameObject();
                        if (validBossShamans.Count > 0)
                            shamanBossInstance = validBossShamans[0];
                        //var instanceThing = shamanBossInstance.TryGetComponent<ShamanBoss>(out var shamanBossEntityInstance);
                        var shamanBossEntityInstance = shamanBossInstance.GetComponent<ShamanBoss>();
                        if (shamanBossEntityInstance != null)
                        {
                            float speed = 0.5f;
                            float3 velo;
                            float rad;
                            float3 relativepos;
                            FireProjectileAtPlayer(Manager._instance.player.WorldPosition,
                                shamanBossEntityInstance.WorldPosition, shamanBossEntityInstance.entity,
                                ObjectID.FireballProjectile, 950, speed, out velo, out relativepos, false);
                            rad = Mathf.Atan2(velo.x, velo.z);
                            for (int i = 0; i < 11; i++)
                            {
                                rad += (Mathf.PI / 6);
                                float3 newVelo = new float3((Mathf.Cos(rad) * 1), 0, (Mathf.Sin(rad) * 1));
                                velo = newVelo * speed;
                                Manager._instance.player.playerCommandSystem.SpawnProjectile(
                                    ObjectID.FireballProjectile, relativepos * -1, velo * -1,
                                    shamanBossEntityInstance.entity,
                                    500, false, false);
                            }

                        }

                        phase3firing = false;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.UpdateHealthBar))]
    [HarmonyPostfix]
    public static void BirdBossThing(BirdBoss __instance)
    {
    }

    public static bool birdPhase2Active = false;
    public static bool birdPhase3Active = false;
    public static bool allowCreateBoundaries = true;
    public static List<Vector3> ringOfFire = new List<Vector3>();
    public static bool firstTime2 = true;
    public static float BirdCurrentHP = 1f;
    public static bool isOwner2;
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.ManagedLateUpdate))]
    [HarmonyPostfix]
    public static void BirdLogger(BirdBoss __instance)
    {
        if(__instance==null)
            return;
        if (!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.BirdBoss)
        {
         //   birdHP = __instance.currentHealth;
         return;
        }

        if (!__instance.SrPivot.active)
        {
            Plugin.logger.LogInfo("Almost went through");
            return;
        }
        isOwner2 = Manager._instance._ecsManager.ServerWorld!=null;
        BirdCurrentHP = (float)__instance.currentHealth / __instance.GetMaxHealth();
        //  Plugin.logger.LogInfo("bird with health" + __instance.currentHealth);
        if (__instance.currentHealth == __instance.GetMaxHealth())
        {
            if (allowCreateBoundaries && isOwner2)
            {
                {
                    float radius = 4;
                    
                    int amt = 120;
                    for (int i = 0; i < amt; i++)
                    {
                        var radians = 2 * MathF.PI / amt * i;
                        var z = radius * MathF.Sin(radians);
                        var x = radius * MathF.Cos(radians);
                        var pos =
                            birdPosReletive - Manager._instance.player.WorldPosition +
                            new Vector3(x, 0, z) * radius;
                        Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.FireTrap, pos);
                        ringOfFire.Add(birdPosReletive + new Vector3(x, 0, z) * radius);
                        Plugin.logger.LogInfo("successfully spawned one fire in the RING OF FIRE at " + birdPosReletive.ToString());
                    }
                }
            }
            allowCreateBoundaries = false;
            canStartTimer = true;
        }
        else if (__instance.currentHealth <= __instance.GetMaxHealth() * 0.65f)
        {
            birdPhase2Active = true;
        }
        else if (__instance.currentHealth <= __instance.GetMaxHealth() * 0.1f)
        {
            birdPhase2Active = false;
            birdPhase3Active = true;
        }
    }
    private static float lastCount = 0;
    private static bool canStartTimer = false;
    private static float count = 0;
    private static bool isSameAsLastCount = false;
    private static bool secondStarted = false;
    [HarmonyPatch(typeof(Manager), nameof(Manager.Update))]
    [HarmonyPostfix]
    public static void BirdTimed(Manager __instance)
    {
        if (!__instance.currentSceneHandler.isInGame)
            return;
      //  Plugin.logger.LogInfo("Can Start Timer is: " + canStartTimer);
        if (!canStartTimer || !isOwner2)
            return;
        count += Time.deltaTime;
        if ((int) count != (int) lastCount)
            isSameAsLastCount = false;
        else
            isSameAsLastCount = true;
        if (!isSameAsLastCount)
        {
            if (BirdCurrentHP <= 0.95f)
            {
                if ((int) count % 6 == 0)
                {
                    if (!secondStarted)
                    {
                        
                        var rand = Random.value;
                        Plugin.logger.LogInfo("Rand is: " + rand);
                        if (rand >= 0.75)
                        {
                            for (int i = 0; i < ringOfFire.Count; i += 6)
                            {
                                var pos = ringOfFire[i];
                                ThunderBeamAtPlayer(birdPosReletive,
                                    pos + (birdPosReletive - Manager._instance.player.WorldPosition),
                                    validBossBirds[0].GetComponent<BirdBoss>().entity);
                                // Manager._instance.player.playerCommandSystem.SpawnThunderBeam(pos, 1, validBossBirds[0].GetComponent<BirdBoss>().entity, -10);
                                Plugin.logger.LogInfo("Bird BOOM");
                            }
                        }
                        else if (rand >= 0.15)
                        {
                            var rand2x = Random.value;
                            var rand2y = Random.value;
                            Vector3 calcRand2 = new Vector3((rand2x * 10) - 5, 0, (rand2y * 10) - 5); 
                            Vector3 randPos = birdPosReletive + calcRand2;
                            float speed = 1.5f;
                            float3 velo;
                            float rad;
                            float3 relativepos;
                            FireProjectileAtPlayer(Manager._instance.player.WorldPosition,
                                randPos, validBossBirds[0].GetComponent<BirdBoss>().entity,
                                ObjectID.FireballProjectile, 950, speed, out velo, out relativepos, true);
                            rad = Mathf.Atan2(velo.x, velo.z);
                            rad += (Mathf.PI / (int) (Random.value * 12));
                            for (int i = 0; i < 11; i++)
                            {
                                rad += (Mathf.PI / 6);
                                float3 newVelo = new float3((Mathf.Cos(rad) * 1), 0, (Mathf.Sin(rad) * 1));
                                velo = newVelo * speed;
                                Manager._instance.player.playerCommandSystem.SpawnThunderBeam(relativepos * -1, velo * -1, validBossBirds[0].GetComponent<BirdBoss>().entity, 800);
                              /*  Manager._instance.player.playerCommandSystem.SpawnProjectile(
                                    ObjectID.FireballProjectile, relativepos * -1, velo * -1,
                                    validBossBirds[0].GetComponent<BirdBoss>().entity,
                                    500, false, false);*/
                            }
                        }
                        else
                        {
                            int rand2 = ((int) (Random.value * 3)) + 3;
                            for (int bCount = 0; bCount < rand2; bCount++)
                                Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.CavelingHunter, 3);
                        }

                        secondStarted = true;
                    }
                }
                else
                {
                    secondStarted = false;
                }
            }
        }
    }

    public static void ResetValuesBird()
    {
        birdPhase2Active = false;
        birdPhase3Active = false;
        allowCreateBoundaries = true;
        canStartTimer = false;
        ringOfFire.Clear();
        count = 0;
    }

    public static Vector3 birdPosReletive = new Vector3();
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnOccupied))]
    [HarmonyPostfix]
    public static void OnOccupiedBird(BirdBoss __instance)
    {
        if(__instance==null)
            return;
        if (!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.BirdBoss)
            return;
        SetValidBossBirds();
        ResetValuesBird();
        birdPosReletive = new Vector3(validBossBirds[0].GetComponent<BirdBoss>().center.x, 0,
            validBossBirds[0].GetComponent<BirdBoss>().center.z);
        /* 
         float radius = 4;
 
         int amt = 72;
         for (int i = 0; i < amt; i++)
         {
             var radians = 2 * MathF.PI / amt * i;
             var z = radius * MathF.Sin(radians);
             var x = radius * MathF.Cos(radians);
             var pos =
                 (__instance.WorldPosition -
                  Manager._instance.player.WorldPosition) +
                 new Vector3(x, 0, z) * radius;
             Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.FireTrap, pos);
             Plugin.logger.LogInfo("successfully spawned one fire in the RING OF FIRE");
         } */
    }

    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnDeath))]
    [HarmonyPostfix]
    public static void eggByeBye(EntityMonoBehaviour __instance)
    {
        if(__instance==null)
            return;
        if (!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.LargeShinyGlimmeringObject)
            return;
        ResetValuesBird();
        //when egg die
    }
    private static List<GameObject> validBossBirds = new List<GameObject>();
    private static void SetValidBossBirds()
    {
        var origo = Manager._instance._cameraManager.OrigoTransform;
        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
        {
            try
            {

                if (childtransform.gameObject.name == "BirdBoss(Clone)")
                {
                    var sb = childtransform.gameObject.GetComponent<BirdBoss>();
                    if (childtransform.gameObject.active)
                        validBossBirds.Add(childtransform.gameObject);
                    Plugin.logger.LogInfo("A bird boss has been registered!");
                }
            }
            catch
            {
                Plugin.logger.LogInfo("something went wrong!");
            }
        }
    }
    public static List<GameObject> validBossSlimes = new List<GameObject>();
    private static void SetValidBossSlimes()
    {
        var origo = Manager._instance._cameraManager.OrigoTransform;
        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
        {
            try
            {

                if (childtransform.gameObject.name == "SlimeBoss(Clone)")
                {
                    var sb = childtransform.gameObject.GetComponent<SlimeBoss>();
                    if (childtransform.gameObject.active)
                        validBossSlimes.Add(childtransform.gameObject);
                    Plugin.logger.LogInfo("A slime boss has been registered!");
                }
            }
            catch
            {
                Plugin.logger.LogInfo("something went wrong!");
            }
        }
    }

    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnOccupied))]
    [HarmonyPostfix]
    public static void OnOccupiedSlime(SlimeBoss __instance)
    {
        if (__instance == null)
            return;
        if (!__instance.entityExist)
            return;
        if (__instance.objectInfo == null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.SlimeBoss)
        {
            //Plugin.logger.LogInfo("Object is not a slimeboss, instead it is a " + __instance.objectInfo.objectID);
            return;
        }
        SetValidBossSlimes();
    }

    public static bool isSlam;
    [HarmonyPatch(typeof(SlimeBoss), nameof(SlimeBoss.AE_LandingEffect))]
    [HarmonyPostfix]
    public static void OnGlurchGroundSlam(SlimeBoss __instance)
    {
        isSlam = true;
    }

    /*[HarmonyPatch(typeof(SlimeBoss), nameof(SlimeBoss.AE_LandingEffect))]
    [HarmonyPostfix]
    public static void OnGlurchGroundSlam(SlimeBoss __instance)
    {
        Plugin.logger.LogInfo("Slime sort of ATTACKING");
        if (__instance == null)
            return;
        if (!__instance.entityExist)
            return;
        if (__instance.objectInfo == null)
            return;
        Plugin.logger.LogInfo("Slime definately attacking");
        if (__instance.objectInfo.objectID == ObjectID.SlimeBoss)
        {
            bool isOwner = Manager._instance._ecsManager.ServerWorld != null;
            if (isOwner)
            {
                Plugin.logger.LogInfo("Player is owner");
                float rad;
                float speed = 0.75f;
                var slimeBossInstance = validBossSlimes[0].GetComponent<SlimeBoss>();
                if (slimeBossInstance != null)
                {
                    if (slimeBossInstance.entityExist)
                    {
                        Plugin.logger.LogInfo("Slimeboss[0] is " + slimeBossInstance + " and its pos is " +
                                              slimeBossInstance.WorldPosition);
                        FireProjectileAtPlayer(Manager._instance.player.WorldPosition, slimeBossInstance.WorldPosition
                            , slimeBossInstance.entity, ObjectID.LegendaryEnergyProjectile, 200, speed, out float3 velo,
                            out float3 relativePos, false);
                        rad = Mathf.Atan2(velo.x, velo.z);
                        rad += (Mathf.PI / (int) (Random.value * 12));
                        for (int i = 0; i < 6; i++)
                        {
                            rad += (Mathf.PI / 3);
                            float3 newVelo = new float3((Mathf.Cos(rad) * 1), 0, (Mathf.Sin(rad) * 1));
                            velo = newVelo * speed;
                            Manager._instance.player.playerCommandSystem.SpawnProjectile(
                                ObjectID.LegendaryEnergyProjectile,
                                relativePos * -1, velo * -1, slimeBossInstance.entity, 200, false, false);
                            /*  Manager._instance.player.playerCommandSystem.SpawnProjectile(
                                  ObjectID.FireballProjectile, relativepos * -1, velo * -1,
                                  validBossBirds[0].GetComponent<BirdBoss>().entity,
                                  500, false, false);*
                        }
                    }
                }
            }
        }
    }*/
    //GHORM REWORK
    public static void ResetValuesLarva()
    {
        if(validBossLarva[0]!=null)
            Manager._instance.player.playerCommandSystem.SetHealth(validBossLarva[0].GetComponent<BossLarva>().entity, 
                validBossLarva[0].GetComponent<BossLarva>().GetMaxHealth(), validBossLarva[0].GetComponent<BossLarva>().WorldPosition);
    }
    private static List<GameObject> validBossLarva = new List<GameObject>();
    private static void SetValidBossLarva()
    {
        var origo = Manager._instance._cameraManager.OrigoTransform;
        foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
        {
            try
            {

                if (childtransform.gameObject.name == "BossLarva(Clone)")
                {
                    var sb = childtransform.gameObject.GetComponent<BossLarva>();
                    if (childtransform.gameObject.active)
                        validBossLarva.Add(childtransform.gameObject);
                    Plugin.logger.LogInfo("A larva boss has been registered!");
                }
            }
            catch
            {
                Plugin.logger.LogInfo("something went wrong!");
            }
        }
    }
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnOccupied))]
    [HarmonyPostfix]
    public static void OnOccupiedLarva(BossLarva __instance)
    {
        if(__instance==null)
            return;
        if (!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.BossLarva)
            return;
        SetValidBossLarva();
        ResetValuesLarva();
        /* 
         float radius = 4;
 
         int amt = 72;
         for (int i = 0; i < amt; i++)
         {
             var radians = 2 * MathF.PI / amt * i;
             var z = radius * MathF.Sin(radians);
             var x = radius * MathF.Cos(radians);
             var pos =
                 (__instance.WorldPosition -
                  Manager._instance.player.WorldPosition) +
                 new Vector3(x, 0, z) * radius;
             Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.FireTrap, pos);
             Plugin.logger.LogInfo("successfully spawned one fire in the RING OF FIRE");
         } */
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Kill))]
    [HarmonyPostfix]
    public static void PlayerDie(PlayerController __instance)
    {
        Plugin.logger.LogInfo(__instance.playerName + " is now dieing");
        ResetValuesLarva();
    }
    //ra akar edit
    public static bool scarabPhase2Active;
    public static bool firstInitP2 = false;
    public static bool scarabPhase3Active;
    public static bool firstInitP3 = false;
    [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.ManagedLateUpdate))]
    [HarmonyPostfix]
    public static void ScarabLogger(ScarabBoss __instance)
    {
        if(__instance==null)
            return;
        if (!__instance.entityExist)
            return;
        if(__instance.objectInfo==null)
            return;
        if (__instance.objectInfo.objectID != ObjectID.ScarabBoss)
        {
         //   birdHP = __instance.currentHealth;
         return;
        }
        isOwner2 = Manager._instance._ecsManager.ServerWorld!=null;
        //ScarabCurrentHP = (float)__instance.currentHealth / __instance.GetMaxHealth();
        //  Plugin.logger.LogInfo("bird with health" + __instance.currentHealth);
        if (__instance.currentHealth <= __instance.GetMaxHealth() * 0.50f && !firstInitP2)
        {
            scarabPhase2Active = true;
            firstInitP2 = true;
        }
        if (__instance.currentHealth <= __instance.GetMaxHealth() * 0.25f && !firstInitP3)
        {
            Plugin.logger.LogInfo("Phase 3 yessir");
            scarabPhase3Active = true;
            firstInitP3 = true;
        }
    }


    /*[HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.ManagedLateUpdate))]
    [HarmonyPostfix]
    public static void HealthBarChange(EntityMonoBehaviour __instance)
    {
        if (__instance.objectInfo.objectID == ObjectID.ShamanBoss)
        {
            Plugin.logger.LogInfo("health bar updated!");
            Plugin.logger.LogInfo("health is now " +  ((float)((float)__instance.previousHealth/ (float)__instance.GetMaxHealth()) * 100f));
        }
    } */
}