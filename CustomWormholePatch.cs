using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Submodules.ModComponent;
using CoreLib.Util.Extensions;
using HarmonyLib;
using PlayerState;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Playables;

namespace PainMod;
[HarmonyPatch]
public class CustomWormholePatch
{
    private static Casting currentCastingInstance;
    [HarmonyPatch(typeof(Casting), nameof(Casting.FinishCastingItem))]
    [HarmonyPostfix]
    public static void PreMemInit(Casting __instance)
    {
        if (__instance.objectData.objectID == Plugin.wormholeIdol)
        {
            currentCastingInstance = __instance;
            if (Manager._instance.allPlayers.Count < 2)
            {
                Emote.SpawnEmoteText(__instance.pc.center, (Emote.EmoteType)500 , true, false, true);
                Plugin.logger.LogInfo("You dont have anyone to teleport to!");
            }
            else
            {
                //Emote.SpawnEmoteText(__instance.pc.center, Emote.EmoteType.NotFullyCharged, true, false, true);
                if (!Manager.ui.isShowingMap)
                {
                    Manager.ui.OnMapToggle();
                }
            }
        }
        Plugin.logger.LogInfo("Invalid used object!");
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapMarkerUIElement), "OnLeftClicked")]
    public static void S(MapMarkerUIElement __instance)
    {
        var pc = Players.GetCurrentPlayer();
        if (__instance.markerType == MapMarkerType.Player)
        {
            if (!(__instance.player == null || __instance.player.wasInBoatPreviousFrame != pc.wasInBoatPreviousFrame))
            {
                if (pc.playerInventoryHandler.GetExistingAmountOfObject(Plugin.wormholeIdol) >= 1)
                {
                    CoroutineHelper.StartCoroutine(Teleport(currentCastingInstance, __instance));
                    pc.playerInventoryHandler.DestroyUpToAmountOfEntity(Plugin.wormholeIdol, 1);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Casting), nameof(Casting.StartCastingItem))]
    [HarmonyPostfix]
    public static void StartCastItemPatch(Casting __instance)
    {
        if (__instance.objectData.objectID == Plugin.wormholeIdol)
        {
            __instance.pc.PlayAnimationTrigger(AnimID.startTeleport);
        }
    }

    [HarmonyPatch(typeof(Casting), nameof(Casting.StopCastingItem))]
    [HarmonyPostfix]
    public static void StopCastItemPatch(Casting __instance)
    {
        if (__instance.objectData.objectID == Plugin.wormholeIdol)
        {
            if(__instance.hasTeleported)
                __instance.pc.PlayAnimationTrigger(AnimID.endTeleport);
        }
    }

    private static CameraSceneFader.FadeSettings black = new CameraSceneFader.FadeSettings(
        CameraSceneFader.FadeStyle.BLACK, CameraSceneFader.FadeCurve.SMOOTH, CameraSceneFader.FadeStyle.BLACK,
        CameraSceneFader.FadeCurve.STRAIGHT);
    private static IEnumerator TeleportToNearest(Casting casting)
    {
        bool hasTeleported = false;
        Manager._instance.player.LockCurrentState();
        Manager._instance._uiManager.FadeOutAllGameplayUI();
        Manager._instance._uiManager.FadeInMouse();
        Manager._instance.player.AE_HidePlayer();
        yield return new WaitForSeconds(0.1f);
        casting.StopCastingItem();
        hasTeleported = true;
        yield return new WaitForSeconds(1.0f);
        Manager._instance._loadManager.FadeOut(2.0f, black);
        yield return new WaitForSeconds(2.0f);
        yield return new WaitForSeconds(1.0f);
        TeleportToFriend(casting);
        Manager._instance.player.UpdateAllEquipmentSlots();
        yield return new WaitForSeconds(1.0f);
        Manager._instance._uiManager.FadeInAllGameplayUI();
        Manager._instance._uiManager.FadeInMouse();
        Manager._instance._loadManager.FadeIn(1.0f, black);
        Manager._instance.player.UnlockCurrentState(false);

        // Mode code
    }
    
    private static IEnumerator Teleport(Casting casting, MapMarkerUIElement mapMarkerUIElement)
    {
        PlayerController pc = casting.pc;
        pc.LockCurrentState();
        Manager.ui.FadeOutAllGameplayUI();
        Manager.ui.FadeInMouse();
        pc.isDyingOrDead = true;

        EntityUtility.AddComponent(pc.entity, casting.world, ComponentType.ReadOnly<PhysicsExclude>());
    
        pc.playerCommandSystem.SetPlayerInvincible(pc.entity, true);

        Vector3 renderPos = pc.RenderPosition;
        Vector3 newPos = renderPos + new Vector3(0, 5, -5);
    

        EffectEventCD effectEventCD = new EffectEventCD()
        {
            effectID = EffectID.TeleportExplosion,
            position1 = renderPos.ToFloat3(),
            position2 = newPos.ToFloat3(),
            entity = pc.entity
        };
        EntityUtility.PlayEffectEventClient(effectEventCD);
    
        pc.flashable.FlashCustomColor(1);
        pc.PlayAnimationTrigger(AnimID.hidePlayer);
        yield return new WaitForSeconds(0.1f);
    
        casting.StopCastingItem();
        casting.hasTeleported = true;
        yield return new WaitForSeconds(1f);
        
        Manager.load.FadeOut(2, FadePresets.blackToBlack);
        yield return new WaitForSeconds(2f);

        ClientInput clientInput = pc.clientInput;
        clientInput.position = float2.zero;
        pc.clientInput = clientInput;
        yield return new WaitForSeconds(1f);
        
        TeleportToPlayer(casting.pc,mapMarkerUIElement.player);
        //TeleportToFriend(casting);
        pc.UpdateAllEquipmentSlots();
        EntityUtility.RemoveComponentData(pc.entity, casting.world, ComponentType.ReadOnly<PhysicsExclude>());
        pc.playerCommandSystem.SetPlayerInvincible(pc.entity, false);
        pc.isDyingOrDead = false;
        // Some pc.recentDamagers shenanigans, but i'm too sleepy to recover what high level action it's actually is (PC.isdyingordead)
        yield return new WaitForSeconds(1f);
    
        Manager.ui.FadeInAllGameplayUI();
        Manager.ui.FadeInMouse();
        Manager.load.FadeIn(1, FadePresets.blackToBlack);
        casting.itemIsInProcessOfBeingUsed = false;
        pc.UnlockCurrentState();
    }

    public static void TeleportToPlayer(PlayerController me,PlayerController pc)
    {
        new Teleporting(me)
        {
            targetPosition = pc.WorldPosition
        }.MovePlayerToTargetDestination();
    }

    private static void TeleportToFriend(Casting casting)
    {
        List<Vector3> pos = new List<Vector3>();
        List<float> dist = new List<float>();
        List<float> distSorted = new List<float>();
        Plugin.logger.LogInfo("players count here is " + Manager._instance.allPlayers.Count);
        casting.pc.isDyingOrDead = false;
        if (Manager._instance.allPlayers.Count > 1)
        {
            for (int i = 0; i < Manager._instance.allPlayers.Count; i++)
            {
                pos.Add(Manager._instance.allPlayers._items[i].WorldPosition);
                dist.Add(Vector3.Distance(Manager._instance.player.WorldPosition,
                    Manager._instance.allPlayers._items[i].WorldPosition));
                distSorted.Add(Vector3.Distance(Manager._instance.player.WorldPosition,
                    Manager._instance.allPlayers._items[i].WorldPosition));
                Plugin.logger.LogInfo("player "+ i + " at" + Manager._instance.allPlayers._items[i].WorldPosition + " and " + Manager._instance.allPlayers._items[i].RenderPosition);
            }
            distSorted.Sort();
            distSorted.RemoveAt(0);
            int ind = dist.IndexOf(distSorted[0]);
            Manager._instance.player.SetPlayerPosition(pos[ind]);
        }
        else
        {
            Plugin.logger.LogInfo("must be lonely huh, well ill send you away!!!");
            Manager._instance.player.SetPlayerPosition(new Vector3(30000, 1, 1));
        }
        casting.pc.shadow.gameObject.SetActive(true);
    }

}