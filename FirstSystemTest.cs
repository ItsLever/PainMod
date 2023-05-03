using System;
using System.Collections.Generic;
using CoreLib.Submodules.ModComponent;
using CoreLib.Util;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Type = Il2CppSystem.Type;

namespace PainMod;

public class FirstSystemTest : MonoBehaviour
{
    public static FirstSystemTest instance;
    internal World serverWorld;
    private bool isInit = false;
    private bool isOtherInit = false;
    private float timer = 0;
    private int amountToBirdHeal = 55;
    private int scarabLastTimeHP = 10000;
    private float waitTime;
    private int aggrSlimeHp = 500;
    private int aggrSlimeJDmg = 116;
    private int aggrSlimeSpeed = 165;
    private int shamanDmg = 186;
    private int shamanHP = 1428;
    //private bool birdInPhase2 = false;
    public FirstSystemTest(IntPtr ptr) : base(ptr) { }
    private void Awake()
    {
        instance = this;
        waitTime = 5;
    }

    /*public NativeArray<Entity> GetDataFromQuery(ComponentType filter)
    {
        EntityQuery query = serverWorld.EntityManager.CreateEntityQuery(
            filter);
        NativeArray<Entity> result3 = query.ToEntityArray(Allocator.Temp);
        return result3;
    }

    public List<T> ToComponentList<T>(NativeArray<Entity> array,T component)
    {
        List<T> list = new List<T>();
        foreach (var e in array)
        {
            T ComponentData = serverWorld.EntityManager.GetModComponentData<T>(e);
            list.Add(ComponentData);
        }
        return list;
    }*/

