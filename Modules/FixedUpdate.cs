using Hazel;
using System;
using System.Runtime.CompilerServices;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
public static class FixedUpdate
{
    public static void Postfix()
    {
        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            Main.GameTimer += Time.fixedDeltaTime;
        }

        GameObject n = GameObject.Find("NewRequestInactive");
        if (n != null)
        {
            n.SetActive(false);
        }

        GameObject nr = GameObject.Find("NewRequest");
        if (nr != null)
        {
            nr.SetActive(false);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    public static readonly HashSet<byte> ProcessedModerators = new HashSet<byte>();

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255 || !AmongUsClient.Instance.AmHost) return;
        
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
                Main.NormalOptions.TaskBarMode = 0;
            }
        }

        // Speedrun
        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek && g == null)
        {
            Main.NormalOptions.TaskBarMode = 0;

            if (__instance.AllTasksCompleted() && Utils.InGame && Utils.GamePastRoleSelection && !Utils.HandlingGameEnd)
            {
                if (__instance == PlayerControl.LocalPlayer && Main.GM.Value) return;

                Utils.CustomWinnerEndGame(__instance, 1);
                NormalGameEndChecker.LastWinReason = $"★ {__instance.Data.PlayerName} Wins! (Completed tasks first)";

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

        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            // 2 = Shift and Seek
            if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && Options.CrewAutoWinsGameAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {                        
                if (Main.GameTimer > Options.CrewAutoWinsGameAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);
                    Logger.Info($" Crewmates won because the game took longer than {Options.CrewAutoWinsGameAfter.GetInt()}s", "SNSManager");
                    NormalGameEndChecker.LastWinReason = $"★ Crewmates win! (Timer)\n\nImpostors:\n" + string.Join("\n", NormalGameEndChecker.imps.Select(p => p.Data.PlayerName));
                }
            }
            // 3 = Speedrun
            if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek && Options.GameAutoEndsAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {                        
                if (Main.GameTimer > Options.GameAutoEndsAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.CustomWinnerEndGame(PlayerControl.LocalPlayer, 0);
                    Logger.Info($" No one won because the game took longer than {Options.GameAutoEndsAfter.GetInt()}s", "SpeedrunManager");
                    NormalGameEndChecker.LastWinReason = $"★ No one wins! (Timer)";
                }
            }
        }
    }
}