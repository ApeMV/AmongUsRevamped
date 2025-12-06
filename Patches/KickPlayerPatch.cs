using HarmonyLib;
using InnerNet;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/tukasa0001/TownOfHost/blob/main/Patches/ClientPatch.cs
namespace HNSRevamped;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.KickPlayer))]
internal class KickPlayerPatch
{
    public static Dictionary<string, int> AttemptedKickPlayerList = [];
    public static bool Prefix(InnerNetClient __instance, int clientId, bool ban)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        var HashedPuid = AmongUsClient.Instance.GetClient(clientId).GetHashedPuid();
        if (!AttemptedKickPlayerList.ContainsKey(HashedPuid))
            AttemptedKickPlayerList.Add(HashedPuid, 0);
        else if (AttemptedKickPlayerList[HashedPuid] < 10)
        {
            Logger.Fatal($"Kick player Request too fast! Canceled.", "KickPlayerPatch");
            return false;
        }
        if (ban) BanManager.AddBanPlayer(AmongUsClient.Instance.GetRecentClient(clientId));

        return true;
    }
}