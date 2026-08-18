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

        if (Options.DisableAnnoyingMeetingCalls.GetBool() && !Utils.CanCallMeetings && target == null && Options.Gamemode.GetValue() < 1)
        {
            Logger.Info($" {__instance.Data.PlayerName} is calling a meeting too fast, attempt blocked", "ReportDeadBodyPatch");
            return false;
        }

        // target == null means meeting

        if (Options.Gamemode.GetValue() == 1 || Options.Gamemode.GetValue() == 2 || Options.Gamemode.GetValue() == 3)
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

    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags, ref bool __state)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        byte playerId = __instance.Data.PlayerId;

        if (!killCount.ContainsKey(playerId))
        {
            killCount[playerId] = 0;
        }

        if (resultFlags.HasFlag(MurderResultFlags.Succeeded))
        {
            killCount[playerId]++;
            Logger.Info($" {__instance.Data.PlayerName} killed {target.Data.PlayerName}", "MurderPlayer");
            if (string.IsNullOrEmpty(NormalGameEndChecker.firstDeath)) 
            {
                NormalGameEndChecker.firstDeath = target.Data.PlayerName;
                Utils.HasMurdered = true;
            }

            if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek)
            {
                if (target.Data.PlayerId == __instance.shapeshiftTargetPlayerId) return;
                Logger.Info($" {__instance.Data.PlayerName} directly killed {target.Data.PlayerName} in SnS, forcing suicide. (Are they hacking?)", "MurderPlayer");
                __instance.RpcSetRole(RoleTypes.ImpostorGhost); 
            }
        }

        if ((target == PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data.IsDead) && !Main.DisableInfoWhenDead.Value && resultFlags.HasFlag(MurderResultFlags.Succeeded))
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data.Role.IsImpostor)
                {
                    p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=red><size=90%>({killCount[p.Data.PlayerId]}†)</color> - {Utils.StoredRoleText[p.PlayerId]}";
                }
                else
                {
                    p.cosmetics.nameText.text = $"{p.Data.PlayerName}<color=green><size=90%>({PlayerControlCompleteTaskPatch.playerTasksCompleted[p.PlayerId]}/{PlayerControlCompleteTaskPatch.tasksPerPlayer[p.PlayerId]})</color> - {Utils.StoredRoleText[p.PlayerId]}";
                }
            }
        }

        //1 = Shift and Seek
        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek)
        {
            if (target.Data.PlayerId == __instance.shapeshiftTargetPlayerId)
            {
                killCount[playerId]++;
                Logger.Info($" {__instance.Data.PlayerName} correctly killed {target.Data.PlayerName} ", "SNSKillManager");
                target.RpcSetRole(RoleTypes.CrewmateGhost);
                if (string.IsNullOrEmpty(NormalGameEndChecker.firstDeath))
                {
                    NormalGameEndChecker.firstDeath = target.Data.PlayerName;
                    Utils.HasMurdered = true;
                }
            }
            else
            {
                Logger.Info($" {__instance.Data.PlayerName} misfired trying to kill {target.Data.PlayerName}. Blocking kill", "SNSKillManager");
            }
        }
    }
}

/*
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
internal static class CheckShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        // Canceling a Shapeshift freezes the player until they successfully Shapeshift again. Unavoidable game logic.
        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek && __instance.isNew)
        {
            Logger.Info($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.", "SNSShapeshiftManager");
            Logger.SendInGame($" {__instance.Data.PlayerName} shapeshifted during misfire cooldown, making the game temporarily freeze them.");
            return false;
        }
        else return true;
    }
}
*/

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

            if (!playerTasksCompleted.ContainsKey(p.PlayerId))
            {
                playerTasksCompleted[p.PlayerId] = 0;                
            }
            tasksPerPlayer[p.PlayerId] = p.Data.Tasks.Count;
        }

        if (__instance.Data.RoleType == RoleTypes.Impostor ||
            __instance.Data.RoleType == RoleTypes.Shapeshifter ||
            __instance.Data.RoleType == RoleTypes.Phantom ||
            __instance.Data.RoleType == RoleTypes.Viper)
        {
            EACR.TaskCheat(__instance);
        }

        playerTasksCompleted[__instance.PlayerId]++;

        if (playerTasksCompleted[__instance.PlayerId] > __instance.Data.Tasks.Count)
        {
            EACR.TaskCheat(__instance);
        }

        Logger.Info($" {__instance.Data.PlayerName} completed {idx}", "TaskPatch");

        if (Options.Gamemode.GetValue() != 2) CalculateTaskWin();

        if (Options.Gamemode.GetValue() == 2)
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
            nameText.text = $"{__instance.Data.PlayerName}<color=green><size=90%>({playerTasksCompleted[__instance.PlayerId]}/{tasksPerPlayer[__instance.PlayerId]})</color> - {Utils.StoredRoleText[__instance.PlayerId]}";
        }
    }
    
    public static void CalculateTaskWin()
    {
        if (!Utils.GamePastRoleSelection || Utils.isHideNSeek || Options.NoGameEnd.GetBool() || !OnGameStartPatch.PastStartScreen) return;

        //Logger.Info($" Checking if {GameData.Instance.CompletedTasks} - {ignoredCompletedTasks} >= ({GameData.Instance.TotalTasks} - {ignoredTasks}) * 0.01 * {Options.TaskPercentNeededToWin.GetInt()}", "TaskPatch");

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

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.PlayAnimation))]
public static class PlayAnimationPatch
{
    public static void Prefix(PlayerControl __instance, byte animType)
    {
        if (!AmongUsClient.Instance.AmHost || __instance == PlayerControl.LocalPlayer) return;

        var task = (TaskTypes)animType;

        switch (task)
        {
            case TaskTypes.PrimeShields:
            case TaskTypes.ClearAsteroids:
            case TaskTypes.EmptyGarbage:
            if (__instance.Data.RoleType == RoleTypes.Impostor ||
                __instance.Data.RoleType == RoleTypes.Shapeshifter ||
                __instance.Data.RoleType == RoleTypes.Phantom ||
                __instance.Data.RoleType == RoleTypes.Viper)
            {
                EACR.PlayAnimationCheat(__instance);
                return;
            }
            if (!GameManager.Instance.LogicOptions.GetVisualTasks())
            {
                {
                    EACR.PlayAnimationCheat(__instance);
                    return;
                }
            }
            break;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetScanner))]
public static class SetScannerPatch
{
    public static void Prefix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost || __instance == PlayerControl.LocalPlayer) return;

        if (__instance.Data.RoleType == RoleTypes.Impostor ||
            __instance.Data.RoleType == RoleTypes.Shapeshifter ||
            __instance.Data.RoleType == RoleTypes.Phantom ||
            __instance.Data.RoleType == RoleTypes.Viper)
        {
            EACR.PlayAnimationCheat(__instance);
            return;
        }

        if (!GameManager.Instance.LogicOptions.GetVisualTasks() && __instance != PlayerControl.LocalPlayer)
        {
            {
                EACR.PlayAnimationCheat(__instance);
                return;
            }
        }
    }
}
