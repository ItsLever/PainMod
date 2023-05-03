using System;
using CoreLib.Submodules.ChatCommands;
using UnityEngine;

namespace PainMod;

public class ChallengeCommandHandler : IChatCommandHandler
{
    public CommandOutput Execute(string[] parameters)
    {
        string res = "";
        string finalText;
        string activeText = "";
        string inactiveText = "";
        bool becameActive = false;
        if (parameters.Length != 1)
            return new CommandOutput("Please type in the challenge you would like to activate. \n List is: \n minesweeper\n GhormKeeper\n AfraidOfTheDark", Color.yellow);
        if (parameters[0].Equals("minesweeper", StringComparison.OrdinalIgnoreCase))
        {
            res = "minesweeper";
            Plugin.mineChallengeActive = !Plugin.mineChallengeActive;
            if (Plugin.mineChallengeActive)
                becameActive = true;
            else
                becameActive = false;
        }
        else if (parameters[0].Equals("GhormKeeper", StringComparison.OrdinalIgnoreCase))
        {
            res = "Ghorm Keeper";
            Plugin.ghormChallengeActive = !Plugin.ghormChallengeActive;
            if (Plugin.ghormChallengeActive)
                becameActive = true;
            else
                becameActive = false;
        }
        else if (parameters[0].Equals("AfraidOfTheDark", StringComparison.OrdinalIgnoreCase))
        {
            res = "Afraid of the dark";
            Plugin.darkChallengeActive = !Plugin.darkChallengeActive;
            if (Plugin.darkChallengeActive)
                becameActive = true;
            else
                becameActive = false;
        }
        else
        {
            return new CommandOutput(
                "Please type in the challenge you would like to activate. \n List is: \n minesweeper", Color.red);
        }
        
        activeText = "Good luck m8, ur gonna need it if you want to suceed with the challenge: " + res;
        inactiveText = "I see, " + res + " is quite a hard challenge, sad to see you werent up for it though, maybe next time?";
        if (becameActive)
            finalText = activeText;
        else
            finalText = inactiveText;
        return (finalText);
    }

    public string GetDescription()
    {
        return "Activate challenges with /challenge (challenge name)";
    }

    public string[] GetTriggerNames()
    {
        return new[] {"challenge"};
    }
    
    public string GetModName()
    {
        return "PainMod";
    }
}