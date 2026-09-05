using AmongUs.Data;
using Hazel;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
public static class EndGameManagerPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        Logger.Info(" -------- GAME ENDED --------", "EndGame");

        Utils.ClearLeftoverData();
        OptionManager.RestoreOptions();

        EndGameNavigation navigation = __instance.Navigation;
        if (!AmongUsClient.Instance.AmHost || __instance == null || navigation == null || !Options.AutoRejoinLobby.GetBool()) return;
        navigation.NextGame();
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class NormalGameEndChecker
{
    public static bool ImpCheckComplete;
    public static string LastWinReason = "";
    public static List<PlayerControl> imps = new List<PlayerControl>();
    public static string firstDeath;
    public static string firstDeathString => firstDeath != "" ? $"\n\nFirst death: {firstDeath}" : "";

    public static bool Prefix()
    {
        if (Options.NoGameEnd.GetBool() || Options.Gamemode.GetValue() == 2 || Utils.HandlingGameEnd) return false;

        var allPlayers = PlayerControl.AllPlayerControls.ToArray();

        if (!ImpCheckComplete)
        {
            imps.AddRange(allPlayers.Where(pc => pc.Data.Role.IsImpostor));
            ImpCheckComplete = true;
        }

        CheckWinnerText("");

        return true;
    }
    public static bool canUpdateWinnerText;
    public static string customRoles { get; set; }
    public static void CheckWinnerText(string Winner)
    {
        string impostorList = string.Join(", ", imps.Select(p => p.Data.PlayerName));
        string impostorString = impostorList != "" ? $"\n\n Impostors: {impostorList}" : "";
        
        if (!canUpdateWinnerText) return;

        if (Winner == "SnSTimer")
        {
            LastWinReason = $"Crewmates Win! (Survived {Options.CrewAutoWinsGameAfter.GetInt()}s)" + impostorString + firstDeathString;
            canUpdateWinnerText = false;
            return;
        }

        if (Winner == "PnSTimer")
        {
            LastWinReason = $"Crewmates Win! (Survived {Options.CrewAutoWinsGameAfter.GetInt()}s)" + impostorString + firstDeathString;
            canUpdateWinnerText = false;
            return;
        }

        if (Winner == "NoOneWinsSpeedrun")
        {
            LastWinReason = $"No one wins. ({Options.GameAutoEndsAfter.GetInt()}s Timer)";
            canUpdateWinnerText = false;
            return;
        }

        if ((Options.Gamemode.GetValue() != 2 && Utils.AliveImpostors == 0) || Winner == "Crewmate") 
        {
            LastWinReason = "Crewmates Win!" + impostorString + firstDeathString;
            canUpdateWinnerText = false;
        }
        else if ((Options.Gamemode.GetValue() != 2 && Utils.AliveImpostors >= Utils.AliveCrewmates) || Winner == "Impostor") 
        {
            LastWinReason = "Impostors Win!" + impostorString + firstDeathString;
            canUpdateWinnerText = false;
        }
        else if (GameData.Instance != null && GameData.Instance.TotalTasks > 0 && GameData.Instance.CompletedTasks >= GameData.Instance.TotalTasks || Winner == "CrewmateTasks")
        {
            LastWinReason = $"Crewmates Win! ({Options.TaskPercentNeededToWin.GetInt()}% tasks completed)" + impostorString + firstDeathString;
            canUpdateWinnerText = false;
        }
        else if (Options.Gamemode.GetValue() < 1 || Winner == "ImpostorSabotage")
        {
            LastWinReason = "Impostors Win! (Sabotage)" + impostorString + firstDeathString;
        }
    }
}

[HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
class HNSGameEndChecker
{
    public static bool Prefix()
    {
        if (Options.NoGameEnd.GetBool()) return false;
        else return true;
    }
}