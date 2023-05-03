using CoreLib;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using PugTilemap;
using UnityEngine;
using Color = System.Drawing.Color;

namespace PainMod;

[HarmonyPatch]
internal class OnAwakePatch
{
    [HarmonyPatch(typeof(MapUI), nameof(MapUI.Awake))]
    [HarmonyPostfix]
    public static void MapUIStart(MapUI __instance)
    {
     //   UIManager m = GameManagers.GetManager<UIManager>();
       // MapUI e = m.mapUI;
       //MapUI e = __instance;
       TileTypeColorTable.TileColor tileColor1 = new TileTypeColorTable.TileColor
       {
           color = new Color32( 160, 8, 122, 255),
           tileType = TileType.wall
       };
        TileTypeColorTable.TileSetColors tileSetColors = new TileTypeColorTable.TileSetColors();
        //NRE issue?
        tileSetColors.tileColors = new List<TileTypeColorTable.TileColor>();
        tileSetColors.tileColors.Add(tileColor1);
        tileSetColors.pugMapTileset = Plugin.brick1;
        __instance.colorTable.tileSetColors.Add(tileSetColors);
       /* PugColor32 pugColor32 = new PugColor32();
        pugColor32.r = 160;
        pugColor32.g = 8;
        pugColor32.b = 122;
        pugColor32.a = 255;
        TileInfo info = new TileInfo();
        info.state = 0;
        info.tileset = 100;
        info.tileType = TileType.wall;
        __instance.colorTable.colorToTileDict.Add(pugColor32, info);*/
    }
}