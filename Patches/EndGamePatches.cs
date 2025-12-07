using AmongUs.Data;
using Hazel;
using InnerNet;
using UnityEngine;

namespace HNSRevamped;

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.ShowButtons))]
public static class EndGameManagerPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        EndGameNavigation navigation = __instance.Navigation;
        if (!AmongUsClient.Instance.AmHost || __instance == null || navigation == null || !Options.AutoRejoinLobby.GetBool()) return;
        navigation.NextGame();
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class NormalGameEndChecker
{
    public static bool Prefix()
    {
        if (Options.NoGameEnd.GetBool()) return false;
        else return true;
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
[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class ControllerManagerUpdatePatch
{
    public static void Postfix()
    {
        if (Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Return))
        {
            // Instead of a gigantic custom RPC, the game can just be ended in 3 lines of code
            MessageWriter writer = AmongUsClient.Instance.StartEndGame();
            writer.Write((byte)GameOverReason.ImpostorDisconnect);
            AmongUsClient.Instance.FinishEndGame(writer);
        }
    }
}