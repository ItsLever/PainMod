using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using PugTilemap;
//using UnhollowerBaseLib;
//using UnhollowerRuntimeLib;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Playables;
namespace PainMod;
[HarmonyPatch]
internal class MusicManager
{
    public static bool hasSetIn = false;
    public static string testing = "testing";
    [HarmonyPatch(typeof(global::MusicManager), nameof(global::MusicManager.Update))]
    [HarmonyPostfix]
    public static void Yes(global::MusicManager __instance)
    {
        hasSetIn = true;
        if (!hasSetIn)
        {
            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            /*if(pluginFolder!=null)
                Plugin.logger.LogInfo(pluginFolder.ToString() + " yes!");
            else
                Plugin.logger.LogInfo("pluginfolder is null!"); */
            AssetBundle myTrack = AssetBundle.LoadFromFile($"{pluginFolder}/testing");
            if (myTrack != null)
            {
                Plugin.logger.LogInfo("my track is not null");
                    var prefabList = myTrack.LoadAsset<AudioClip>("bossMusicCandidate2");
                    
                    
              //      foreach (var track in __instance.)
                    {
                   //     track.track = GameObject.Instantiate(prefabList);
                 //       Plugin.logger.LogInfo("AUDIO SOURCE IS IN AT " + track.track.name + "with a duration of " + track.track.length);
                    }
            }
            else
            {
                Plugin.logger.LogInfo("everything is null fuck");
            }
        }

        hasSetIn = true;
    }

}