    private void Update()
    {
        if (serverWorld == null) return;
        if (EnemyUpgrade.wallIsLowered)
        {
            if (SystemsStarterPatch.isInit)
            {
                waitTime -= Time.deltaTime;
                //Plugin.logger.LogInfo("WaitTime " + waitTime);
                if (waitTime <= 0)
                {
                    Plugin.logger.LogInfo("Great wall has been lowered");
                    //do enemy changes
                    EntityQuery query = serverWorld.EntityManager.CreateEntityQuery(
                        ComponentType.ReadOnly<HealthCD>());
                    NativeArray<Entity> result3 = query.ToEntityArray(Allocator.Temp);
                    foreach (var e2 in result3)
                    {
                        if (serverWorld.EntityManager.GetModComponentData<ObjectDataCD>(e2).objectID ==
                            ObjectID.AggressiveSlimeBlob)
                        {
                            if (serverWorld.EntityManager.GetModComponentData<HealthCD>(e2).maxHealth != aggrSlimeHp)
                            {
                                HealthCD healthCD = serverWorld.EntityManager.GetModComponentData<HealthCD>(e2);
                                healthCD.maxHealth = aggrSlimeHp;
                                healthCD.health = aggrSlimeHp;
                                serverWorld.EntityManager.SetComponentData(e2, healthCD);
                                Plugin.logger.LogInfo("This is a new aggressive slime blob and its hp is now " + serverWorld.EntityManager.GetModComponentData<HealthCD>(e2).maxHealth);
                            }

                            if (serverWorld.EntityManager.GetModComponentData<JumpAttackStateCD>(e2).jumpDamage !=
                                aggrSlimeJDmg)
                            {
                                JumpAttackStateCD attackStateCd = serverWorld.EntityManager.GetModComponentData<JumpAttackStateCD>(e2);
                                attackStateCd.jumpDamage = aggrSlimeJDmg;
                                attackStateCd.jumpMoveSpeed = aggrSlimeSpeed;
                                serverWorld.EntityManager.SetComponentData(e2, attackStateCd);
                            }

                            if (serverWorld.EntityManager.GetModComponentData<ChaseStateCD>(e2).preferPathFind != true)
                            {
                                ChaseStateCD chase = serverWorld.EntityManager.GetModComponentData<ChaseStateCD>(e2);
                                chase.preferPathFind = true;
                                chase.targetEntity = Manager._instance.player.entity;
                                chase.chaseAtDistanceSq = 100;
                                serverWorld.EntityManager.SetComponentData(e2, chase);
                            }
                        }

                        if (serverWorld.EntityManager.GetModComponentData<ObjectDataCD>(e2).objectID ==
                            ObjectID.CavelingShaman)
                        {
                            if (serverWorld.EntityManager.GetModComponentData<HealthCD>(e2).maxHealth != shamanHP)
                            {
                                HealthCD healthCD = serverWorld.EntityManager.GetModComponentData<HealthCD>(e2);
                                healthCD.maxHealth = shamanHP;
                                healthCD.health = shamanHP;
                                serverWorld.EntityManager.SetComponentData(e2, healthCD);
                            }
                            if (serverWorld.EntityManager.GetModComponentData<RangeAttackStateCD>(e2)
                                .rangeDamage != shamanDmg)
                            {
                                RangeAttackStateCD rAtkState = serverWorld.EntityManager.GetModComponentData<RangeAttackStateCD>(e2);
                                rAtkState.minCooldown = 0;
                                rAtkState.maxCooldown = 2;
                                rAtkState.rangeDamage = shamanDmg;
                                serverWorld.EntityManager.SetComponentData(e2, rAtkState);
                            }
                        }
                    }
                    waitTime = 1f;
                    //SystemsStarterPatch.isInit = false;
                }
            }
        }
        
        #region GlurchCrash

        

                /*if (BossCustomAddPatch.isSlam)
        {
            float rad;
            float speed = 0.75f;
            EntityQuery query = serverWorld.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<HealthCD>());
            NativeArray<Entity> result3 = query.ToEntityArray(Allocator.Temp);
            BossCustomAddPatch.isSlam = false;
            foreach (var e in result3)
            {
                if (serverWorld.EntityManager.GetComponentData<ObjectDataCD>(e).objectID == ObjectID.SlimeBoss)
                {
                    var v = EntityUtility.GetObjectInfo(e, serverWorld);
                    //Plugin.logger.LogInfo("Slime name is " + v.prefabInfos._items[0].prefab.name);

                    Translation t = serverWorld.EntityManager.GetModComponentData<Translation>(e);
                    BossCustomAddPatch.FireProjectileAtPlayer(Manager._instance.player.WorldPosition, t.Value
                        , e, ObjectID.LegendaryEnergyProjectile, 200, speed, out float3 velo,
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
                            relativePos * -1, velo * -1, (v.prefabInfos._items[0].prefab.GetComponent<SlimeBoss>()).entity/*e* , 200, false, false);
                        /*  Manager._instance.player.playerCommandSystem.SpawnProjectile(
                              ObjectID.FireballProjectile, relativepos * -1, velo * -1,
                              validBossBirds[0].GetComponent<BirdBoss>().entity,
                              500, false, false);*//*
                    }
                }
            }
        }*/
        #endregion


if (BossCustomAddPatch.birdPhase2Active)
{
    if (isInit) return;
    Plugin.logger.LogInfo("Second phase bird!");
    //birdInPhase2 = true;
    EntityQuery q = serverWorld.EntityManager.CreateEntityQuery(
        ComponentType.ReadOnly<BossCD>(),
        ComponentType.ReadOnly<HealthCD>());
    NativeArray<Entity> result = q.ToEntityArray(Allocator.Temp);
    foreach (var e in result)
    {
        if (serverWorld.EntityManager.GetComponentData<ObjectDataCD>(e).objectID == ObjectID.BirdBoss)
        {
            HealthCD healthCd = serverWorld.EntityManager.GetComponentData<HealthCD>(e);
            healthCd.health = (int) (healthCd.maxHealth * 0.9f);
            serverWorld.EntityManager.SetComponentData(e, healthCd);
            //working translate code
            /*Translation translation = serverWorld.EntityManager.GetComponentData<Translation>(e);
            translation.Value = new float3(translation.Value.x + 10, translation.Value.y, translation.Value.z);
            serverWorld.EntityManager.SetComponentData(e, translation);
            Plugin.logger.LogInfo("The translation data was " + translation.Value.ToString());*/
                    //testing stuff
                    /*StateInfoCD stateInfoCd = serverWorld.EntityManager.GetComponentData<StateInfoCD>(e);
                    Plugin.logger.LogInfo("State info cd is " + stateInfoCd.currentState + " and next state is " + stateInfoCd.newState);
                    stateInfoCd.locked = false;
                    stateInfoCd.newState = StateID.BirdBossSpawnBeams;
                    serverWorld.EntityManager.SetComponentData(e, stateInfoCd);*/
                }
            }

            isInit = true;
        }


        if (BossCustomAddPatch.scarabPhase2Active)
        {
            //if (isOtherInit) return;
            EntityQuery q = serverWorld.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<BossCD>(),
                ComponentType.ReadOnly<HealthCD>());
            NativeArray<Entity> result2 = q.ToEntityArray(Allocator.Temp);
            foreach (var e in result2)
            {
                if (serverWorld.EntityManager.GetComponentData<ObjectDataCD>(e).objectID == ObjectID.ScarabBoss)
                {
                    HealthCD healthCd = serverWorld.EntityManager.GetComponentData<HealthCD>(e);
                    if (healthCd.health != scarabLastTimeHP)
                        Plugin.logger.LogInfo("Health of ra akar is now " + healthCd.health + "and last health is " +
                                              scarabLastTimeHP);
                    scarabLastTimeHP = healthCd.health;

                    ScarabBossChargeStateCD scarabBossCd =
                        serverWorld.EntityManager.GetModComponentData<ScarabBossChargeStateCD>(e);
                    scarabBossCd.minCooldown = 2.5f;
                    scarabBossCd.maxCooldown = 4;
                    scarabBossCd.damage = 420;
                    StateInfoCD stateInfoCd = serverWorld.EntityManager.GetModComponentData<StateInfoCD>(e);
                    stateInfoCd.locked = false;
                    stateInfoCd.currentState = StateID.ScarabBossCharge;
                    stateInfoCd.newState = StateID.ScarabBossCharge;
                    Plugin.logger.LogInfo("Setting scarab component data back!");
                    serverWorld.EntityManager.SetModComponentData(e, scarabBossCd);
                    serverWorld.EntityManager.SetModComponentData(e, stateInfoCd);
                }
            }
            
            Plugin.logger.LogInfo("Set successfully so now disabling until next phase!");
            BossCustomAddPatch.scarabPhase2Active = false;
            //isOtherInit = true;
        }
        if (BossCustomAddPatch.scarabPhase3Active)
        {
            //if (isOtherInit) return;
            EntityQuery q = serverWorld.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<BossCD>(),
                ComponentType.ReadOnly<HealthCD>());
            NativeArray<Entity> result2 = q.ToEntityArray(Allocator.Temp);
            foreach (var e in result2)
            {
                if (serverWorld.EntityManager.GetComponentData<ObjectDataCD>(e).objectID == ObjectID.ScarabBoss)
                {
                    HealthCD healthCd = serverWorld.EntityManager.GetComponentData<HealthCD>(e);
                    if (healthCd.health != scarabLastTimeHP)
                        Plugin.logger.LogInfo("Health of ra akar is now " + healthCd.health + "and last health is " +
                                              scarabLastTimeHP);
                    scarabLastTimeHP = healthCd.health;

                    ScarabBossChargeStateCD scarabBossCd =
                        serverWorld.EntityManager.GetModComponentData<ScarabBossChargeStateCD>(e);
                    scarabBossCd.minCooldown = 0.25f;
                    scarabBossCd.maxCooldown = 0.5f;
                    scarabBossCd.damage = 650;
                    StateInfoCD stateInfoCd = serverWorld.EntityManager.GetModComponentData<StateInfoCD>(e);
                    stateInfoCd.locked = false;
                    stateInfoCd.currentState = StateID.ScarabBossCharge;
                    stateInfoCd.newState = StateID.ScarabBossCharge;
                    Plugin.logger.LogInfo("Setting scarab component data back!");
                    serverWorld.EntityManager.SetModComponentData(e, scarabBossCd);
                    serverWorld.EntityManager.SetModComponentData(e, stateInfoCd);
                }
            }
            
            Plugin.logger.LogInfo("Set successfully so now disabling until 5% hp!");
            BossCustomAddPatch.scarabPhase3Active = false;
            //isOtherInit = true;
        }

        /*if (birdInPhase2)
        {
            if (!isOtherInit)
            {
                timer += Time.deltaTime;
                //Plugin.logger.LogInfo("Timer is " + timer);
                foreach (var e in realResult)
                {
                    HealthCD healthCd = serverWorld.EntityManager.GetComponentData<HealthCD>(e);
                    healthCd.health = 35 + (int) (healthCd.maxHealth * ((amountToBirdHeal / 3f) * timer));
                    //Plugin.logger.LogInfo("Bird hp is now " + healthCd.health + " and it should be " + (35 + (int) (healthCd.maxHealth * ((amountToBirdHeal / 3f) * timer))/100 * healthCd.maxHealth));
                    serverWorld.EntityManager.SetComponentData(e, healthCd);
                }
            }
        } */
    }

    public void SetWorld(World world) {serverWorld = world;}

    public void RemoveWorld() { serverWorld = null; }
}