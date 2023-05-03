using HarmonyLib;
using UnityEngine;

namespace PainMod;
[HarmonyPatch]
internal class EmotePatcher
{
    [HarmonyPatch(typeof(Emote), nameof(Emote.OnOccupied))]
    [HarmonyPrefix]
    public static bool OnEmoteStart(Emote __instance)
    {
        if (__instance.emoteTypeInput == Emote.EmoteType.__illegal__)
            return false;
        switch (__instance.emoteTypeInput)
        {
            case (Emote.EmoteType)500:
                int num = UnityEngine.Random.Range(0, Constants.DarknessWarnEmotes.Length);
                __instance.textToPrint = Constants.DarknessWarnEmotes[num];
                DoEndingStuff(__instance);
                return false;
        }
        return true;
    }

    private static void DoEndingStuff(Emote __instance)
    {
        __instance.text.Render(__instance.textToPrint, true);
        __instance.textOutline.Render(__instance.textToPrint, true);
        __instance.textOutline.SetTempColor(Color.black);
    }
}