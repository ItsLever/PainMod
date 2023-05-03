using System;
using CoreLib;
using CoreLib.Util;
using UnityEngine;

namespace PainMod
{
    public class MonoStuff : MonoBehaviour
    {
        public MonoStuff(IntPtr ptr) : base(ptr) { }
        public PlayerController myPlayer;
        public bool isInit = false;

        public void Update()
        {
            if (CoreLib.State.IsInGame())
            {
                if (CoreLib.Players.GetAllPlayers().Count != 0)
                {
                    myPlayer = CoreLib.Players.GetCurrentPlayer();
                    CoreLib.Players.GetCurrentPlayer().SetInvincibility(true);
                    if (!isInit)
                    {
                        myPlayer.playerInventoryHandler.CreateItem(0, ObjectID.OctarineMiningPick, 1,
                            myPlayer.WorldPosition, 0);
                        myPlayer.playerInventoryHandler.CreateItem(0, ObjectID.HiveMotherScanner, 1,
                            myPlayer.WorldPosition, 0);
                        isInit = true;
                    }
                }
                CoreLib.GameManagers.GetMainManager()._menuManager.creditsMenu.title.textString = "Lever is gaymer";
                CoreLib.GameManagers.GetMainManager()._menuManager.creditsMenu.title.color = Color.Lerp(Color.red, Color.blue, 1);
                CoreLib.GameManagers.GetMainManager().player.UpdateGreatWall();
                CoreLib.GameManagers.GetMainManager().player.playerCanInteractWithGreatWall = true;
                //CoreLib.GameManagers.GetMainManager().player.greatWallHasBeenLowered = true;
                if (!CoreLib.GameManagers.GetMainManager().player.greatWallHasBeenLowered)
                {
                    TheGreatWallSystem.LowerGreatWall();
                }
            }
        }
    }
}