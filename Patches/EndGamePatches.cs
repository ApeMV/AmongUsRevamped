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

    public static bool Prefix()
    {

        if (Options.NoGameEnd.GetBool() || Options.Gamemode.GetValue() == 3 || Utils.HandlingGameEnd) return false;

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
        var impostorList = string.Join(", ", imps.Select(p => p.Data.PlayerName));
        
        if (!canUpdateWinnerText) return;
        
        if (Winner == "Jester")
        {
            LastWinReason = $"Jester wins! (Voted)\n\nImpostors: {impostorList}";
            canUpdateWinnerText = false;
            return;
        }

        if (Winner == "Juggernaut")
        {
            LastWinReason = $"Impostors win! (Juggernaut killed {Options.KillsNeededForJuggernaut.GetInt()} players)\n\nImpostor: {impostorList}";
            canUpdateWinnerText = false;
            return;
        }

        if (Winner == "Speedrunner")
        {
            LastWinReason = $"Crewmates win! (Speedrunner finished tasks)\n\nImpostor: {impostorList}";
            canUpdateWinnerText = false;
            return;
        }

        if (Utils.AliveImpostors == 0 || Winner == "Crewmate") 
        {
            LastWinReason = $"Crewmates win!\n\nImpostors: {impostorList}";
            canUpdateWinnerText = false;
        }
        else if (Utils.AliveImpostors >= Utils.AliveCrewmates || Winner == "Impostor") 
        {
            LastWinReason = $"Impostors win!\n\nImpostor: {impostorList}";
            canUpdateWinnerText = false;
        }
        else if (GameData.Instance != null && GameData.Instance.TotalTasks > 0 && GameData.Instance.CompletedTasks >= GameData.Instance.TotalTasks || Winner == "CrewmateTasks")
        {
            LastWinReason = $"Crewmates win! ({Options.TaskPercentNeededToWin.GetInt()}% Tasks)\n\nImpostors: {impostorList}";
            canUpdateWinnerText = false;
        }
        else if (Options.Gamemode.GetValue() < 2 || Winner == "ImpostorSabotage")
        {
            LastWinReason = $"Impostors win! (Sabotage)\n\nImpostors: {impostorList}" + (string.IsNullOrEmpty(customRoles) ? "" : "\n\n" + customRoles);
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