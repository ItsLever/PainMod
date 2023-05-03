using HarmonyLib;
using Unity.Entities;

namespace PainMod;
[HarmonyPatch]
public static class SystemsStarterPatch
{
    public static bool isInit = false;
    [HarmonyPatch(typeof(SceneHandler), nameof(SceneHandler.Start))]
    [HarmonyPostfix]
    public static void OnWorldLoad()
    {
        World world = Manager.ecs.ServerWorld;
        if (world != null)
        {
            Plugin.logger.LogInfo("Starting all systems!");
            FirstSystemTest.instance.SetWorld(world);
            //CustomStateSystem.instance.SetWorld(world);
            isInit = true;
        }
    }

    [HarmonyPatch(typeof(RadicalPauseMenuOption_ExitToTitle), nameof(RadicalPauseMenuOption_ExitToTitle.OnActivated))]
    [HarmonyPostfix]
    public static void OnLeaveServerWorld()
    {
        FirstSystemTest.instance.RemoveWorld();
        //CustomStateSystem.instance.RemoveWorld();
    }
}