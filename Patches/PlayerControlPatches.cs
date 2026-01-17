using AmongUs.Data;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    public static readonly HashSet<byte> ProcessedModerators = new HashSet<byte>();

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255) return;
        
        if (Utils.IsPlayerModerator(__instance.Data.FriendCode) && !ProcessedModerators.Contains(__instance.PlayerId))
        {
            __instance.SetName($"★{__instance.Data.PlayerName}★");
            ProcessedModerators.Add(__instance.PlayerId);
        }

        GameObject g = GameObject.Find("GameSettingsLabel");

        // 0Kc
        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek && Main.NormalOptions.KillCooldown != 0.01f)
        {
            Main.NormalOptions.KillCooldown = 0.01f;

            if (Options.NoKcdSettingsOverride.GetBool() && g == null)
            {
                Main.NormalOptions.EmergencyCooldown = 0;

                Main.NormalOptions.TaskBarMode = 0;
            }
        }

        // SnS
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && Main.NormalOptions.KillCooldown != 2.5f)
        {
            Main.NormalOptions.KillCooldown = 2.5f;

            if (Options.SNSSettingsOverride.GetBool() && g == null)
            {
                Main.NormalOptions.NumEmergencyMeetings = 0;

                Main.NormalOptions.TaskBarMode = 0;
            }
        }

        // Speedrun
        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek && g == null)
        {
            Main.NormalOptions.NumEmergencyMeetings = 0;

            Main.NormalOptions.TaskBarMode = 0;

            if (__instance.AllTasksCompleted() && Utils.InGame && Utils.GamePastRoleSelection && !Utils.HandlingGameEnd)
            {
                Utils.HandlingGameEnd = true;
                __instance.RpcSetRole(AmongUs.GameOptions.RoleTypes.ImpostorGhost, false);

                new LateTask(() =>
                {
                    NormalGameEndChecker.LastWinReason = $"★ {__instance.Data.PlayerName} Wins! (Completed tasks first)";
                    MessageWriter writer = AmongUsClient.Instance.StartEndGame();
                    writer.Write((byte)GameOverReason.ImpostorsByVote);
                    AmongUsClient.Instance.FinishEndGame(writer);
                }, 1f, "SpeedrunSetWinner");
            }
        }

        if (Options.Gamemode.GetValue() == 0 && Main.NormalOptions.KillCooldown <= 0.01f)
        {
            Main.NormalOptions.KillCooldown = 25f;
        }

        if (AmongUsClient.Instance.AmHost)
        {
            if (__instance.Data.PlayerLevel != 0 && __instance.Data.PlayerLevel < Options.KickLowLevelPlayer.GetInt() && __instance.Data.ClientId != AmongUsClient.Instance.HostId)
            {
                if (!Options.TempBanLowLevelPlayer.GetBool()) 
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, false);
                    Logger.Info($" {__instance.Data.PlayerName} was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}", "KickLowLevelPlayer");
                    Logger.SendInGame($" {__instance.Data.PlayerName} was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, true);
                    Logger.Info($" {__instance.Data.PlayerName} was banned for being under level {Options.KickLowLevelPlayer.GetInt()} ", "BanLowLevelPlayer");
                    Logger.SendInGame($" {__instance.Data.PlayerName} was banned for being under level {Options.KickLowLevelPlayer.GetInt()}");
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        if (Options.DisableReport.GetBool() && target != null || Options.Gamemode.GetValue() == 2)
        {
            Logger.Info($" Stopped {__instance.Data.PlayerName} reporting the body of {target.PlayerName}", "ReportDeadBodyPatch");
            return false;
        }
        else return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
internal static class MurderPlayerPatch
{
    public static readonly Dictionary<byte, int> misfireCount = new();

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags, ref bool __state)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        //2 = Shift and Seek
        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (!resultFlags.HasFlag(MurderResultFlags.Succeeded))
            return;

            byte playerId = __instance.Data.PlayerId;

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
                    Logger.Info($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} times and suicided", "SNSKillManager");
                    Logger.SendInGame($" {__instance.Data.PlayerName} misfired {misfireCount[playerId]} times and suicided");
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
