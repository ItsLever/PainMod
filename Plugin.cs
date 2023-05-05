using System.Collections.Generic;
using AsmResolver.PE.Win32Resources;
using BepInEx;
//using BepInEx.IL2CPP;
using BepInEx.Logging;
//using Cpp2IL.Core;
using BepInEx.Unity.IL2CPP;
using CoreLib;
using CoreLib.Components;
using CoreLib.Submodules.ChatCommands;
using CoreLib.Submodules.ModEntity;
using CoreLib.Submodules.ModEntity.Atributes;
using CoreLib.Submodules.DropTables;
using CoreLib.Submodules.Localization;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModEntity;
using CoreLib.Submodules.ModEntity.Atributes;
using CoreLib.Submodules.ModResources;
using CoreLib.Submodules.ModSystem;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using Il2CppSystem.Reflection;
using PlayerState;
using PugTilemap;
using PugTilemap.Workshop;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Path = System.IO.Path;
using PluginInfo = BepInEx.PluginInfo;
using ResourceData = CoreLib.Submodules.ModResources.ResourceData;

//using PainMod;
//using UnhollowerBaseLib;
//using UnhollowerRuntimeLib;
//using Unity.Entities;
//using UnityEngine;
//using Logger = Cpp2IL.Core.Logger;

