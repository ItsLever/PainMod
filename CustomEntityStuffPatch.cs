using HarmonyLib;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PainMod
{
    [HarmonyPatch]
    internal class CustomEntityStuffPatch
    {
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnOccupied))]
        [HarmonyPostfix]
        public static void OnOccupied(Projectile __instance)
        {
            /*Plugin.logger.LogInfo("My projectile launched with " + __instance.lastPos.ToString());
            CustomEnergyModProjectile myProjectile = __instance.TryCast<CustomEnergyModProjectile>();

            if (myProjectile != null)
            {
                Plugin.logger.LogInfo("My projectile is not null");
                myProjectile.OnOccupied();
            }
            else
            {
                Plugin.logger.LogError("My projectile is null");
            }*/
        }

        [HarmonyPatch(typeof(EntityCommandBuffer), nameof(EntityCommandBuffer.Instantiate))]
        [HarmonyPrefix]
        public static unsafe void Thing(EntityCommandBuffer __instance)
        {
            Plugin.logger.LogInfo("Entity command buffer created and m_data being null is " + (__instance.m_Data==null));
        }
    }
}