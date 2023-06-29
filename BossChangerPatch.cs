using System.Collections.Generic;
using CoreLib;
using CoreLib.Components;
using CoreLib.Submodules.DropTables;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using PugTilemap;
//using UnhollowerBaseLib;
using UnityEngine;
using LootList = Il2CppSystem.Collections.Generic.List<LootInfo>;
//using UnityEngine.Playables;

namespace PainMod
{
    [HarmonyPatch]
    internal class BossChangerPatch
    {
        private static bool isInitOnce = false;
        [ HarmonyPatch(typeof(PugDatabaseAuthoring), nameof(PugDatabaseAuthoring.DeclareReferencedPrefabs))]
        [HarmonyPostfix]
        public static void PreMemInit(List<GameObject> referencedPrefabs, PugDatabaseAuthoring __instance)
        {
            bool instanceNull = __instance == null;
            Plugin.logger.LogInfo("Memory manager init rn: " + instanceNull);
            var entities = __instance.prefabList;
            Plugin.logger.LogInfo("prefab list: " + entities.Count);
            foreach (var boss in entities)
            {
                if (boss.GetComponent<BossCDAuthoring>() != null)
                {
                    if (boss.gameObject.GetComponent<EntityMonoBehaviourData>().objectInfo.objectID ==
                        ObjectID.LarvaHiveBoss)
                    {
                        Plugin.logger.LogInfo("Hive mother spotted! with original health: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().maxHealth +
                                              " and startHealth: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().startHealth);
                        
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 69420;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 69420;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().mortarDamage = 1500;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().goUpTime = 0.3f;
                        boss.GetComponent<EnrageStateCDAuthoring>().enrageAtHealthRatio = 0.9f;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().minAmountOfProjectiles = 15;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().maxAmountOfProjectiles = 30;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().minCooldown = 0.25f;
                        boss.GetComponent<ShootMortarProjectileStateCDAuthoring>().maxCooldown = 0.5f;
                        boss.GetComponent<NearbyEntitiesTrackerCDAuthoring>().radius = 69;
                        if (boss.GetComponent<DropsLootBufferAuthoring>() == null)
                            boss.gameObject.AddComponent<DropsLootBufferAuthoring>();
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().chance = 1;
                        /* LootDrop lD = new LootDrop();
                         lD.lootDropID = Plugin.masterArmorHelm;
                         lD.amount = 1;
                         lD.multiplayerAmountAdditionScaling = 0f;
                         boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values =
                             new Il2CppSystem.Collections.Generic.List<LootDrop>();
                         boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values.Add(lD);*/
                        LootDrop lD2 = new LootDrop();
                         lD2.lootDropID = Plugin.maxHealthBuffStation;
                         lD2.amount = 1;
                         lD2.multiplayerAmountAdditionScaling = 0f;
                         boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values =
                             new Il2CppSystem.Collections.Generic.List<LootDrop>();
                         boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values.Add(lD2);
                    }
                    else if (boss.gameObject.GetComponent<EntityMonoBehaviourData>().objectInfo.objectID ==
                             ObjectID.SlimeBoss)
                    {
                        Plugin.logger.LogInfo("Glurch spotted! with original health: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().maxHealth +
                                              " and startHealth: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().startHealth);
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 24069;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 24069;
                        boss.GetComponent<SlimeBossJumpStateCDAuthoring>().jumpMoveSpeed = 225;
                        boss.GetComponent<SlimeBossJumpStateCDAuthoring>().enragedJumpMoveSpeed = 355;

                        #region mortarStuff

                        boss.gameObject.AddComponent<ShootMortarProjectileStateCDAuthoring>();
                        var mortar = boss.GetComponent<ShootMortarProjectileStateCDAuthoring>();
                        mortar.mortarProjectileID = ObjectID.LarvaHiveBossMortarProjectile;
                        mortar.anticipationDuration = 0.4f;
                        mortar.skipVisibilityCheck = true;
                        mortar.onlyShootWhenInCombat = true;
                        mortar.attackDuration = 0.2f;
                        mortar.minCooldown = 2;
                        mortar.maxCooldown = 3;
                        mortar.goUpTime = 0f;
                        mortar.airTime = 0.2f;
                        mortar.goDownTime = 0.85f;
                        mortar.explodeTime = 1;
                        mortar.minAmountOfProjectiles = 10;
                        mortar.maxAmountOfProjectiles = 10;
                        mortar.maxHealthRatioToShoot = 1;
                        mortar.mortarDamage = 30;
                        mortar.damageMultiplier = 2;

                        #endregion
                    }
                    else if (boss.gameObject.GetComponent<EntityMonoBehaviourData>().objectInfo.objectID ==
                             ObjectID.BossLarva)
                    {
                        Plugin.logger.LogInfo("Ghorm spotted (ghorm gang)! with original health: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().maxHealth +
                                              " and startHealth: " +
                                              boss.gameObject.GetComponent<HealthCDAuthoring>().startHealth);
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 34000 /*ghorm gang!*/;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 34000;
                        boss.GetComponent<HealthCDAuthoring>().maxHealthMultiplier = 1;
                        boss.GetComponent<EnrageStateCDAuthoring>().enrageAtHealthRatio = 0.53f;
                        boss.GetComponent<EnrageStateCDAuthoring>().enrageAtHealthRatio = 0.53f;
                        boss.GetComponent<HealthRegenerationCDAuthoring>().healInCombatAsWell = false;
                        boss.GetComponent<HealthRegenerationCDAuthoring>().healthPercentagePerFifthSecond = 0;
                        /*boss.GetComponent<MovementSpeedCDAuthoring>().speed = 420;*/
                        // big fucking mistake never do this again
                        //used to be 200
                        boss.GetComponent<MovementSpeedCDAuthoring>().speed = 150;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.ShamanBoss)
                    {
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 69420 * 2;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 69420;
                        //boss.GetComponent<PhaseTransitionStateCDAuthoring>().phase1HealthThreshold = 0.69f;
                        boss.GetComponent<RangeAttackStateCDAuthoring>().projectilesPerShot = 5;
                        boss.GetComponent<RangeAttackStateCDAuthoring>().spreadAngle = 60;
                        boss.GetComponent<RangeAttackStateCDAuthoring>().spreadType = ProjectileSpreadType.Random;
                        boss.GetComponent<MovementSpeedCDAuthoring>().speed = 120;
                        boss.GetComponent<ChaseStateAuthoringCD>().chaseAtDistance = 12;
                        boss.GetComponent<ChaseStateAuthoringCD>().moveSpeedMultiplier = 1.25f;
                        if (boss.GetComponent<DropsLootBufferAuthoring>() == null)
                            boss.gameObject.AddComponent<DropsLootBufferAuthoring>();
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().chance = 0.69f;
                        LootDrop lD = new LootDrop
                        {
                            lootDropID = Plugin.masterArmorPants,
                            amount = 1,
                            multiplayerAmountAdditionScaling = 0f
                        };
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values =
                            new Il2CppSystem.Collections.Generic.List<LootDrop>();
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values.Add(lD);
                        //  boss.GetComponent<RangeAttackStateCDAuthoring>().projectileID = Plugin.customProjectile;
                        //boss.GetComponent<MeleeAttackStateCDAuthoring>().objectToSpawnOnHitTiles = ObjectID.FireballProjectile;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.BirdBoss)
                    {
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 69420;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 69420;
                        if (boss.gameObject.GetComponent<BirdBossReworkCDAuthoring>() == null)
                            boss.gameObject.AddComponent<BirdBossReworkCDAuthoring>();
                        boss.gameObject.GetComponent<BirdBossReworkCDAuthoring>().radius.Value = 16;
                        boss.gameObject.GetComponent<BirdBossReworkCDAuthoring>().amount.Value = 75;
                        boss.gameObject.GetComponent<BirdBossReworkCDAuthoring>().type.Value = 0;
                        boss.gameObject.GetComponent<BirdBossReworkCDAuthoring>().iState.Value = 0;
                        //replaced rework with system based
                        /*boss.GetComponent<BirdBossAuthoring>().durationBeforeStartingToSpawnStones = 1;
                        boss.GetComponent<BirdBossAuthoring>().durationBeforeStartingToSpawnStones = 3;
                        var spwnCfg = new BirdBossAuthoring.SpawnConfiguration();
                        var spwnCfg2 = new BirdBossAuthoring.SpawnConfiguration();
                        spwnCfg.minCooldown = 10000;
                        spwnCfg.maxCooldown = 10000;
                        spwnCfg2.minCooldown = 2.5f;
                        spwnCfg2.maxCooldown = 5f;
                        boss.GetComponent<BirdBossAuthoring>().beamSpawn = spwnCfg;
                       // boss.GetComponent<BirdBossAuthoring>().stoneSpawn = spwnCfg2;
                       //GameObject.Destroy(boss.GetComponent<TeleportStateCDAuthoring>());
                       boss.GetComponent<TeleportStateCDAuthoring>().allowedRadiusToMoveFromPosition = 0;
                       boss.GetComponent<TeleportStateCDAuthoring>().minCooldown = 10000;
                       boss.GetComponent<TeleportStateCDAuthoring>().minCooldown = 10000;
                       boss.GetComponent<SpawnEntityOnDeathCDAuthoring>().spawnChance = 0.75f;*/
                    }
                    else if (boss.objectInfo.objectID == ObjectID.KingSlime)
                    {
                        boss.GetComponent<SlimeBossJumpStateCDAuthoring>().slimeTileset = Tileset.Sea;
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        int health = Random.Range(50000, 100000);
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = health;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = health;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.OctopusBoss)
                    {
                        if (boss.GetComponent<DropsLootBufferAuthoring>() == null)
                            boss.gameObject.AddComponent<DropsLootBufferAuthoring>();
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().chance = 1;
                        LootDrop lD = new LootDrop
                        {
                            lootDropID = Plugin.masterArmorBreastplate,
                            amount = 1,
                            multiplayerAmountAdditionScaling = 0f
                        };
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values =
                            new Il2CppSystem.Collections.Generic.List<LootDrop>();
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values.Add(lD);
                        LootDrop lD2 = new LootDrop
                        {
                            lootDropID = Plugin.myCustomShootThing,
                            amount = 1,
                            multiplayerAmountAdditionScaling = 0
                        };
                        boss.gameObject.GetComponent<DropsLootBufferAuthoring>().Values.Add(lD2);
                        if (boss.gameObject.GetComponent<OctopusModdedStateCDAuthoring>()==null)
                            boss.gameObject.AddComponent<OctopusModdedStateCDAuthoring>();
                        //this should be 0.75 and 0.35
                        boss.gameObject.GetComponent<OctopusModdedStateCDAuthoring>().hpToEnter.Value = 0.75f;
                        boss.gameObject.GetComponent<OctopusModdedStateCDAuthoring>().hpToEnter2.Value = 0.35f;
                        boss.gameObject.GetComponent<OctopusModdedStateCDAuthoring>().iteration.Value = 1;
                        boss.gameObject.GetComponent<OctopusModdedStateCDAuthoring>().tentacleCap.Value = 10;
                        if (boss.gameObject.GetComponent<DamageReductionCDAuthoring>() == null)
                            boss.gameObject.AddComponent<DamageReductionCDAuthoring>();
                        if (boss.gameObject.GetComponent<RotatingBeamCDAuthoring>() == null)
                            boss.gameObject.AddComponent<RotatingBeamCDAuthoring>();
                        boss.gameObject.GetComponent<RotatingBeamCDAuthoring>().id.Value = ObjectID.OctopusBossProjectile;
                        boss.gameObject.GetComponent<RotatingBeamCDAuthoring>().amt.Value = 18;
                        boss.gameObject.GetComponent<RotatingBeamCDAuthoring>().speed.Value = 1;
                    }

                    Plugin.logger.LogInfo(string.Format("Set {0}'s max health to {1}", boss.name,
                        boss.gameObject.GetComponent<HealthCDAuthoring>().maxHealth));
                    
                }
                else if (boss.objectInfo.objectType == ObjectType.Creature)
                {
                    if (boss.objectInfo.objectID == ObjectID.BigLarva && boss.objectInfo.variation == 2)
                    {
                        if (boss == null) 
                            continue;
                        if (boss.gameObject == null)
                            continue;
                        var place = boss.gameObject.GetComponent<EnsureSameGroundTileBeneathEntityCDAuthoring>();
                        if (place == null)
                            place = boss.gameObject.AddComponent<EnsureSameGroundTileBeneathEntityCDAuthoring>();
                        place.continouslyCheck = true;
                        place.tileType = TileType.ground;
                        Il2CppSystem.Collections.Generic.List<Tileset> e =
                            new Il2CppSystem.Collections.Generic.List<Tileset>();
                        e.Add(Tileset.LarvaHive);
                        place.onlySupportsTilesets = e;
                        place.fallbackTileset = Tileset.LarvaHive;
                        var chase = boss.GetComponent<ChaseStateAuthoringCD>();
                        chase.ignoreLowColliders = true;
                        var race = boss.gameObject.GetComponent<ControlledByOtherEntityCDAuthoring>();
                        if (race == null)
                            race = boss.gameObject.GetComponent<ControlledByOtherEntityCDAuthoring>();
                    }
                    else if (boss.objectInfo.objectID == ObjectID.MushroomEnemy)
                    {
                        boss.GetComponent<MovementSpeedCDAuthoring>().speed = 20;
                        boss.GetComponent<RandomWalkStateAuthoring>().minIdleDuration = 0.1f;
                        boss.GetComponent<RandomWalkStateAuthoring>().maxIdleDuration = 0.25f;
                        boss.GetComponent<RandomWalkStateAuthoring>().maxWalkDuration = 8;
                        boss.GetComponent<NearbyEntitiesTrackerCDAuthoring>().radius = 30;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.SlimeBlob)
                    {
                        //boss.GetComponent<FactionAuthoringCD>().faction = FactionID.OnlyAttacksPlayer;
                        boss.GetComponent<BehaviourTagsCDAuthoring>().wantsToAttackTags.Add(ObjectCategoryTag.Player);
                    }
                    else if (boss.objectInfo.objectID == ObjectID.CavelingBrute)
                    {
                        //boss.GetComponent<MovementSpeedCDAuthoring>().speed = 80f;
                        boss.GetComponent<HealthCDAuthoring>().dontCalculateHealthFromLevel = true;
                        boss.GetComponent<HealthCDAuthoring>().maxHealth = 5540;
                        boss.GetComponent<HealthCDAuthoring>().startHealth = 5540;
                        boss.GetComponent<RandomWalkStateAuthoring>().minIdleDuration = 0.1f;
                        boss.GetComponent<RandomWalkStateAuthoring>().maxIdleDuration = 0.25f;
                        boss.GetComponent<RandomWalkStateAuthoring>().maxWalkDuration = 8;
                        boss.GetComponent<NearbyEntitiesTrackerCDAuthoring>().radius = 30;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.CavelingHunter)
                    {
                        boss.GetComponent<MovementSpeedCDAuthoring>().speed = 50;
                        boss.GetComponent<ChaseStateAuthoringCD>().moveSpeedMultiplier = 1.5f;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.Larva)
                    {
                        if (boss.gameObject.GetComponent<LarvaFleeStateCDAuthoring>() == null)
                            boss.gameObject.AddComponent<LarvaFleeStateCDAuthoring>();
                        boss.gameObject.GetComponent<LarvaFleeStateCDAuthoring>().hpToFlee.Value = 0.40f;
                        boss.gameObject.GetComponent<LarvaFleeStateCDAuthoring>().hpToLeaveFlee.Value = 0.89f;
                        boss.gameObject.GetComponent<LarvaFleeStateCDAuthoring>().speedToFlee.Value = 2.0f;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.OctopusTentacle)
                    {
                        if (boss.gameObject.GetComponent<OctopusTenticleCounterCDAuthoring>() == null)
                            boss.gameObject.AddComponent<OctopusTenticleCounterCDAuthoring>();
                    }
                }
                else if (boss.objectInfo.objectType == ObjectType.NonObtainable)
                {
                    if (boss.objectInfo.objectID == ObjectID.BirdBossStone)
                    {
                        boss.GetComponent<HealNearbyEntitiesCDAuthoring>().radius = 27;
                        boss.GetComponent<HealNearbyEntitiesCDAuthoring>().healthPerSecond = 225;
                        if (boss.GetComponent<BirdStoneCDAuthoring>() == null)
                            boss.gameObject.AddComponent<BirdStoneCDAuthoring>();
                    }
                    else if (boss.objectInfo.objectID == ObjectID.BirdBossBeam)
                    {
                        boss.GetComponent<AttackContinuouslyAuthoring>().damageMultiplier = 3;
                        boss.GetComponent<AttackContinuouslyAuthoring>().damage = 300;
                    }
                }
                else if (boss.objectInfo.objectType == ObjectType.RangeWeapon)
                {
                    if (boss.objectInfo.objectID == ObjectID.ScholarStaff)
                    {
                        //boss.GetComponent<WeaponDamageCDAuthoring>().damage = 5000;
                    }
                    else if (boss.objectInfo.objectID == Plugin.myCustomShootThing)
                    {
                        if (boss.GetComponent<WindupCDAuthoring>() == null)
                            boss.gameObject.AddComponent<WindupCDAuthoring>();
                        var windup = boss.GetComponent<WindupCDAuthoring>();
                        windup.hasWindup = true;
                        windup.windupTiers = 4;
                        windup.windupTime = 12;
                        windup.weaponEffectType = WeaponEffectType.Metal;
                        windup.windupMultiplier = 0;
                    }
                }
                /*else if (boss.objectInfo.objectType == ObjectType.MeleeWeapon)
                {
                    boss.GetComponent<WeaponDamageCDAuthoring>().damage = 100000;
                }*/
                else if (boss.objectInfo.objectType == ObjectType.PlaceablePrefab)
                {
                    if (boss.objectInfo.objectID == ObjectID.ElectricalDoor)
                    {
                        boss.GetComponent<DamageReductionCDAuthoring>().reduction = 9999;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.FuryForge)
                    {
                        if (!isInitOnce)
                        {
                            CraftableObject myCraftableObject = new CraftableObject
                            {
                                objectID = Plugin.customWall,
                                amount = 1
                            };
                            boss.GetComponent<CraftingCDAuthoring>().canCraftObjects.Add(myCraftableObject);
                            isInitOnce = true;
                        }
                    }
                }
                else if (boss.objectInfo.objectType == ObjectType.Lantern)
                {
                    //nerfs stuff when afraid of the dark
                    if (boss.objectInfo.objectID == ObjectID.SmallLantern)
                    {
                        boss.gameObject.AddComponent<DurabilityCDAuthoring>();
                        boss.GetComponent<DurabilityCDAuthoring>().maxDurability = 100; // 20 sec
                        boss.objectInfo.initialAmount = 100;
                        boss.GetComponent<DurabilityCDAuthoring>().repairMultiplier = 5;
                    }
                    else if (boss.objectInfo.objectID == ObjectID.Lantern)
                    {
                        boss.gameObject.AddComponent<DurabilityCDAuthoring>();
                        boss.GetComponent<DurabilityCDAuthoring>().maxDurability = 100; // 20 sec
                        boss.objectInfo.initialAmount = 100;
                        boss.GetComponent<DurabilityCDAuthoring>().repairMultiplier = 5;
                    }
                }
                else if (boss.objectInfo.objectType == ObjectType.CastingItem)
                {
                    if (boss.objectInfo.objectID == Plugin.wormholeIdol)
                    {
                        boss.GetComponent<CastItemCDAuthoring>().useType = CastItemUseType.TeleportToCore;
                    }
                }
                /*else if (boss.objectInfo.objectType == ObjectType.Helm)
                {
                    if (boss.objectInfo.objectID == Plugin.masterArmorHelm)
                    {
                        //if (!boss.TryGetComponent<ModEquipmentSkinCDAuthoring>(out ModEquipmentSkinCDAuthoring modEquipmentSkinCdAuthoring))
                        {
                          //  boss.gameObject.AddComponent<ModEquipmentSkinCDAuthoring>();
                            boss.gameObject.GetComponent<ModEquipmentSkinCDAuthoring>().skinTexture = Plugin.resource.bundle.LoadAsset<Texture2D>("Assets/PainMod/Textures/TitanSet/Player/TitanHelm.png");
                            boss.gameObject.GetComponent<ModEquipmentSkinCDAuthoring>().helmHairType =
                                HelmHairType.FullyShow;
                        }
                    }
                } */
            }
        }
        /*[HarmonyPatch(typeof(Projectile), nameof(Projectile.OnOccupied))]
        [HarmonyPostfix]
        public static void OnOccupied(Projectile __instance)
        {
            CustomEnergyModProjectile myProjectile = __instance.TryCast<CustomEnergyModProjectile>();

            if (myProjectile != null)
            {
                myProjectile.OnOccupied();
            }
        }*/
/*
        [HarmonyPatch(typeof(LootTableBank), nameof(LootTableBank.InitLoot))]
        [HarmonyPrefix]
        public static void GetLootTableBank(LootTable lootTable, LootList lootInfos, int minUniqueDrops,
            int maxUniqueDrops, LootList guaranteedLootInfos)
        {
            if(lootTable.id==LootTableID.HiveBoss)
                lootTable.dontAllowDuplicates = true;
            if(lootTable.id==LootTableID.ShamanBoss)
                lootTable.dontAllowDuplicates = true;
            if(lootTable.id==LootTableID.OctopusBoss)
                lootTable.dontAllowDuplicates = true;
        } */
    }
}