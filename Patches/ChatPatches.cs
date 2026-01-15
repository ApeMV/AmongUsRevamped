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

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal static class SendChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text.Trim();
        string noKcdMode = "0 Kill Cooldown Mode:\n\nImpostors have no kill cooldown, Crewmates have low tasks.\nThink fast and pay attention.";
        string SnSModeOne = "Shift and Seek Mode:\n\nImps can only kill someone while shapeshifted as them.\nSabotages & Meetings = Off.";
        string SnSModeTwo = $"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s.\nImp wins by killing Crew.\n1 Wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s.\n{Options.MisfiresToSuicide.GetInt()} Wrong kills = suicide.";
        string speedrunMode = $"Speedrun Mode:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s";

        if (text == "/h" || text == "/help")
        {
//            PlayerControl.LocalPlayer.SendChat("All AUR commands:\n/0kc - Sends the 0 Kill Cooldown gamemode description to everyone");
            return false;
        }
        
        if (__instance.timeSinceLastMessage < 3f || OnGameJoinedPatch.WaitingForChat) return false;

        if (text == "/l" || text == "/lastgame" || text == "/win" || text == "/winner")
        {
            Utils.ShowLastResult();
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
        {
            PlayerControl.LocalPlayer.RpcSendChat($"{noKcdMode}");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
        {
            PlayerControl.LocalPlayer.RpcSendChat($"{SnSModeOne}");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);

            new LateTask(() =>
            {
                if (!Utils.IsLobby) return;
                PlayerControl.LocalPlayer.RpcSendChat($"{SnSModeTwo}");
                __instance.timeSinceLastMessage = 0.8f;
            }, 2.2f, "SNSTutorial2");                
            return false;
        }

        if (text == "/sp" || text == "/sr" || text == "/speedrun")
        {
            PlayerControl.LocalPlayer.RpcSendChat($"{speedrunMode}");
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/r" || text == "/roles" || text == "/gamemode" || text == "/gm")
        {
            switch (Options.Gamemode.GetValue())
            {
                case 0:
                    PlayerControl.LocalPlayer.RpcSendChat($"Custom roles:\nThere are no custom roles.");
                    __instance.timeSinceLastMessage = 0.8f;
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText(string.Empty);
                break;

                case 1:
                    PlayerControl.LocalPlayer.RpcSendChat($"{noKcdMode}");
                    __instance.timeSinceLastMessage = 0.8f;
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText(string.Empty);
                break;

                case 2:
                    PlayerControl.LocalPlayer.RpcSendChat($"{SnSModeOne}");
                    __instance.timeSinceLastMessage = 0.8f;
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText(string.Empty);

                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.RpcSendChat($"{SnSModeTwo}");
                        __instance.timeSinceLastMessage = 0.8f;
                    }, 2.2f, "SNSTutorial2");                
                break;

                case 3:
                    PlayerControl.LocalPlayer.RpcSendChat($"{speedrunMode}");
                    __instance.timeSinceLastMessage = 0.8f;
                    __instance.freeChatField.textArea.Clear();
                    __instance.freeChatField.textArea.SetText(string.Empty);
                break;

            }
            return false;
        }

        else
        {
            bool isKick = text.StartsWith("/kick ");
            bool isBan  = text.StartsWith("/ban ");

            bool isColorKick = text.StartsWith("/ckick ");
            bool isColorBan  = text.StartsWith("/cban ");

            bool banLog = isBan || isColorBan;

            if (!isKick && !isBan && !isColorKick && !isColorBan)
            {
                Logger.Info($" {PlayerControl.LocalPlayer.Data.PlayerName}: {text}", "SendChat");
                return true;
            }

            string arg = text.Substring(isKick ? 6 : isBan ? 5 : isColorKick ? 7 : isColorBan ? 6 : 0).Trim();

            PlayerControl target = null;

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if ((isKick || isBan) && p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }

                if ((isColorKick || isColorBan) && Utils.TryGetColorId(arg, out byte colorId))
                {
                    if (p.Data.DefaultOutfit.ColorId == colorId)
                    {
                        target = p;
                        break;
                    }
                }
            }

            if (target != null)
            {
                AmongUsClient.Instance.KickPlayer(target.Data.ClientId, isBan || isColorBan);
                Logger.Info($" {(banLog ? "banned" : "kicked")} {target.Data.PlayerName}", "Kick&BanCommand");
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText(string.Empty);
            }
            return false;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class RPCHandlerPatch
{
	public static void Prefix(PlayerControl __instance, byte callId, MessageReader reader)
	{
        if (!AmongUsClient.Instance.AmHost) return;

        var rpcType = (RpcCalls)callId;
        MessageReader subReader = MessageReader.Get(reader);

        switch (rpcType)
        {
            case RpcCalls.SendChat:
            {
                string chatText = subReader.ReadString();
                Logger.Info($" {__instance.Data.PlayerName}: {chatText}", "SendChat");

                // Banning works by name and color. Commands are seperated incase someone has a color as their name
                bool isKick = chatText.StartsWith("/kick ");
                bool isBan  = chatText.StartsWith("/ban ");

                bool isColorKick = chatText.StartsWith("/ckick ");
                bool isColorBan  = chatText.StartsWith("/cban ");

                bool banLog = isBan || isColorBan;

                if (!isKick && !isBan && !isColorKick && !isColorBan) return;

                if (Utils.IsPlayerModerator(__instance.Data.FriendCode))
                {
                    string arg = chatText.Substring(isKick ? 6 : isBan ? 5 : isColorKick ? 7 : isColorBan ? 6 : 0).Trim();

                    PlayerControl target = null;

                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.Data == null || p == PlayerControl.LocalPlayer || Utils.IsPlayerModerator(p.Data.FriendCode)) continue;

                        if ((isKick || isBan) && p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                        {
                            target = p;
                            break;
                        }

                        if ((isColorKick || isColorBan) && Utils.TryGetColorId(arg, out byte colorId))
                        {
                            if (p.Data.DefaultOutfit.ColorId == colorId)
                            {
                                target = p;
                                break;
                            }
                        }
                    }

                    if (target != null)
                    {
                        AmongUsClient.Instance.KickPlayer(target.Data.ClientId, isBan || isColorBan);
                        Logger.Info($" {__instance.Data.PlayerName} {(banLog ? "banned" : "kicked")} {target.Data.PlayerName}", "Kick&BanCommand");
                    }
                }
                break;
            }
        }
    }
}