using System;
using System.Collections.Generic;
using CoreLib;
using HarmonyLib;
using PlayerCommand;
using PugRP;
using PugTilemap;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Experimental.Rendering;
using UnityEngine.PlayerLoop;

namespace PainMod
{
    [HarmonyPatch]
    internal class Challenges
    {
        #region MineSweeperChallenge
        private static int blocksBroken = 0;
        private static int slimeEventCount = 0;
        private static int bombEventCount = 0;
        
        [HarmonyPatch(typeof(EntityMonoBehaviour), nameof(EntityMonoBehaviour.OnDeath))]
        [HarmonyPostfix]
        public static void IHaveTheBomb(EntityMonoBehaviour __instance)
        {
            if (Plugin.mineChallengeActive)
            {
                Plugin.logger.LogInfo("Something has died and mine challenge active");
                if (__instance.objectInfo.objectType == ObjectType.PlaceablePrefab)
                {
                    if (Random.value <= 0.25)
                        Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.LargeBomb, Manager._instance.player.RenderPosition);
                    Plugin.logger.LogInfo("block has died and mine challenge active");
                }
            }
        }

        [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnDamagingTile))]
        [HarmonyPostfix]
        public static void BreakWallBomb(TileInfo tileInfo, int damageDone, float normHealth, float3 pos ,PlayerController __instance)
        {
            if (Plugin.mineChallengeActive)
            {
                Plugin.logger.LogInfo("WallBlock is being mined");
                Plugin.logger.LogInfo("state is: " + tileInfo.state);
                Plugin.logger.LogInfo("damage done is " + damageDone + " and normal health is " + normHealth);
                if (tileInfo.tileset == (int) Tileset.Dirt || tileInfo.tileset == (int) Tileset.Clay ||
                    tileInfo.tileset == (int) Tileset.LarvaHive || tileInfo.tileset == (int) Tileset.Stone
                    || tileInfo.tileset == (int) Tileset.Nature || tileInfo.tileset == (int) Tileset.Mold ||
                    tileInfo.tileset == (int) Tileset.Turf || tileInfo.tileset == (int) Tileset.Sea
                    || tileInfo.tileset == (int) Tileset.Sand || tileInfo.tileset == (int) Tileset.Desert ||
                    tileInfo.tileset == (int) Tileset.Lava)
                {
                    if (normHealth <= 0)
                    {
                        float rand = Random.value;
                        if (rand <= 0.15f)
                        {
                            Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.LargeBomb,
                                Manager._instance.player.RenderPosition);
                            bombEventCount++;
                        }
                        else if (rand <= 0.20f)
                        {
                            Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.SlimeBlob, pos);
                            slimeEventCount++;
                        }

                        blocksBroken++;
                        Plugin.logger.LogInfo("block has died and mine challenge active");
                        Plugin.logger.LogInfo("You have broken " + blocksBroken + "blocks, of these "
                                              + slimeEventCount + " has spawned slimes upon breaking, and " +
                                              bombEventCount + " " + "has spawned bombs upon breaking");
                    }
                }
                // Manager._instance.player.DigUpTile();
            }
        }


        #endregion

        #region GhormKeeperChallenge

        private static List<GameObject> validBossLarvae = new List<GameObject>();
        [HarmonyPatch(typeof(BossLarva), nameof(BossLarva.OnOccupied))]
        [HarmonyPostfix]
        public static void GhormIsSmol()
        {
            SetValidBossLarvae();
            
            GameObject bossLarvaInstance = new GameObject();
            if (validBossLarvae.Count > 0)
                bossLarvaInstance = validBossLarvae[0];
            if(Plugin.ghormChallengeActive)
                bossLarvaInstance.gameObject.transform.GetChild(0).transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            else
                bossLarvaInstance.gameObject.transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private static void SetValidBossLarvae()
        {
            var origo = Manager._instance._cameraManager.OrigoTransform;
            foreach (var childtransform in origo.GetComponentsInChildren<Transform>())
            {
                try
                {

                    if (childtransform.gameObject.name == "BossLarva(Clone)")
                    {
                        //   var sb = childtransform.gameObject.GetComponent<ShamanBoss>();
                        if (childtransform.gameObject.active)
                            validBossLarvae.Add(childtransform.gameObject);
                        Plugin.logger.LogInfo("A ghorm boss has been registered!");
                    }
                }
                catch
                {
                    Plugin.logger.LogInfo("something went wrong!");
                }
            }
        }

        [HarmonyPatch(typeof(ClientSystem), nameof(ClientSystem.DealDamageToEntity))]
        [HarmonyPostfix]
        public static void GetGhormedLol(ClientSystem __instance, bool wasKilled, float3 damagePosition, bool isDigging)
        {
            
            if (Plugin.ghormChallengeActive)
            {
                if (wasKilled)
                {
                    Plugin.logger.LogInfo("Enemy has been killed!");
                    if(!isDigging)
                        Plugin.logger.LogInfo("Boss has been defeated!");
                    if (Random.value <= 0.05)
                        Manager._instance.player.playerCommandSystem.CreateEntity(ObjectID.BossLarva,
                            damagePosition);
                }
            }
        }

        private static float threshold = 0.001f;
        private static float lastCount = 0;
        private static float count = 0;
        private static bool isSameAsLastCount = false;
        [HarmonyPatch(typeof(Manager), nameof(Manager.Update))]
        [HarmonyPostfix]
        public static void LightRead(Manager __instance)
        {
            if(!__instance.currentSceneHandler.isInGame)
                return;
            count += Time.deltaTime;
            if ((int) count != (int) lastCount)
                isSameAsLastCount = false;
            else
                isSameAsLastCount = true;
            if (!isSameAsLastCount)
            {
                Plugin.logger.LogInfo("Count is " + (int) count + " and time delta time is " + Time.fixedDeltaTime);
                if (Manager._instance.player.GetOffHand().objectID == ObjectID.Lantern || Manager._instance.player.equipmentHandler.lanternInventoryHandler.GetObjectData(0).objectID == ObjectID.SmallLantern)
                {
                    if (Plugin.darkChallengeActive)
                    {
                        Plugin.logger.LogInfo("You have light? it will now take damage");
                        Manager._instance.player.ReduceDurabilityOfEquipment(Manager._instance.player.equipmentHandler.offHandInventoryHandler, 5);
                    }
                }
            }

            //count++;
            if (Plugin.darkChallengeActive)
            {
                if (!isSameAsLastCount && ((int) (count + 2)) % 5 == 0)
                {
                    Plugin.logger.LogInfo("rt2d is maybe null");
                    if(rt2d==null)
                        return;
                    Plugin.logger.LogInfo("rt2d is not null");
                    Vector3 p = Manager.camera.gameCamera.WorldToScreenPoint(Manager._instance.player.gameObject.transform.position);
                    Vector3 a = Manager.camera.gameCamera.WorldToScreenPoint(Manager.camera.gameCamera.transform.position);
                    Vector2 d = new Vector2((p.x-a.x), p.y-a.y);
                    anticipationTexture2D = CreateTextureThing(PugRP.PugRP.s_cameraData[Manager.camera.gameCamera].GetRenderFeature<IndirectLightRenderFeature>().m_indirectIrradiance/*Manager._instance._loadManager IndirectLightBuffer*/,
                        out int w, out int h);
                    Vector2 str3 = new Vector2(((int)(w/2)) + (d.x * (w/(a.x*2)))*0.8f, ((int)(h / 2)) + (d.y * (h/(a.y*2)))*0.8f);
                    Color atPixel = anticipationTexture2D.GetPixel((int) str3.x, (int) str3.y);
                    if ((atPixel.r + atPixel.g + atPixel.b) / 3 < threshold)
                    {
                        Plugin.logger.LogInfo("Under threshold and should do attack sound now ");
                        AudioManager.Sfx(SfxID.scarabBossAttack, Manager._instance.player.gameObject.transform.position,
                            1.4f, 1, 0.1f);
                    }
                }

                if (!isSameAsLastCount && ((int) count % /*((int)(Plugin.hz)) */ 5 == 0))
                {
                   // Vector3 p = Manager.camera.gameCamera.WorldToScreenPoint(Manager._instance.player.gameObject.transform.position);
                   // Vector3 a = Manager.camera.gameCamera.WorldToScreenPoint(Manager.camera.gameCamera.transform.position);
                 //  PugRP.IndirectLightRenderFeature e;
               //    e.indirectIrradiance;
               Vector3 p = Manager.camera.gameCamera.WorldToScreenPoint(Manager._instance.player.gameObject.transform.position);
                   Vector3 a = Manager.camera.gameCamera.WorldToScreenPoint(Manager.camera.gameCamera.transform.position);
                    Vector2 d = new Vector2((p.x-a.x), p.y-a.y);
                    Plugin.logger.LogInfo("d is " + d.ToString() + "p is " + p.ToString() + " and a is " + a.ToString());
                    rt2d = CreateTextureThing(PugRP.PugRP.s_cameraData[Manager.camera.gameCamera].GetRenderFeature<IndirectLightRenderFeature>().m_indirectIrradiance/*Manager._instance._loadManager IndirectLightBuffer*/,
                        out int w, out int h);
                  /*  Color atPixel = rt2d
                        .GetPixel((w/2) + ((int)d.x/2 * w)/Manager.camera.gameCamera.pixelWidth, (h / 2) + (int)d.y/2 * h/Manager.camera.gameCamera.pixelHeight);*/
                  /*
                  Color atPixel = rt2d
                      .GetPixel((w/2) + ((int)d.x * w)/Screen.width, (h / 2) + (int)d.y * h/Screen.height);
                  */
                  Vector2 str3 = new Vector2(((int)(w/2)) + (d.x * (w/(a.x*2)))*0.8f, ((int)(h / 2)) + (d.y * (h/(a.y*2)))*0.8f);
                  Color atPixel = rt2d
                      .GetPixel((int)str3.x, (int)str3.y);
                  
                    Color.RGBToHSV(atPixel, out var H, out var s, out var v);
                    //TEMP DISABLED
                    Plugin.logger.LogInfo("Using average brightness (r+g+b)/3, the result at center of texture is " +
                                          (atPixel.r + atPixel.g + atPixel.b) / 3 + " and using to HSV its: " +
                                          v + "Additionally, game camera pixel width is " + 
                                          PugRP.PugRP.s_cameraData[Manager.camera.gameCamera].GetRenderFeature<IndirectLightRenderFeature>().m_indirectIrradiance.width + "and pixel height is " + 
                                          PugRP.PugRP.s_cameraData[Manager.camera.gameCamera].GetRenderFeature<IndirectLightRenderFeature>().m_indirectIrradiance.height);
                    Vector2 str2 = new Vector2((w/2) + ((int)d.x/2 * w)/Manager.camera.gameCamera.pixelWidth, (h / 2) + (int)d.y/2 * h/Manager.camera.gameCamera.pixelHeight);
                    Vector2 str = new Vector2((w/2) + ((int)d.x * w)/Screen.width, (h / 2) + (int)d.y * h/Screen.height);
                    
                    for (int i = (int)str.x - 5; i < (int)str.x + 5; i++)
                        for (int j = (int)str.y - 5; j < (int)str.y + 5; j++)
                            rt2d.SetPixel(i,j, Color.red);
                    for (int i = (int)str2.x - 5; i < (int)str2.x + 5; i++)
                    for (int j = (int)str2.y - 5; j < (int)str2.y + 5; j++)
                        rt2d.SetPixel(i,j, Color.magenta);
                    for (int i = (int)str3.x - 5; i < (int)str3.x + 5; i++)
                    for (int j = (int)str3.y - 5; j < (int)str3.y + 5; j++)
                        rt2d.SetPixel(i,j, Color.green);
                    if ((atPixel.r + atPixel.g + atPixel.b) / 3 < threshold)
                    {
                        Plugin.logger.LogInfo("Uhoh brightness value is low get out, quickly before you die");

                        {
                            Plugin.logger.LogError("Uh oh damage time ig!");
                            Manager._instance.player.DealDamageToPlayer(50, /*Manager._instance.player.WorldPosition*/ float3.zero, 0);
                        }
                    }

                    Plugin.logger.LogInfo("Player render position is " + Manager._instance.player.RenderPosition.x +
                                          ", " +
                                          Manager._instance.player.RenderPosition.z);
                    Vector3 worldPos = Manager._instance.player.RenderPosition;
                    Vector3Int origo = Manager.camera.RenderOrigo;
                    Plugin.logger.LogInfo("disp = " + (worldPos - origo));
                    Plugin.logger.LogInfo("Frame rate is " + Manager._instance._prefsManager.targetFrameRate);
                    rt2d.Apply();
                    //UnityExplorer.InspectorManager.Inspect(rt2d);
                }
            }

            lastCount = count;
        }

        public static Texture2D rt2d;
        public static Texture2D anticipationTexture2D;
        public static Texture2D CreateTextureThing(RenderTexture currentTarget, out int w, out int h)
        {
            RenderTexture rt = (RenderTexture)currentTarget;
            w = rt.width;
            h = rt.height;
            RenderTexture old = RenderTexture.active;
            Texture2D texture2D = new Texture2D(rt.width, rt.height, rt.graphicsFormat, TextureCreationFlags.None);

            RenderTexture.active = rt;
            texture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = old;
            return texture2D;
        }

        #endregion
    }
}