using Hazel;
using InnerNet;
using System;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
internal static class AddVotePatch
{
    public static bool Prefix(VoteBanSystem __instance, [HarmonyArgument(0)] int srcClient, [HarmonyArgument(1)] int clientId)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        PlayerControl pc = Utils.GetClientById(srcClient)?.Character;
        PlayerControl target = Utils.GetClientById(clientId)?.Character;

        if (AmongUsClient.Instance.ClientId == srcClient)
        {
            AmongUsClient.Instance.KickPlayer(target.Data.ClientId, false);

            Logger.Info($" Kicked {target.Data.PlayerName}, {target.Data.FriendCode}", "KickPatch");
        }
        return true;
        
    }
}

[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
internal static class CmdAddVotePatch
{
    public static bool Prefix([HarmonyArgument(0)] int clientId)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        else return false;
    }
}

[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
internal static class BanMenuSetVisiblePatch
{
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;
        __instance.BanButton.gameObject.SetActive(true);
        __instance.KickButton.gameObject.SetActive(true);
        __instance.MenuButton.gameObject.SetActive(show);
        
        return false;
    }
}