using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes;

namespace AmongUsRevamped;

[HarmonyPatch]
class ExileControllerWrapUpPatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(ExileController), nameof(ExileController.WrapUp));
    }

    public static void Postfix(ExileController __instance)
    {
        AfterExile(__instance.initData.networkedPlayer);
    }

    public static void AfterExile(NetworkedPlayerInfo ejectedPlayer)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (ejectedPlayer == null) return;

        PlayerControl pc = null;
        PlayerControl exiledPlayer = ejectedPlayer.Object;
        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p.PlayerId == ejectedPlayer.PlayerId)
            {
                pc = p;
            }

            if (ejectedPlayer == PlayerControl.LocalPlayer.Data && !Main.DisableInfoWhenDead.Value)
            {
                if (p.Data.Role.IsImpostor)
                {
                    p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=red><size=90%>({MurderPlayerPatch.killCount[p.Data.PlayerId]}† - {Utils.StoredRoleText[p.PlayerId]})";
                }
                else
                {
                    if (!PlayerControlCompleteTaskPatch.playerTasksCompleted.ContainsKey(p.PlayerId))
                    {
                        PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId] = 0;                
                    }
                    p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=green><size=90%>({PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId]}/{PlayerControlCompleteTaskPatch.tasksPerPlayer[p.PlayerId]} - {Utils.StoredRoleText[p.PlayerId]})";
                }
            }
        }

        Logger.Info($" {ejectedPlayer.PlayerName} was ejected", "ExileController");

        if (PlayerControlSetRolePatch.Jesters.Contains(exiledPlayer))
        {
            Logger.Info($"Jester {ejectedPlayer.PlayerName} wins", "ExilePatch");
            Utils.CustomWinnerEndGame(exiledPlayer, 1);
            NormalGameEndChecker.CheckWinnerText("Jester");
            return;
        }
    }

    [HarmonyPatch]
    class AirshipExileControllerPatch
    {
        public static MethodBase TargetMethod()
        {
        return Utils.GetStateMachineMoveNext<AirshipExileController>(nameof(AirshipExileController.WrapUpAndSpawn));
        }

        public static void Postfix(AirshipExileController._WrapUpAndSpawn_d__11 __instance)
        {
            ExileControllerWrapUpPatch.AfterExile(__instance.__4__this.initData.networkedPlayer);
        }
    }
}