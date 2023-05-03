using HarmonyLib;
using PlayerCommand;

namespace PainMod;
[HarmonyPatch]
internal class EnemyUpgrade
{
    public static bool wallIsLowered = false;
    public static bool timeToWaitDone = false;
    [HarmonyPatch(typeof(ClientSystem),nameof(ClientSystem.LowerTheGreatWall))]
    [HarmonyPostfix]
    public static void onLowerWall(ClientSystem __instance)
    {
        Plugin.logger.LogInfo("wall is now lowered");
        wallIsLowered = true;
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.PlayerInit))]
    [HarmonyPostfix]
    public static void onStartCheckLowerWall(PlayerController __instance)
    {
        Plugin.logger.LogInfo("Player created: " + __instance.playerName);
        wallIsLowered = true;
    }
}