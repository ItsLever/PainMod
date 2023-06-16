using CoreLib.Submodules.ModSystem;
using HarmonyLib;
using Unity.Entities;
using UnityEngine;

namespace PainMod;
[HarmonyPatch]
internal class SecondaryUseCustomPatch
{
    public static bool isStarting = false;
    public static int currentTier = -1;
    public static float duration;
    public static bool tierIsDifferentToLastFrame = false;
    public static bool justStarted = false;
    [HarmonyPatch(typeof(EquipmentSlot), nameof(EquipmentSlot.StartWindup))]
    [HarmonyPostfix]
    public static void OnWindup(EquipmentSlot __instance)
    {
        if (__instance.objectData.objectID != Plugin.myCustomShootThing)
            return;
        isStarting = true;
        duration = __instance.windupTimer.lifespan;
        Plugin.logger.LogInfo("Wind up time is " + duration + "and current windup tier is " + currentTier);
    }

    [HarmonyPatch(typeof(EquipmentSlot), nameof(EquipmentSlot.Windup))]
    [HarmonyPostfix]
    public static void OnWindupDone(EquipmentSlot __instance)
    {
        if (__instance.objectData.objectID != Plugin.myCustomShootThing)
            return;
        if (currentTier != __instance.currentWindupTier)
        {
            Plugin.logger.LogInfo("Current windup tier is now " + __instance.currentWindupTier);
            tierIsDifferentToLastFrame = true;
            if (currentTier == 0)
                justStarted = true;
        }
        else
        {
            tierIsDifferentToLastFrame = false;
            justStarted = false;
        }
        currentTier = __instance.currentWindupTier;
    }
}