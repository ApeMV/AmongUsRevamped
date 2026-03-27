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

        if (!AmongUsClient.Instance.AmHost) return;
        DisableDevice.FixedUpdate();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    public static List<PlayerControl> ScheduledKicks = new();
    private static Dictionary<byte, string> LastColors = new();
    private static float t;
    private static GameObject settingsLabel;

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255 || !AmongUsClient.Instance.AmHost) return;
        
        t += Time.deltaTime;
        if (t < 0.2f) return;
        t = 0f;

        if (Utils.IsLobby)
        {
            int access = Utils.CheckAccessLevel(__instance.Data.FriendCode);
            string color = access switch
            {
                1 => "yellow",
                2 => "purple",
                3 => "red",
                _ => "white"
            };

            if (!LastColors.TryGetValue(__instance.PlayerId, out var lastColor) || lastColor != color || !__instance.cosmetics.nameText.text.Contains($"<color={color}>"))
            {
                __instance.cosmetics.nameText.text = $"<color={color}>{__instance.Data.PlayerName}</color>";
                LastColors[__instance.PlayerId] = color;
            }
        }

        if (settingsLabel == null) settingsLabel = GameObject.Find("GameSettingsLabel");

        int gamemode = Options.Gamemode.GetValue();

        switch (gamemode)
        {
            case 1: // 0Kc
                if (Main.NormalOptions.KillCooldown != 0.01f)
                    Main.NormalOptions.KillCooldown = 0.01f;

                if (Options.NoKcdSettingsOverride.GetBool() && settingsLabel == null)
                {
                    Main.NormalOptions.EmergencyCooldown = 0;
                    Main.NormalOptions.TaskBarMode = 0;
                }
                break;

            case 2: // SnS
                if (Main.NormalOptions.KillCooldown != 2.5f)
                    Main.NormalOptions.KillCooldown = 2.5f;

                if (Options.SNSSettingsOverride.GetBool() && settingsLabel == null)
                    Main.NormalOptions.TaskBarMode = 0;
                break;

            case 3: // Speedrun
                if (settingsLabel == null)
                    Main.NormalOptions.TaskBarMode = 0;
                break;

            case 0: // Reset
                if (Main.NormalOptions.KillCooldown <= 0.01f)
                    Main.NormalOptions.KillCooldown = 25f;
                break;
        }

        if (__instance.CanMove && __instance.Data.PlayerLevel + 1 < Options.KickLowLevelPlayer.GetInt() && __instance.Data.ClientId != AmongUsClient.Instance.HostId)
        {
            if (ScheduledKicks.Contains(__instance)) return;

            if (!Options.TempBanLowLevelPlayer.GetBool()) 
            {
                ScheduledKicks.Add(__instance);
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
                    NormalGameEndChecker.LastWinReason = $"Crewmates win! (Timer)\n\nImpostors:\n" + string.Join("\n", NormalGameEndChecker.imps.Select(p => p.Data.PlayerName));
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
                    NormalGameEndChecker.LastWinReason = $"No one wins! (Timer)";
                }
            }
        }
    }
}