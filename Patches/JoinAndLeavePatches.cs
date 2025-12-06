using AmongUs.Data;
using HarmonyLib;
using AmongUs.GameOptions;
using InnerNet;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using System.Linq;


namespace HNSRevamped;

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


            if (HasInvalidFriendCode(Client.FriendCode) && Options.KickInvalidFriendCodes.GetBool())
            {
                if (!Options.TempBanInvalidFriendCodes.GetBool())
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, false);
                    Logger.Info($" {Client.PlayerName} Was kicked for having an invalid FriendCode", "KickInvalidFriendCode");
                }
                else
                {
                    AmongUsClient.Instance.KickPlayer(Client.Id, true);
                    Logger.Info($" {Client.PlayerName} Was banned for having an invalid FriendCode", "BanInvalidFriendCode");
                }
            }
        }
    }
}