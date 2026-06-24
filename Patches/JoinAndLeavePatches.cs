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
    public static int JoinNum;
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
                    Logger.Info(Translator.Get("invalidFriendCodeKick", Client.PlayerName), "KickInvalidFriendCode");
                    Logger.SendInGame(Translator.Get("invalidFriendCodeKick", Client.PlayerName));
                }
                else
                {
                    if (!BanManager.TempBanWhiteList.Contains(Client.GetHashedPuid())) BanManager.TempBanWhiteList.Add(Client.GetHashedPuid());
                    AmongUsClient.Instance.KickPlayer(Client.Id, true);
                    Logger.Info(Translator.Get("invalidFriendCodeBan", Client.PlayerName), "BanInvalidFriendCode");
                    Logger.SendInGame(Translator.Get("invalidFriendCodeBan", Client.PlayerName));
                }
            }

            if (Options.EnableJoinMessages.GetBool())
            {
                JoinNum++;
                BanManager.Templates.TryGetValue("welcome", out string msg);

                if (JoinNum >= Options.MessagePerPlayerNum.GetInt())
                {
                    JoinNum = 0;
                    Utils.ModeratorChatCommand(BanManager.CheckTemplate(msg), "", false);
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
        if (!AmongUsClient.Instance.AmHost) return;
        Logger.Info($" {client.PlayerName} left the game", "OnPlayerLeft");

        if (!Utils.InGame) return;
        PlayerControlCompleteTaskPatch.CalculateTaskWin();
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
                Logger.Info($" {__instance.Data.PlayerName} (level {level + 1}) was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}", "KickLowLevelPlayer");
                Logger.SendInGame($" {__instance.Data.PlayerName} (level {level + 1}) was kicked for being under level {Options.KickLowLevelPlayer.GetInt()}");
            }
            else
            {
                AmongUsClient.Instance.KickPlayer(__instance.Data.ClientId, true);
                Logger.Info($" {__instance.Data.PlayerName} (level {level + 1}) was banned for being under level {Options.KickLowLevelPlayer.GetInt()} ", "BanLowLevelPlayer");
                Logger.SendInGame($" {__instance.Data.PlayerName} (level {level + 1}) was banned for being under level {Options.KickLowLevelPlayer.GetInt()}");
            }
            HandledLevelKicks.Add(__instance);
        }  
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class ShowLobbyTimer_GameStartManager_Start_Postfix
{
    public static void Postfix(GameStartManager __instance)
    {
        if (__instance == null || !GameData.Instance || !AmongUsClient.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame || !AmongUsClient.Instance.AmHost) return;

        HudManager.Instance.ShowLobbyTimer(600);
    }
}