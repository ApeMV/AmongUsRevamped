using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System;
using System.IO;
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
    public static string ConvertNum(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int digitCount = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]) && ++digitCount > 5)
            {
                var sb = new System.Text.StringBuilder(input.Length);

                foreach (char c in input)
                {
                    if (char.IsDigit(c))
                        sb.Append(Main.CircledDigits[c - '0']);
                    else
                        sb.Append(c);
                }
                return sb.ToString();
            }
        }
        return input;
    }

    public static bool Prefix(ChatController __instance)
    {
        string msgtext = __instance.freeChatField.textArea.text.Trim();
        string text = msgtext.ToLower();
        string converted = ConvertNum(msgtext);

        if (!AmongUsClient.Instance.AmHost) return true;

        if (text == "/reload")
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{Translator.Get("translationReloaded")}");
            Translator.Reload();
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/dump")
        {
            Utils.DumpLog();
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }
        if (text == "/help")
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{Translator.Get("allCommandsFull")}");
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/s" || text == "/start")
        {
            GameStartManager.Instance.BeginGame();
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

        if (text.StartsWith("/vip "))
        {
            PlayerControl target = null;
            string arg = text.Substring(5).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.AppendAllText(BanManager.VipListPath, "\n" + target.Data.FriendCode);
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("vipAdd", target.Data.PlayerName));
            }

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/moderator "))
        {
            PlayerControl target = null;
            string arg = text.Substring(11).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.AppendAllText(BanManager.ModeratorListPath, "\n" + target.Data.FriendCode);
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("moderatorAdd", target.Data.PlayerName));
            }

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/admin "))
        {
            PlayerControl target = null;
            string arg = text.Substring(7).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.AppendAllText(BanManager.AdminListPath, "\n" + target.Data.FriendCode);
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("adminAdd", target.Data.PlayerName));
            }

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/removevip "))
        {
            PlayerControl target = null;
            string arg = text.Substring(11).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.WriteAllLines(BanManager.VipListPath, File.ReadAllLines(BanManager.VipListPath).Where(x => x.Trim() != target.Data.FriendCode));
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("vipRemove", target.Data.PlayerName));
            }
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/removemoderator "))
        {
            PlayerControl target = null;
            string arg = text.Substring(17).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.WriteAllLines(BanManager.ModeratorListPath, File.ReadAllLines(BanManager.ModeratorListPath).Where(x => x.Trim() != target.Data.FriendCode));
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("moderatorRemove", target.Data.PlayerName));
            }
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/removeadmin "))
        {
            PlayerControl target = null;
            string arg = text.Substring(13).Trim();

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer) continue;

                if (p.Data.PlayerName.Equals(arg, StringComparison.OrdinalIgnoreCase))
                {
                    target = p;
                    break;
                }
            }
            if (target == null)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("permFail", arg));
            }
            else
            {
                File.WriteAllLines(BanManager.AdminListPath, File.ReadAllLines(BanManager.AdminListPath).Where(x => x.Trim() != target.Data.FriendCode));
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("adminRemove", target.Data.PlayerName));
            }
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/seeker "))
        {
            if (!Utils.IsLobby || !Utils.isHideNSeek)
            {
                __instance.freeChatField.textArea.Clear();
                __instance.freeChatField.textArea.SetText(string.Empty);
                return false;
            }

            string argSeeker = text.Substring(8).Trim();
            bool found = false;

            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null) continue;

                if (p.Data.PlayerName.Equals(argSeeker, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    PlayerControlSetRolePatch.manualSeekers.Add(p.PlayerId);
                    break;
                }
            }

            if (found)
            {
                List<string> names = new();

                foreach (PlayerControl pl in PlayerControl.AllPlayerControls)
                {
                    if (PlayerControlSetRolePatch.manualSeekers.Contains(pl.PlayerId)) names.Add(pl.Data.PlayerName);
                }
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("seekers") + string.Join(", ", names));
            }

            if (!found)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Player '{argSeeker}' not found.");   
            }

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/clearseeker")
        {
            PlayerControlSetRolePatch.manualSeekers.Clear();
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("clearedSeekers"));

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text == "/seekers")
        {
            List<string> n = new();

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (PlayerControlSetRolePatch.manualSeekers.Contains(pc.PlayerId)) n.Add(pc.Data.PlayerName);
            }
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("seekers") + string.Join(", ", n));

            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;      
        }

        if (__instance.timeSinceLastMessage < 3f || OnGameJoinedPatch.WaitingForChat) return false;

        if (text == "/l" || text == "/lastgame")
        {
            if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return false;
            Utils.ChatCommand(__instance, $"{NormalGameEndChecker.LastWinReason}", "", false);
            return false;
        }

        if (text == "/aur" || text == "/socials")
        {
            Utils.ChatCommand(__instance, "AUR socials:\n\ng i t h u b . c o m /\nApeMV/AmongUsRevamped\n\nd i s c о r d . g g /\n83Zhzhyhya", "", false);
            return false;
        }

        if (text == "/0kc" || text == "/0killcooldown")
        {
            Utils.ChatCommand(__instance, "0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!", "", false);
            return false;
        }

        if (text == "/sns" || text == "/shiftandseek")
        {
            if (Options.MisfiresToSuicide.GetInt() == 1 || Options.CantKillTime.GetInt() == 0)
            {
                Utils.ChatCommand(__instance, "Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);                
            }
            else
            {
                Utils.ChatCommand(__instance, "Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", ConvertNum($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Options.MisfiresToSuicide.GetInt()} wrong kills = suicide"), true);
            }
            return false;
        }

        if (text == "/sr" || text == "/speedrun")
        {
            Utils.ChatCommand(__instance, $"Speedrun:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s", "", false);
            return false;
        }

        if (text == "/r" || text == "/roles")
        {
            switch (Options.Gamemode.GetValue())
            {
                case 0:
                if (GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) == 0.01f)
                {
                    Utils.ChatCommand(__instance, "0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!", "", false);
                }
                break;

                case 1:
                if (Options.MisfiresToSuicide.GetInt() == 1 || Options.CantKillTime.GetInt() == 0)
                {
                    Utils.ChatCommand(__instance, "Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);                
                }
                else
                {
                    Utils.ChatCommand(__instance, "Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", ConvertNum($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Options.MisfiresToSuicide.GetInt()} wrong kills = suicide"), true);
                }          
                break;

                case 2:
                Utils.ChatCommand(__instance, $"Speedrun:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s", "", false);
                break;

            }
            __instance.timeSinceLastMessage = 0.8f;
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }

        if (text.StartsWith("/t "))
        {
            string templateName = text[3..].Trim().ToLower();

            if (BanManager.Templates.TryGetValue(templateName, out string templateMessage))
            {
                if (templateMessage.Length < 121)
                {
                    Utils.ChatCommand(__instance, BanManager.CheckTemplate(templateMessage), "", false);
                }
                else
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("tooLongTemplate")); 
                }
            }
            else
            {
                Logger.Info($"Could not find template '{templateName}'", "TemplateManager");
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Template '{templateName}' not found.");
            }
            __instance.freeChatField.textArea.Clear();
            __instance.freeChatField.textArea.SetText(string.Empty);
            return false;
        }
        
        bool col1 = text.StartsWith("/col ");
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
                
                //__instance.freeChatField.textArea.SetText(converted);
                //Utils.ChatCommand(__instance, $"{converted}", "", false);
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
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
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
                
                BanManager.IsStartWord(__instance, text);
                BanManager.IsWordBanned(__instance, text);

                if (text.StartsWith("/seeker ") && Utils.CheckAccessLevel(__instance.Data.FriendCode) >= Options.SlashEndMeetingCmd.GetValue())
                {
                    if (!Utils.IsLobby || !Utils.isHideNSeek)
                    {
                        return;
                    }

                    string argSeeker = text.Substring(8).Trim();
                    bool found = false;

                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.Data == null) continue;

                        if (p.Data.PlayerName.Equals(argSeeker, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            PlayerControlSetRolePatch.manualSeekers.Add(p.PlayerId);
                            break;
                        }
                    }

                    if (found)
                    {
                        List<string> names = new();

                        foreach (PlayerControl pl in PlayerControl.AllPlayerControls)
                        {
                            if (PlayerControlSetRolePatch.manualSeekers.Contains(pl.PlayerId)) names.Add(pl.Data.PlayerName);
                        }
                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("seekers") + string.Join(", ", names));
                    }

                    if (!found)
                    {
                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Player '{argSeeker}' not found.");   
                    }
                }

                if (text == "/clearseeker" && Utils.CheckAccessLevel(__instance.Data.FriendCode) >= Options.SlashEndMeetingCmd.GetValue())
                {
                    PlayerControlSetRolePatch.manualSeekers.Clear();
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("clearedSeekers"));
                }

                bool col1 = text.StartsWith("/col ");
                bool col2  = text.StartsWith("/color ");
                bool col3 = text.StartsWith("/colour ");

                if ((col1 || col2 || col3) && !Utils.InGame)
                {
                    string argCol = text.Substring(col1 ? 5 : col2 ? 7 : col3 ? 8 : 0).Trim();

                    if (Utils.TryGetColorId(argCol, out byte colId))
                    {
                        if (Utils.CheckAccessLevel(__instance.Data.FriendCode) >= Options.SlashColorCmd.GetValue())
                        {
                            if (colId > 17 && !Options.AllowFortegreen.GetBool()) return;
                            __instance.RpcSetColor(colId);
                        }    
                    }
                }

                // Banning works by name and color. Commands are separated incase someone has a color as their name
                bool isKick = text.StartsWith("/kick ");
                bool isBan  = text.StartsWith("/ban ");

                bool isColorKick = text.StartsWith("/ckick ");
                bool isColorBan  = text.StartsWith("/cban ");

                bool banLog = isBan || isColorBan;

                if (isKick || isBan || isColorKick || isColorBan)
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) >= Options.SlashKickCmd.GetValue())
                    {
                        string arg = text.Substring(isKick ? 6 : isBan ? 5 : isColorKick ? 7 : isColorBan ? 6 : 0).Trim();

                        PlayerControl target = null;

                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            if (p.Data == null || p == PlayerControl.LocalPlayer || Utils.CheckAccessLevel(p.Data.FriendCode) >= 1) continue;

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
                        if (banLog && Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashBanCmd.GetValue()) return;

                        if (target != null)
                        {
                            AmongUsClient.Instance.KickPlayer(target.Data.ClientId, isBan || isColorBan);
                            Logger.Info($" {__instance.Data.PlayerName} {(banLog ? "banned" : "kicked")} {target.Data.PlayerName}", "Kick&BanCommand");
                        }
                    }
                }

                if (OnGameJoinedPatch.WaitingForChat || GameStartManagerUpdatePatch.CountingDown) return;

                if (text == "/eg" || text == "/endgame")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashStartAndEndGameCmd.GetValue()) return;
                    MessageWriter writer = AmongUsClient.Instance.StartEndGame();
                    writer.Write((byte)GameOverReason.ImpostorDisconnect);
                    AmongUsClient.Instance.FinishEndGame(writer);
                }

                if (text == "/em" || text == "/endmeeting")
                {
                    if (Utils.IsMeeting)
                    {
                        if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashEndMeetingCmd.GetValue()) return;
                        MeetingHud.Instance.RpcClose();
                    }
                }

                if (text == "/s" || text == "/start")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashStartAndEndGameCmd.GetValue()) return;
                    GameStartManager.Instance.BeginGame();
                }

                if (text == "/l" || text == "/lastgame")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashLastGameCmd.GetValue()) return;
                    if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason) || Utils.InGame) return;
                    Utils.ModeratorChatCommand($"{NormalGameEndChecker.LastWinReason}", "", false);
                }

                if (text == "/aur" || text == "/socials")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashLastGameCmd.GetValue()) return;
                    Utils.ModeratorChatCommand("AUR socials:\n\ng i t h u b . c o m /\nApeMV/AmongUsRevamped\n\nd i s c o r d . g g /\n83Zhzhyhya", "", false);
                }

                if (text == "/0kc" || text == "/0killcooldown")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashRolesAndGamemodeCmd.GetValue()) return;
                    Utils.ModeratorChatCommand("0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!", "", false);
                }
                if (text == "/sns" || text == "/shiftandseek")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashRolesAndGamemodeCmd.GetValue()) return;

                    if (Options.MisfiresToSuicide.GetInt() == 1 || Options.CantKillTime.GetInt() == 0)
                    {
                        Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);                
                    }
                    else
                    {
                        
                        Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", SendChatPatch.ConvertNum($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Options.MisfiresToSuicide.GetInt()} wrong kills = suicide"), true);
                    }
                }

                if (text == "/sr" || text == "/speedrun")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashRolesAndGamemodeCmd.GetValue()) return;
                    Utils.ModeratorChatCommand($"Speedrun:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s", "", false);
                }

                if (text == "/r" || text == "/roles")
                {
                    if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashRolesAndGamemodeCmd.GetValue()) return;
                    switch (Options.Gamemode.GetValue())
                    {
                        case 0:
                        if (GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) == 0.01f)
                        {
                            Utils.ModeratorChatCommand("0 Kill Cooldown:\n\nImpostors have no kill cooldown, Crewmates have low tasks\nThink fast and pay attention!", "", false);
                        }
                        break;

                        case 1:
                        if (Options.MisfiresToSuicide.GetInt() == 1 || Options.CantKillTime.GetInt() == 0)
                        {
                            Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", $"Crewmates win by doing tasks or surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImpostors win by killing everyone\n{Options.MisfiresToSuicide.GetInt()} wrong kill(s) = suicide", true);                
                        }
                        else
                        {
                            Utils.ModeratorChatCommand("Shift and Seek:\n\nImpostors can only kill someone while shapeshifted as them\nEmergency Meetings = Off", SendChatPatch.ConvertNum($"Crew wins by tasks/surviving {Options.CrewAutoWinsGameAfter.GetInt()}s\nImp wins by killing\nOne wrong kill = Can't kill for {Options.CantKillTime.GetInt()}s\n{Options.MisfiresToSuicide.GetInt()} wrong kills = suicide"), true);
                        }         
                        break;

                        case 2:
                        Utils.ModeratorChatCommand($"Speedrun:\n\nEveryone is a crewmate. The 1st player to finish tasks wins the game. Game auto ends after {Options.GameAutoEndsAfter.GetInt()}s", "", false);
                        break;

                    }
                }

                if (Utils.CheckAccessLevel(__instance.Data.FriendCode) < Options.SlashRolesAndGamemodeCmd.GetValue()) return;

                if (text.StartsWith("/t "))
                {
                    string templateName = text[3..].Trim().ToLower();

                    if (BanManager.Templates.TryGetValue(templateName, out string templateMessage))
                    {
                        if (templateMessage.Length < 121)
                        {  
                            Utils.ModeratorChatCommand(BanManager.CheckTemplate(templateMessage), "", false);
                        }
                        else
                        {
                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Translator.Get("tooLongTemplate"));                            
                        }
                    }
                }
                break;
            }
        }
    }
}