namespace PainMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(CoreLibPlugin.GUID)]
    [CoreLibSubmoduleDependency(nameof(LocalizationModule), nameof(EntityModule))]
    [CoreLibSubmoduleDependency(nameof(DropTablesModule))]
    [CoreLibSubmoduleDependency(nameof(CommandsModule))]
    [CoreLibSubmoduleDependency(nameof(SystemModule))]
    [CoreLibSubmoduleDependency(nameof(ComponentModule))]
    [EntityModification]
    public class Plugin : BasePlugin
    {
        public static Tileset brick1;
        public static TileTypeColorTable.TileColor tileColor1;
        //public static ObjectID ProjectileSBMod;
        public static ObjectID myCustomModdedProjectile;
        public static ObjectID masterArmorHelm;
        public static ObjectID masterArmorBreastplate;
        public static ObjectID masterArmorPants;
        public static ObjectID fungalCap;
        public static ManualLogSource logger;
        public static ObjectID customWall;
        public static ResourceData resource;
        public static ObjectID myCustomShootThing;
        public static bool mineChallengeActive = false;
        public static bool ghormChallengeActive = false;
        public static int hz;
        public static bool darkChallengeActive = false;
        public static ObjectID maxHealthBuffStation;
        public static ObjectID wormholeIdol;
        public override void Load()
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomEnergyModProjectile>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomBuffingStation>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomMaxHpStation>();
            ClassInjector.RegisterTypeInIl2Cpp<FirstSystemTest>();
            AddComponent<FirstSystemTest>();
            SystemModule.RegisterSystem<CustomStateSystem>();
            ComponentModule.RegisterECSComponent<OctopusModdedStateCD>();
            ComponentModule.RegisterECSComponent<OctopusModdedStateCDAuthoring>();
            ComponentModule.RegisterECSComponent<MustBeDestroyedForOctopusLeaveStateCD>();
            ComponentModule.RegisterECSComponent<MustBeDestroyedForOctopusLeaveStateCDAuthoring>();
            logger = Log;
            // Plugin startup logic
            ClassInjector.RegisterTypeInIl2Cpp<MonoStuff>();
            //ClassInjector.RegisterTypeInIl2Cpp<ModEnergyProjectile>();

            string pluginfolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
         //   logger.LogInfo("plugin folder at " + pluginfolder);

            resource = new ResourceData(PluginInfo.PLUGIN_GUID, "PainMod", pluginfolder);
            resource.LoadAssetBundle("modassets");
            ResourcesModule.AddResource(resource);
//eeeee
           // ObjectID objectID = CustomEntityModule.AddEntity("PainMod:MyModdedWall", "ModdedPrefabs/MyModWall");

            //  ObjectID workbench = CustomEntityModule.AddModWorkbench("PainMod:Workbench", "Assets/PainMod/Resources/kiln.png", 
             //   new List<CraftingData> {new CraftingData(ObjectID.ScarletBar, 4)});
          //  CustomEntityModule.AddWorkbenchItem(workbench, objectID);
           // CustomEntityModule.AddEntityLocalization(workbench,
             //   "ModWorkBench",
               // "testing.");
               
            EntityModule.AddCustomTileset("Assets/PainMod/ScriptableObjects/Tilesets/BrickTileset.asset");
            customWall = EntityModule.AddEntity("PainMod:BrickWall", "Assets/PainMod/Assets/CustomWall.prefab");
            
            EntityModule.AddEntityLocalization(customWall,
                "Clay Brick wall",
                "Brick");
            Tileset brick = EntityModule.GetTilesetId("PainMod:BrickTileset");
            brick1 = brick;
            
       //     CustomEntityModule.AddWorkbenchItem(workbench, customWall);
            
         /*   
            masterArmorHelm = CustomEntityModule.AddEntity("PainMod:MasterArmorHelm", "Assets/PainMod/Assets/MasterArmor.prefab");

            CustomEntityModule.AddEntityLocalization(masterArmorHelm,
                "Ancient Armor Helm",
                "A masterfully crafted battle helm once overtaken by a misshapen being. Its engravings tell a story you can't quite comprehend.");
            
            masterArmorBreastplate = CustomEntityModule.AddEntity("PainMod:MasterArmorBreastplate", "Assets/PainMod/Assets/MasterArmorBreastplate.prefab");

            CustomEntityModule.AddEntityLocalization(masterArmorBreastplate,
                "Ancient Armor Breastplate",
                "Heavy plate mail formerly worn by a legendary warrior. Pulled away from the firm grasp of an abyssal colossus, it's still as polished as it has ever been.");
            
            masterArmorPants = CustomEntityModule.AddEntity("PainMod:MasterArmorPants", "Assets/PainMod/Assets/MasterArmorPants.prefab");

            CustomEntityModule.AddEntityLocalization(masterArmorPants,
                "Ancient Armor Pants",
                "Divine leggings that allow its wearer to perform the most dexterous leaps and dodges. Fervently treasured by a figure of power from a bygone age. ");
        */
         fungalCap = EntityModule.AddEntity("PainMod:FungalCap", "Assets/PainMod/Assets/ShroomCap.prefab");
            
            EntityModule.AddEntityLocalization(fungalCap,
                "Funbrella",
                "Hm. what an oddity, it fills you with the urge to run");
            myCustomShootThing = EntityModule.AddEntity("PainMod:ShootyThing", "Assets/PainMod/Assets/ModStaffEntity.prefab");
            EntityModule.AddEntityLocalization(myCustomShootThing, "Totally not a ripoff of the Scholar Staff™", "What do you think of my pretty gradients? :D");
            
            DropTablesModule.AddNewDrop(LootTableID.MushroomEnemy, new DropTableInfo(fungalCap, 1, 0.0125f));
           // mineChallengeActive = true;
           CommandsModule.AddCommands(System.Reflection.Assembly.GetExecutingAssembly(), PluginInfo.PLUGIN_NAME);
           
           
           wormholeIdol = EntityModule.AddEntity("PainMod:TeleporterToFriend", "Assets/PainMod/Assets/TeleportIdol.prefab");
            
           EntityModule.AddEntityLocalization(wormholeIdol,
               "Teleportation Idol",
               "Cast to teleport to the nearest player further than 10 blocks away");
           /*  Texture2D armorTexture = 
           
             byte skinId = CustomEntityModule.AddPlayerCustomization(new HelmSkin()
             {
                 helmTexture = armorTexture,
                 hairType = HelmHairType.FullyShow
             });
             CustomEntityModule.SetEquipmentSkin(masterArmorHelm, skinId);*/
           // DropTablesModule.AddNewDrop(LootTableID.HiveBoss, new DropTableInfo(masterArmorHelm, 1,1f, true));
           // DropTablesModule.AddNewDrop(LootTableID.OctopusBoss, new DropTableInfo(masterArmorBreastplate, 1,1f, true));
           // DropTablesModule.AddNewDrop(LootTableID.ShamanBoss, new DropTableInfo(masterArmorPants, 1,0.69f, true));
            
            //temporary
           // CustomEntityModule.AddWorkbenchItem(workbench, masterArmorHelm);
           // CustomEntityModule.AddWorkbenchItem(workbench, masterArmorBreastplate);
           // CustomEntityModule.AddWorkbenchItem(workbench, masterArmorPants);
           hz = Screen.currentResolution.refreshRate;
            //ProjectileSBMod = CustomEntityModule.AddEntity("PainMod:ModShamanProjectile", "Assets/PainMod/Assets/AnotherCustomProjectile.prefab");
            myCustomModdedProjectile = EntityModule.AddEntity("PainMod:MyCustomModdedProjectile",
                "Assets/PainMod/Assets/ModdedEnergyProjectileEntity.prefab");
            maxHealthBuffStation = EntityModule.AddEntity("PainMod:MaxHpBuffStation", "Assets/PainMod/Assets/CustomBuffStationEntity.prefab");
            EntityModule.AddEntityLocalization(maxHealthBuffStation, "Max Health Buffing Station", "Free extra HP! Seems like you're going to need it.");
            /* CustomEntityModule.AddEntityLocalization(objectID,
                 "Wall",
                 "yeah theres no way this will work lol."); */
            //logger.LogInfo("Projectile SB mod is: " + ProjectileSBMod);
            LocalizationModule.AddTerm("ModEmotes/DarknessWarn", "Its dark in here");
            LocalizationModule.AddTerm("ModEmotes/DarknessWarn2", "Lights out!");
            LocalizationModule.AddTerm("ModEmotes/GoodLuck", "Gulp");
            LocalizationModule.AddTerm("ModEmotes/BewareWorm", "This can't be good...");
            LocalizationModule.AddTerm("ModEmotes/ShamanBossAwaken", "It's not over");
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID); 
            harmony.PatchAll();
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            //AddComponent<MonoStuff>();
        }
        
        
            /*foreach (EntityMonoBehaviourData e in gae)
            {
                foreach (PrefabInfo prefabInfo in e.objectInfo.prefabInfos)
                {
                    if (e.objectInfo.objectID == ObjectID.LarvaHiveBoss)
                    {
                        prefabInfo.prefab.gameObject.GetComponent<HealthCDAuthoring>().maxHealth = 69420;
                        prefabInfo.prefab.gameObject.GetComponent<HealthCDAuthoring>().startHealth = 69420;
                        prefabInfo.prefab.gameObject.GetComponent<ShootMortarProjectileStateCDAuthoring>().mortarDamage = 500;
                        prefabInfo.prefab.gameObject.GetComponent<ShootMortarProjectileStateCDAuthoring>().goUpTime = 0.3f;
                    }
                }
            }*/
    }
    [HarmonyPatch(typeof(Manager))]
    internal class ManagerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void ManagerUpdatePatch(Manager __instance)
        {
            // Your code here, use __instance to get access to the Manager Instance
        }
    }
}
