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
    public static string noKcdMode = "0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!";
    public static string SnSModeOne = "Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nSabotages & Meetings = Off";
    public static string SnSModeTwo = $"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Utils.BasicIntToWord(Options.MisfiresToSuicide.GetInt())} wrong kills = suicide";
    public static string speedrunMode = $"Speedrun:\n\nEveryone is a crewmate The 1st player to finish tasks wins the game Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s";

    public static string allCommandsFull = "Commands:\n/r - Current mode description\n/0kc, /sns, /sp - Specific mode description\n/l - Shows last winner info\n/kick, /ban - Bans or kicks a player by name\n/ckick, /cban - Bans or kicks a player by color";
    public static string allCommandsOne = "Commands:\n/r - Current mode description\n/0kc, /sns, /sp - Specific mode description\n/l - Shows last winner info";
    public static string allCommandsTwo = "/kick, /ban - Bans or kicks a player by name\n/ckick, /cban - Bans or kicks a player by color";

    public static bool Prefix(ChatController __instance)
    {
        string text = __instance.freeChatField.textArea.text.Trim();

        if (__instance.timeSinceLastMessage < 3f || OnGameJoinedPatch.WaitingForChat) return false;

        if (text == "/h" || text == "/help" || text == "/cmd" || text == "/commands")
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{allCommandsFull}");
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/l" || text == "/lastgame" || text == "/win" || text == "/winner")
        {
            if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return false;
            Utils.ChatCommand(__instance, $"{NormalGameEndChecker.LastWinReason}", "", false);
            return false;
        }

        if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
        {
            Utils.ChatCommand(__instance, $"{noKcdMode}", "", false);
            return false;
        }

        if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
        {
            Utils.ChatCommand(__instance, $"{SnSModeOne}", $"{SnSModeTwo}", true);
            return false;
        }

        if (text == "/sp" || text == "/sr" || text == "/speedrun")
        {
            Utils.ChatCommand(__instance, $"{speedrunMode}", "", false);
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
                Utils.ChatCommand(__instance, $"{noKcdMode}", "", false);
                break;

                case 2:
                Utils.ChatCommand(__instance, $"{SnSModeOne}", $"{SnSModeTwo}", true);              
                break;

                case 3:
                Utils.ChatCommand(__instance, $"{speedrunMode}", "", false);
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
                string text = subReader.ReadString();
                Logger.Info($" {__instance.Data.PlayerName}: {text}", "SendChat");

                if (!Utils.IsPlayerModerator(__instance.Data.FriendCode) || !Options.ModeratorCanUseCommand.GetBool()) return;

                if (text == "/h" || text == "/help" || text == "/cmd" || text == "/commands")
                {
                    OnGameJoinedPatch.WaitingForChat = true;

                    new LateTask(() =>
                    {
                        Utils.SendPrivateMessage(__instance, $"{SendChatPatch.allCommandsOne}");
                    }, 2.2f, "MHP1");

                    new LateTask(() =>
                    {
                        Utils.SendPrivateMessage(__instance, $"{SendChatPatch.allCommandsTwo}");
                    }, 4.4f, "MHP2");

                    new LateTask(() =>
                    {
                        OnGameJoinedPatch.WaitingForChat = false;
                    }, 6.6f, "MHP3");
                }

                if (text == "/l" || text == "/lastgame" || text == "/win" || text == "/winner")
                {
                    if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return;
                    Utils.ModeratorChatCommand($"{NormalGameEndChecker.LastWinReason}", "", false);
                }

                if (text == "/0kc" || text == "/0kcd" || text == "/0killcooldown")
                {
                    Utils.ModeratorChatCommand($"{SendChatPatch.noKcdMode}", "", false);
                }
                if (text == "/sns" || text == "/shiftandseek" || text == "/shift&seek")
                {
                    Utils.ModeratorChatCommand($"{SendChatPatch.SnSModeOne}", $"{SendChatPatch.SnSModeTwo}", true);   
                }

                if (text == "/sp" || text == "/sr" || text == "/speedrun")
                {
                    Utils.ModeratorChatCommand($"{SendChatPatch.speedrunMode}", "", false);
                }

                if (text == "/r" || text == "/roles" || text == "/gamemode" || text == "/gm")
                {
                    switch (Options.Gamemode.GetValue())
                    {
                        case 0:
                        break;

                        case 1:
                        Utils.ModeratorChatCommand($"{SendChatPatch.noKcdMode}", "", false);
                        break;

                        case 2:
                        Utils.ModeratorChatCommand($"{SendChatPatch.SnSModeOne}", $"{SendChatPatch.SnSModeTwo}", true);              
                        break;

                        case 3:
                        Utils.ModeratorChatCommand($"{SendChatPatch.speedrunMode}", "", false);
                        break;

                    }
                }

                else
                {
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