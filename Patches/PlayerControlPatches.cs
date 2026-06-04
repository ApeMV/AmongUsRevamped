using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost || __instance == null) return true;
        if (Utils.isHideNSeek) return false;

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.CanCallMeetings && target == null && Options.Gamemode.GetValue() < 2)
        {
            Logger.Info($" {__instance.Data.PlayerName} is calling a meeting too fast, attempt blocked", "ReportDeadBodyPatch");
            return false;
        }

        // target == null means meeting

        if (Options.Gamemode.GetValue() == 2 || Options.Gamemode.GetValue() == 3)
        {
            if (target != null)
            {
                Logger.Info($" Stopped {__instance.Data.PlayerName} reporting the body of {target.PlayerName}", "ReportDeadBodyPatch");
                return false;
            }
            if (__instance != PlayerControl.LocalPlayer)
            {
                Logger.Info($" Stopped {__instance.Data.PlayerName} trying to call a meeting", "ReportDeadBodyPatch");
                return false;
            }
            return true;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MurderPlayerPatch
{
    public static readonly Dictionary<byte, int> misfireCount = new();
    public static readonly Dictionary<byte, int> killCount = new();

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags, ref bool __state)
    {
        if (!AmongUsClient.Instance.AmHost || !resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;

        byte playerId = __instance.Data.PlayerId;

        if (!killCount.ContainsKey(playerId))
        {
            killCount[playerId] = 0;
        }

        if (!__instance.isNew)
        {
            killCount[playerId]++;
            Logger.Info($" {__instance.Data.PlayerName} killed {target.Data.PlayerName}", "MurderPlayer");

            if ((target == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead) && !Main.DisableInfoWhenDead.Value)
            {
                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p.Data.Role.IsImpostor)
                    {
                        p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=red><size=90%>({killCount[p.Data.PlayerId]}†)</color> - {Utils.GetRoleText(p)}";
                    }
                    else
                    {
                        p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=green><size=90%>({PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId]}/{PlayerControlCompleteTaskPatch.tasksPerPlayer[p.PlayerId]})</color> - {Utils.GetRoleText(p)}";
                    }
                }
            }
        }

        if (Options.Gamemode.GetValue() < 2 && AbilityManagement.IsJuggernaut(__instance) && killCount[__instance.PlayerId] >= Options.KillsNeededForJuggernaut.GetInt())
        {
            Logger.Info("Juggernaut win", "MurderPlayer");
            Utils.ContinueEndGame((byte)GameOverReason.ImpostorsByVote);
            NormalGameEndChecker.CheckWinnerText("Juggernaut");
        }

        //2 = Shift and Seek
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (!resultFlags.HasFlag(MurderResultFlags.Succeeded))
            return;

            if (target.Data.PlayerId == __instance.shapeshiftTargetPlayerId)
            {
                Logger.Info($" {__instance.Data.PlayerName} correctly killed {target.Data.PlayerName} ", "SNSKillManager");
            }
            else
            {
                if (!misfireCount.ContainsKey(playerId))
                misfireCount[playerId] = 0;

                misfireCount[playerId]++;

                if (misfireCount[__instance.Data.PlayerId] < Options.MisfiresToSuicide.GetFloat())
                {
                    __instance.RpcSetRole(RoleTypes.Crewmate);
                    __instance.isNew = true;
                    Logger.Info($" {__instance.Data.PlayerName} killed {target.Data.PlayerName} incorrectly and can't kill for {Options.CantKillTime.GetInt()}s", "SNSKillManager");
                    Logger.SendInGame($" {__instance.Data.PlayerName} killed {target.Data.PlayerName} incorrectly and can't kill for {Options.CantKillTime.GetInt()}s");

                    new LateTask(() =>
                    {
                        __instance.isNew = false;
                        if (!__instance.Data.IsDead) {__instance.RpcSetRole(RoleTypes.Shapeshifter, false);}
                    }, Options.CantKillTime.GetInt(), "SNSResetRole");
                }

                if (misfireCount[__instance.Data.PlayerId] >= Options.MisfiresToSuicide.GetFloat())
                {
                    __instance.RpcSetRole(RoleTypes.ImpostorGhost);
                    Logger.Info($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} time(s) and suicided", "SNSKillManager");
                    Logger.SendInGame($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} time(s) and suicided");
                }

            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
internal static class CheckShapeshiftPatch
{

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        // Canceling a Shapeshift freezes the player until they successfully Shapeshift again. Unavoidable game logic.
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && __instance.isNew)
        {
            Logger.Info($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.", "SNSShapeshiftManager");
            Logger.SendInGame($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.");
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class PlayerControlCompleteTaskPatch
{
    public static Dictionary<byte, int> playerTasksCompleted = new Dictionary<byte, int>();
    public static Dictionary<byte, int> tasksPerPlayer = new Dictionary<byte, int>();
    public static List<string> ignoredRoles = new List<string> {"Jester"};
    public static int ignoredTasks;
    public static int ignoredCompletedTasks;
    public static bool tasksInitiated;

    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (AbilityManagement.IsJester(p) && !tasksInitiated)
            {
                ignoredTasks += p.Data.Tasks.Count;
                tasksInitiated = true;
            }

            if (!playerTasksCompleted.ContainsKey(p.PlayerId))
            {
                playerTasksCompleted[p.PlayerId] = 0;                
            }
            tasksPerPlayer[p.PlayerId] = p.Data.Tasks.Count;
        }

        playerTasksCompleted[__instance.PlayerId]++;

        if (AbilityManagement.IsJester(__instance))
        {
            ignoredCompletedTasks = playerTasksCompleted[__instance.PlayerId];
        }

        Logger.Info($" {__instance.Data.PlayerName} completed {idx}", "TaskPatch");

        if (Options.Gamemode.GetValue() < 2)
        {
            if (AbilityManagement.IsSpeedrunner(__instance) && !__instance.Data.IsDead && playerTasksCompleted[__instance.PlayerId] >= __instance.Data.Tasks.Count)
            {
                Logger.Info($"Speedrunner {__instance.Data.PlayerName} won", "TaskPatch");
                Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);
                NormalGameEndChecker.CheckWinnerText("Speedrunner");
            }
        }

        if (Options.Gamemode.GetValue() != 3) CalculateTaskWin();

        if (Options.Gamemode.GetValue() == 3)
        {
            if (!__instance.Data.IsDead && playerTasksCompleted[__instance.PlayerId] >= __instance.Data.Tasks.Count)
            {
                Utils.CustomWinnerEndGame(__instance, 1);
                NormalGameEndChecker.LastWinReason = $"{__instance.Data.PlayerName} wins! (Completed tasks)";
                NormalGameEndChecker.canUpdateWinnerText = false;
            }
        }

        if (PlayerControl.LocalPlayer.Data.IsDead && !Main.DisableInfoWhenDead.Value)
        {
            TMP_Text nameText = __instance.cosmetics.nameText;
            nameText.text = $"{__instance.Data.PlayerName}<color=green><size=90%>({playerTasksCompleted[__instance.PlayerId]}/{tasksPerPlayer[__instance.PlayerId]})</color> - {Utils.GetRoleText(__instance)}";
        }
    }
    
    public static void CalculateTaskWin()
    {
        if (!Utils.GamePastRoleSelection || Utils.isHideNSeek || Options.NoGameEnd.GetBool() || !CoShowIntroPatch.IntroInitiated) return;

        Logger.Info($" Checking if {GameData.Instance.CompletedTasks} - {ignoredCompletedTasks} >= ({GameData.Instance.TotalTasks} - {ignoredTasks}) * 0.01 * {Options.TaskPercentNeededToWin.GetInt()}", "TaskPatch");

        if ((GameData.Instance.CompletedTasks - ignoredCompletedTasks) >= (GameData.Instance.TotalTasks - ignoredTasks)*0.01*Options.TaskPercentNeededToWin.GetInt())
        {
            Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);
            NormalGameEndChecker.CheckWinnerText("CrewmateTasks");
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckSporeTrigger))]
public static class PlayerControlCheckSporeTriggerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] Mushroom mushroom)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (Options.DisableSporeTrigger.GetBool()) return false;
        else return true;
    }
}
