using AmongUs.Data;
using AmongUs.GameOptions;
using InnerNet;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal static class OnGameJoinedPatch
{
    public static bool WaitingForChat;
    public static bool AutoStartCheck;
    public static void Postfix()
    {
        Logger.Info(" -------- JOINED GAME --------", "OnGameJoined");

        if (!AmongUsClient.Instance.AmHost) return;

        WaitingForChat = false;
        AutoStartCheck = false;

        if (Main.AutoStart.Value)
        {
            LateTask.Tasks.Clear();

            new LateTask(() =>
            {
                AutoStartCheck = true;
            }, Options.WaitAutoStart.GetFloat(), "AutoStartTimer");
        }

        if (Options.AutoSendGameInfo.GetBool() && !string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason))
        {      
            WaitingForChat = true;

            new LateTask(() =>
            {        
                Utils.ShowLastResult();
            }, 3f, "AutoSendGameInfo");

            new LateTask(() =>
            {        
                WaitingForChat = false;
            }, 5.2f, "AutoSendGameInfo2");
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class OnPlayerJoinedPatch
{
    public static bool HasInvalidFriendCode(string friendcode)
    {
        if (string.IsNullOrEmpty(friendcode))
        {
            return true;
        }

        if (friendcode.Count(c => c == '#') != 1)
        {
            return true;
        }

        string pattern = @"[\W\d]";
        if (Regex.IsMatch(friendcode[..friendcode.IndexOf("#")], pattern))
        {
            return true;
        }

        return false;
    }

    static void Postfix([HarmonyArgument(0)] ClientData Client)
    {
        Logger.Info($" {Client.PlayerName} / {Client.FriendCode} / {Client.PlatformData.Platform}", "Joined The Game");

        if (AmongUsClient.Instance.AmHost)
        {
            BanManager.CheckBanPlayer(Client);
            BanManager.IsPlayerInDenyName(Client, Client.PlayerName);
            Logger.Info($" {Client.PlayerName} has access level {Utils.CheckAccessLevel(Client.FriendCode)}", "AccessCheck");

            if (HasInvalidFriendCode(Client.FriendCode) && Options.KickInvalidFriendCodes.GetBool())
            {
                if (!Options.TempBanInvalidFriendCodes.GetBool())
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, false);
                    Logger.Info($" {Client.PlayerName} Was kicked for having an invalid FriendCode", "KickInvalidFriendCode");
                    Logger.SendInGame($" {Client.PlayerName} Was kicked for having an invalid FriendCode");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, true);
                    Logger.Info($" {Client.PlayerName} Was banned for having an invalid FriendCode", "BanInvalidFriendCode");
                    Logger.SendInGame($" {Client.PlayerName} Was banned for having an invalid FriendCode");
                }
            }
        }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    static void Postfix([HarmonyArgument(0)] ClientData client)
    {
        if (!AmongUsClient.Instance.AmHost || !Utils.InGame) return;
        {
            PlayerControlCompleteTaskPatch.CalculateTaskWin();
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetLevel))]
public static class SetLevelPatch
{
    public static List<PlayerControl> HandledLevelKicks = [];
    public static void Prefix(PlayerControl __instance, uint level)
    {
        if (AmongUsClient.Instance.AmHost && level < Options.KickLowLevelPlayer.GetInt() - 1 && __instance.Data.ClientId != AmongUsClient.Instance.HostId)
        {
            if (level == 0 && Options.DontKickLevelOnes.GetBool()) return;
            if (HandledLevelKicks.Contains(__instance)) return;

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
            HandledLevelKicks.Add(__instance);
        }  
    }
}