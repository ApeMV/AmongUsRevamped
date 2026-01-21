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

        if (Options.NoGameEnd.GetBool() || Options.Gamemode.GetValue() == 3) return false;

        var allPlayers = PlayerControl.AllPlayerControls.ToArray();

        if (!ImpCheckComplete)
        {
            imps.AddRange(allPlayers.Where(pc => pc.Data.Role.IsImpostor));
            ImpCheckComplete = true;
        }

        if (Utils.AliveImpostors == 0) 
        {
            LastWinReason = $"★ Crewmates win!\n\nImpostors:\n" + string.Join("\n", imps.Select(p => p.Data.PlayerName));
        }
        else if (Utils.AliveImpostors >= Utils.AliveCrewmates) 
        {
            LastWinReason = $"★ Impostors win!\n\nImpostors:\n★" + string.Join("\n★", imps.Select(p => p.Data.PlayerName));
        }
        else if (GameData.Instance != null && GameData.Instance.TotalTasks > 0 && GameData.Instance.CompletedTasks >= GameData.Instance.TotalTasks)
        {
            LastWinReason = $"★ Crewmates win! (Tasks)\n\nImpostors:\n" + string.Join("\n", imps.Select(p => p.Data.PlayerName));
        }
        else if (Options.Gamemode.GetValue() < 2)
        {
            LastWinReason = $"★ Impostors win! (Sabotage)\n\nImpostors:\n★" + string.Join("\n★", imps.Select(p => p.Data.PlayerName));
        }
        return true;
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