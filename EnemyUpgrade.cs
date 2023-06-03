using HarmonyLib;
using PlayerCommand;
using UnityEngine;

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
        ChatWindow_Patch.SendMessage("The world grows harder...", Color.red); //Chat Notification
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

//Stuff for Chat Message
[HarmonyPatch]
public class ChatWindow_Patch
{
    private static ChatWindow _chat;

    [HarmonyPatch(typeof(ChatWindow), "Awake")]
    [HarmonyPostfix]
    public static void OnCreate(ChatWindow __instance)
    {
        _chat = __instance;
    }

    public static void SendMessage(string message, Color color)
    {
        PugTextEffectMaxFade fadeEffect;
        PugText text = _chat.AllocPugText(ChatWindow.MessageTextType.Sent, out fadeEffect);
        text.Render(message);
        text.style.color = color;
        text.defaultStyle.color = color;
        text.color = color;
        if (!(fadeEffect != null))
            return;
        fadeEffect.FadeOut();
        _chat.AddPugText(ChatWindow.MessageTextType.Sent, text);
    }
}