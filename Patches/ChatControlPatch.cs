using Hazel;
using InnerNet;
using System;
using UnityEngine;

// https://github.com/SuperNewRoles/SuperNewRoles/blob/master/SuperNewRoles/Patches/LobbyBehaviourPatch.cs
namespace AmongUsRevamped;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal static class ChatControllerUpdatePatch
{

    public static void Postfix(ChatController __instance)
    {
        if (Main.DarkTheme.Value)
        {
            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);

            __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.text.color = Color.white;
        }
        else __instance.freeChatField.textArea.outputText.color = Color.black;
    }
}

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
        return false;
        
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