using HarmonyLib;

namespace PainMod;
[HarmonyPatch]
internal class PreMemManage
{
    [HarmonyPatch(typeof(MemoryManager), nameof(MemoryManager.Init))]
    [HarmonyPrefix]
    public static void ImRunningOutOfNames(MemoryManager __instance)
    {
        foreach (var pool in __instance.poolablePrefabBank.poolInitializers)
        {
           // pool.prefab = 
        }
        //__instance.poolablePrefabBank.poolInitializers
    }
}