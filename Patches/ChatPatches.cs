using Hazel;
using InnerNet;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal static class ChatControllerUpdatePatch
{

    public static void Postfix(ChatController __instance)
    {
        if (!__instance||!__instance.freeChatField||!__instance.freeChatField.textArea||!__instance.freeChatField.background||__instance.freeChatField.textArea.compoText == null||!__instance.freeChatField.textArea.outputText) return;
        if (!__instance.quickChatField||!__instance.quickChatField.background||__instance.quickChatField.text==null) return;

        if (Main.DarkTheme.Value)
        {
            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.freeChatField.textArea.compoText.Color(Color.white);
            __instance.freeChatField.textArea.outputText.color = Color.white;

            __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.text.color = Color.white;
        }
        else
        {
            __instance.freeChatField.background.color = Color.white;
            __instance.freeChatField.textArea.compoText.Color(Color.black);
            __instance.freeChatField.textArea.outputText.color = Color.black;

            __instance.quickChatField.background.color = Color.white;
            __instance.quickChatField.text.color = Color.black;
        }
    }
}

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
internal static class ChatBubbleSetNamePatch
{
    public static void Postfix(ChatBubble __instance, [HarmonyArgument(2)] bool voted)
    {
        if (!__instance||!__instance.playerInfo||!__instance.playerInfo.Object||!__instance.playerInfo.Object.Data||!__instance.TextArea||!__instance.Background) return;

        PlayerControl target = __instance.playerInfo.Object;

        if (Main.DarkTheme.Value)
        {
            __instance.Background.color = new(0.1f, 0.1f, 0.1f, 1f);
            __instance.TextArea.color = Color.white;

            if (__instance.playerInfo.Object.Data.IsDead && Utils.InGame) __instance.Background.color = new(0.1f, 0.1f, 0.1f, 0.7f);
        }
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal static class SendChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        string msgtext = __instance.freeChatField.textArea.text.Trim();
        string text = msgtext.ToLower();

        if (!AmongUsClient.Instance.AmHost) return true;

        if (text == "/dump")
        {
            Utils.DumpLog();
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/h" || text == "/help" || text == "/cmd" || text == "/commands")
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{String.allCommandsFull}");
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/eg" || text == "/endgame")
        {
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);

            if (!Utils.InGame) return false;
            MessageWriter writer = AmongUsClient.Instance.StartEndGame();
            writer.Write((byte)GameOverReason.ImpostorDisconnect);
            AmongUsClient.Instance.FinishEndGame(writer);
            return false;
        }

        if (text == "/em" || text == "/endmeeting")
        {
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            
            if ( !Utils.InGame || !Utils.IsMeeting) return false;
            MeetingHud.Instance.RpcClose();
            return false;
        }

        if (__instance.timeSinceLastMessage < 3f || OnGameJoinedPatch.WaitingForChat) return false;

        if (text == "/l" || text == "/lastgame" || text == "/win" || text == "/winner")
        {
            if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return false;
            Utils.ChatCommand(__instance, $"{NormalGameEndChecker.LastWinReason}", "", false);
            return false;
        }

        if (text == "/aur" || text == "/amongusrevamped" || text == "/socials" || text == "/github" || text == "/discord")
        {
            Utils.ChatCommand(__instance, $"{String.SocialsAll}", "", false);
            return false;
        }

        if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
        {
            Utils.ChatCommand(__instance, $"{String.noKcdMode}", "", false);
            return false;
        }

        if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
        {
            Utils.ChatCommand(__instance, $"{String.SnSModeOne}", $"{String.SnSModeTwo}", true);
            return false;
        }

        if (text == "/sp" || text == "/sr" || text == "/speedrun")
        {
            Utils.ChatCommand(__instance, $"{String.speedrunMode}", "", false);
            return false;
        }

        if (text == "/r" || text == "/roles" || text == "/gamemode" || text == "/gm")
        {
            switch (Options.Gamemode.GetValue())
            {
                case 0:
                Utils.ChatCommand(__instance, $"ENABLED CUSTOM ROLES:\n\n{CustomRoleManagement.GetActiveRoles()}", "", false);
                break;

                case 1:
                Utils.ChatCommand(__instance, $"{String.noKcdMode}", "", false);
                break;

                case 2:
                Utils.ChatCommand(__instance, $"{String.SnSModeOne}", $"{String.SnSModeTwo}", true);              
                break;

                case 3:
                Utils.ChatCommand(__instance, $"{String.speedrunMode}", "", false);
                break;

            }
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }
        
        bool col1 = text.StartsWith("/col ") || text.StartsWith("/cor ");
        bool col2  = text.StartsWith("/color ");
        bool col3 = text.StartsWith("/colour ");

        if (col1 || col2 || col3)
        {
            string argCol = text.Substring(col1 ? 5 : col2 ? 7 : col3 ? 8 : 0).Trim();

            if (Utils.TryGetColorId(argCol, out byte colId) && (col1 || col2 || col3))
            {
                PlayerControl.LocalPlayer.RpcSetColor(colId);
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText(string.Empty);
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
                Logger.Info($" {PlayerControl.LocalPlayer.Data.PlayerName}: {msgtext}", "SendChat");
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
                string msgtext = subReader.ReadString();
                string text = msgtext.ToLower();

                Logger.Info($" {__instance.Data.PlayerName}: {msgtext}", "SendChat");

                string[] keywords = Options.AutoKickStartStrength.GetBool() ? new[] { "start", "begin", "commence" } : new[] { "start", "begin", "commence", "s t a r t", "go" };

                bool c = false;

                if (Options.AutoKickStartStrength.GetBool())
                {
                    c = keywords.Any(k => text.Contains(k));
                }
                else
                {
                    c = keywords.Any(k => text == k);
                }
                if (c && !Utils.IsPlayerModerator(__instance.Data.FriendCode))
                {
                    int clientId = __instance.Data.ClientId;

                    if (!Main.SayStartTimes.ContainsKey(clientId))
                    Main.SayStartTimes.Add(clientId, 0);
                    Main.SayStartTimes[clientId]++;

                    if (Main.SayStartTimes[clientId] >= Options.AutoKickStartTimes.GetInt())
                    {
                        bool sBan = Options.AutoKickStartAsBan.GetBool();
                        AmongUsClient.Instance.KickPlayer(clientId, sBan);
                        Logger.Info($" {__instance.Data.PlayerName} was {(sBan ? "banned" : "kicked")} for saying start", "KickAnnoyingKids");
                        Logger.SendInGame($" {__instance.Data.PlayerName} was {(sBan ? "banned" : "kicked")} for saying start");
                    }
                }

                if (text == "/h" || text == "/help" || text == "/cmd" || text == "/commands")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    OnGameJoinedPatch.WaitingForChat = true;

                    new LateTask(() =>
                    {
                        Utils.SendPrivateMessage(__instance, $"{String.allCommandsOne}");
                    }, 2.2f, "MHP1");

                    new LateTask(() =>
                    {
                        Utils.SendPrivateMessage(__instance, $"{String.allCommandsTwo}");
                    }, 4.4f, "MHP2");

                    new LateTask(() =>
                    {
                        OnGameJoinedPatch.WaitingForChat = false;
                    }, 6.6f, "MHP3");
                }

                if (text == "/l" || text == "/lastgame" || text == "/win" || text == "/winner")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return;
                    Utils.ModeratorChatCommand($"{NormalGameEndChecker.LastWinReason}", "", false);
                }

                if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    Utils.ModeratorChatCommand($"{String.noKcdMode}", "", false);
                }
                if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    Utils.ModeratorChatCommand($"{String.SnSModeOne}", $"{String.SnSModeTwo}", true);   
                }

                if (text == "/sp" || text == "/sr" || text == "/speedrun")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    Utils.ModeratorChatCommand($"{String.speedrunMode}", "", false);
                }

                if (text == "/r" || text == "/roles" || text == "/gamemode" || text == "/gm")
                {
                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;
                    switch (Options.Gamemode.GetValue())
                    {
                        case 0:
                        break;

                        case 1:
                        Utils.ModeratorChatCommand($"{String.noKcdMode}", "", false);
                        break;

                        case 2:
                        Utils.ModeratorChatCommand($"{String.SnSModeOne}", $"{String.SnSModeTwo}", true);              
                        break;

                        case 3:
                        Utils.ModeratorChatCommand($"{String.speedrunMode}", "", false);
                        break;

                    }
                }

                bool col1 = text.StartsWith("/col ") || text.StartsWith("/cor ");
                bool col2  = text.StartsWith("/color ");
                bool col3 = text.StartsWith("/colour ");

                if ((col1 || col2 || col3) && !Utils.InGame)
                {
                    string argCol = text.Substring(col1 ? 5 : col2 ? 7 : col3 ? 8 : 0).Trim();

                    if (Utils.TryGetColorId(argCol, out byte colId))
                    {
                        if (Options.ColorCommandLevel.GetValue() == 0 && Utils.IsPlayerModerator(__instance.Data.FriendCode) || Options.ColorCommandLevel.GetValue() == 1)
                        {
                            if (colId > 17 && !Options.AllowFortegreen.GetBool()) return;
                            __instance.RpcSetColor(colId);
                        }    
                    }
                }
                else
                {

                    if (!Utils.IsPlayerModerator(__instance.Data.FriendCode)) return;
                    // Banning works by name and color. Commands are seperated incase someone has a color as their name
                    bool isKick = text.StartsWith("/kick ");
                    bool isBan  = text.StartsWith("/ban ");

                    bool isColorKick = text.StartsWith("/ckick ");
                    bool isColorBan  = text.StartsWith("/cban ");

                    bool banLog = isBan || isColorBan;

                    if (!isKick && !isBan && !isColorKick && !isColorBan) return;

                    if (Utils.IsPlayerModerator(__instance.Data.FriendCode))
                    {
                        string arg = text.Substring(isKick ? 6 : isBan ? 5 : isColorKick ? 7 : isColorBan ? 6 : 0).Trim();

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
                }
                break;
            }
        }
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
	public static void Postfix(HudManager __instance)
    {
        if (Main.GM.Value)
        {
			__instance.Chat.gameObject.SetActive(true);
			__instance.MapButton.gameObject.SetActive(true); 
        }
    }
}