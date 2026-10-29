using System.Data;
using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System;
using System.Security.Cryptography;
using System.Text;
using AmongUs.InnerNet.GameDataMessages;

namespace AmongUsRevamped;

public static class Utils
{
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StartTime = DateTime.UtcNow;
    private static readonly long EpochStartSeconds = (long)(StartTime - Epoch).TotalSeconds;
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    public static long TimeStamp => EpochStartSeconds + (long)Stopwatch.Elapsed.TotalSeconds;

    public static int allAlivePlayersCount => AllAlivePlayerControls.Count();
    public static int AliveCrewmates => AllAlivePlayerControls.Count(pc => !pc.Data.Role.IsImpostor);
    public static int AliveImpostors => AllAlivePlayerControls.Count(pc => pc.Data.Role.IsImpostor || pc.isNew);

    public static bool IsLobby => AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined;
    public static bool InGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started;
    public static bool isHideNSeek => GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;

    public static bool IsShip => ShipStatus.Instance != null;
    public static bool CanMove => PlayerControl.LocalPlayer?.CanMove is true;
    public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;

    public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool IsMeeting => InGame && (MeetingHud.Instance);
    public static bool HandlingGameEnd;
    public static byte CustomGameOverReason;
    public static bool CanCallMeetings;
    public static bool HasMurdered;

    public static string ColorString(Color32 color, string str) => $"<#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    public static string ColorToHex(Color32 color) => $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";
    public static byte GetActiveMapId() => GameOptionsManager.Instance.CurrentGameOptions.MapId;

    public static int CheckAccessLevel(string friendCode)
    {
        if (friendCode == "") return 0;

        var vipFilePath = @$"{BanManager.DataPath}/AUR-DATA/VIP.txt";
        var vipFriendCodes = File.ReadAllLines(vipFilePath);

        var moderatorFilePath = @$"{BanManager.DataPath}/AUR-DATA/Moderator.txt";
        var moderatorFriendCodes = File.ReadAllLines(moderatorFilePath);

        var adminFilePath = @$"{BanManager.DataPath}/AUR-DATA/Admin.txt";
        var adminFriendCodes = File.ReadAllLines(adminFilePath);

        if (adminFriendCodes.Any(code => code.Contains(friendCode))) return 3;
        if (moderatorFriendCodes.Any(code => code.Contains(friendCode))) return 2;
        if (vipFriendCodes.Any(code => code.Contains(friendCode))) return 1;

        return 0;
    }

    public static string GetTabName(TabGroup tab)
    {
        switch (tab)
        {
            case TabGroup.SystemSettings:
                return Translator.Get("systemSettings");
            case TabGroup.ModerationSettings:
                return Translator.Get("moderationSettings");
            case TabGroup.ModSettings:
                return Translator.Get("gameplaySettings");
            case TabGroup.GamemodeSettings:
                return Translator.Get("gamemodeSettings");
            default:
                return "";
        }
    }

    public static bool IsCustomOption(NumberOption option)
    {
        return option.GetComponent<OptionItem>() != null;
    }

    public static void DestroyTranslator(this GameObject obj)
    {
        var translator = obj.GetComponent<TextTranslatorTMP>();
        if (translator != null)
        {
            Object.Destroy(translator);
        }
    }

    public static void DestroyTranslator(this MonoBehaviour obj) => obj.gameObject.DestroyTranslator();

    public static void CustomSettingsChangeMessageLogic(this NotificationPopper notificationPopper, OptionItem optionItem, string text, bool playSound)
    {
        if (notificationPopper.lastMessageKey == 10000 + optionItem.Id && notificationPopper.activeMessages.Count > 0)
        {
            notificationPopper.activeMessages[notificationPopper.activeMessages.Count - 1].UpdateMessage(text);
        }
        else
        {
            notificationPopper.lastMessageKey = 10000 + optionItem.Id;
            LobbyNotificationMessage settingmessage = Object.Instantiate(notificationPopper.notificationMessageOrigin, Vector3.zero, Quaternion.identity, notificationPopper.transform);
            settingmessage.transform.localPosition = new Vector3(0f, 0f, -2f);
            settingmessage.SetUp(text, notificationPopper.settingsChangeSprite, notificationPopper.settingsChangeColor, new Action(() =>
            {
                notificationPopper.OnMessageDestroy(settingmessage);
            }));
            notificationPopper.ShiftMessages();
            notificationPopper.AddMessageToQueue(settingmessage);
        }
        if (playSound)
        {
            SoundManager.Instance.PlaySoundImmediate(notificationPopper.settingsChangeSound, false, 1f, 1f, null);
        }
    }

    public static string GetOptionNameSCM(this OptionItem optionItem)
    {
        if (optionItem.Name == "Enable")
        {
            int id = optionItem.Id;
            while (id % 10 != 0)
                --id;
            var optionItem2 = OptionItem.AllOptions.FirstOrDefault(opt => opt.Id == id);
            return optionItem2 != null ? optionItem2.GetName() : optionItem.GetName();
        }
        else
        return optionItem.GetName();
    }

    public static string GetRegionName(IRegionInfo region = null)
    {
        region ??= ServerManager.Instance.CurrentRegion;

        string name = region.Name;

        // Joining games shows incorrect regions
        if (!AmongUsClient.Instance.AmHost)
        {
            name = "";
            return name;
        }

        if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame)
        {
            name = "Server: Local Game";
            return name;
        }

        if (region.PingServer.EndsWith("among.us", StringComparison.Ordinal))
        {
            // Official servers
            name = name switch
            {
                "North America" => "Server: NA",
                "Europe" => "Server: EU",
                "Asia" => "Server: AS",
                _ => name
            };

            return name;
        }

        string ip = region.Servers.FirstOrDefault()?.Ip ?? string.Empty;

        if (ip.Contains("aumods.us", StringComparison.Ordinal) || ip.Contains("duikbo.at", StringComparison.Ordinal))
        {
            // Modded Servers
            if (ip.Contains("au-eu"))
                name = "Server: MEU";
            else if (ip.Contains("au-as"))
                name = "Server: MAS";
            else
                name = "Server: MNA";

            return name;
        }

        if (name.Contains("Niko", StringComparison.OrdinalIgnoreCase))
            name = name.Replace("233(", "-").TrimEnd(')');

        return name;
    }
    
    public static ClientData GetClientById(int id)
    {
        try { return AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Id == id); }
        catch { return null; }
    }

    public static void ClearLeftoverData()
    {
        Logger.Info(" Cleared Leftover Data", "Utils");

        RpcSetTasksPatch.GlobalTaskIds = null;
        HandlingGameEnd = false;
        OnGameJoinedPatch.AutoStartCheck = false;
        Main.GameTimer = 0f;
        Main.AfkTimer = 0f;
        FixedUpdate.Position.Clear();

        NormalGameEndChecker.canUpdateWinnerText = true;
        MurderPlayerPatch.misfireCount.Clear();
        MurderPlayerPatch.killCount.Clear();
        HasMurdered = false;

        LateTask.Tasks.RemoveAll(x => x.name != "RehostManager");
        NormalGameEndChecker.ImpCheckComplete = false;
        CreateOptionsPickerPatch.SetDleks2 = false;
        CanCallMeetings = true;

        PlayerControlCompleteTaskPatch.playerTasksCompleted.Clear();
        PlayerControlCompleteTaskPatch.tasksPerPlayer.Clear();
        PlayerControlCompleteTaskPatch.tasksInitiated = false;
        PlayerControlCompleteTaskPatch.ignoredCompletedTasks = 0;
        PlayerControlCompleteTaskPatch.ignoredTasks = 0;

        OnPlayerJoinedPatch.JoinNum = 0;

        PlayerControlSetRolePatch.ProcessedPlayers.Clear();
        PlayerControlSetRolePatch.Seekers.Clear();
        PlayerControlSetRolePatch.FirstAssign = true;
        PlayerControlSetRolePatch.manualSeekers.Clear();

        OnGameStartPatch.PastStartScreen = false;

        FixedUpdate.BadMovementTimer.Clear();

        skeldVotes = 0;
        miraVotes = 0;
        polusVotes = 0;
        airshipVotes = 0;
        fungleVotes = 0;
        dleksVotes = 0;
        HasVoted.Clear();
        MapVoteActive = false;

        standardVotes = 0;
        noKillCooldownVotes = 0;
        speedrunVotes = 0;
        shiftAndSeekVotes = 0;
        poofAndSeekVotes = 0;
        ModeVoteActive = false;
    }

    public static PlayerControl[] AllAlivePlayerControls
    {
        get
        {
            int count = PlayerControl.AllPlayerControls.Count;
            var result = new PlayerControl[count];
            var i = 0;

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null || pc.Data == null || pc.PlayerId >= 254 || pc.Data.IsDead || (pc.Data.Disconnected)) continue;

                result[i++] = pc;
            }

            if (i == 0) return [];

            Array.Resize(ref result, i);
            return result;
        }
    }

    public static bool IsActive(SystemTypes type)
    {
        try
        {
            if (Utils.IsLobby || !ShipStatus.Instance || !ShipStatus.Instance.Systems.TryGetValue(type, out ISystemType systemType)) return false;

            int mapId = Main.NormalOptions.MapId;

            switch (type)
            {
                case SystemTypes.Electrical:
                {
                    if (mapId == 5) return false;

                    var switchSystem = systemType.CastFast<SwitchSystem>();
                    return switchSystem is { IsActive: true };
                }
                case SystemTypes.Reactor:
                {
                    switch (mapId)
                    {
                        case 2:
                            return false;
                        case 4:
                            var heliSabotageSystem = systemType.CastFast<HeliSabotageSystem>();
                            return heliSabotageSystem != null && heliSabotageSystem.IsActive;
                        default:
                            var reactorSystemType = systemType.CastFast<ReactorSystemType>();
                            return reactorSystemType is { IsActive: true };
                    }
                }
                case SystemTypes.Laboratory:
                {
                    if (mapId != 2) return false;

                    var reactorSystemType = systemType.CastFast<ReactorSystemType>();
                    return reactorSystemType is { IsActive: true };
                }
                case SystemTypes.LifeSupp:
                {
                    if (mapId is 2 or 4 or 5) return false;

                    var lifeSuppSystemType = systemType.CastFast<LifeSuppSystemType>();
                    return lifeSuppSystemType is { IsActive: true };
                }
                case SystemTypes.Comms:
                {
                    if (mapId is 1 or 5)
                    {
                        var hqHudSystemType = systemType.TryCast<HqHudSystemType>();
                        return hqHudSystemType != null && hqHudSystemType.IsActive;
                    }

                    var hudOverrideSystemType = systemType.CastFast<HudOverrideSystemType>();
                    return hudOverrideSystemType is { IsActive: true };
                }
                case SystemTypes.HeliSabotage:
                {
                    if (mapId != 4) return false;

                    var heliSabotageSystem = systemType.CastFast<HeliSabotageSystem>();
                    return heliSabotageSystem != null && heliSabotageSystem.IsActive;
                }
                case SystemTypes.MushroomMixupSabotage:
                {
                    if (mapId != 5) return false;

                    var mushroomMixupSabotageSystem = systemType.CastFast<MushroomMixupSabotageSystem>();
                    return mushroomMixupSabotageSystem != null && mushroomMixupSabotageSystem.IsActive;
                }
                default:
                    return false;
            }
        }
        catch (Exception e)
        {
            Logger.Exception(e, "IsActive");
            return false;
        }
    }

    public static void ShowLastResult(byte playerId = byte.MaxValue)
    {
        if (InGame)
        {
            Logger.SendInGame($"Hi, you're currently in-game. Let's use this command afterwards");
            return;
        }

        if (string.IsNullOrEmpty(NormalGameEndChecker.LastWinReason))
        {
            Logger.SendInGame($"Your command was canceled due to not having the required info");
            return;
        }

        PlayerControl.LocalPlayer.RpcSendChat($"{NormalGameEndChecker.LastWinReason}");
    }

    public static bool TryGetColorId(string input, out byte colorId)
    {
        colorId = 0;

        if (Enum.TryParse<Main.ColorToString>(input, true, out var color))
        {
            colorId = (byte)color;
            return true;
        }

        return false;
    }

    public static void ChatCommand(ChatController __instance, string msg, string msg2, bool multi)
    {
        if (OnGameJoinedPatch.WaitingForChat) return;

        OnGameJoinedPatch.WaitingForChat = true;

        __instance.freeChatField.textArea.Clear();
        __instance.freeChatField.textArea.SetText(string.Empty); 

        PlayerControl.LocalPlayer.RpcSendChat($"{msg}");

        new LateTask(() =>
        {
            if (multi && msg2 != "") 
            {
                PlayerControl.LocalPlayer.RpcSendChat($"{msg2}");
            }

            if (!multi) 
            {
                OnGameJoinedPatch.WaitingForChat = false;
            }
        }, 2.2f, "ChatCommand1");

        if (!multi || msg2 == "") return;

        new LateTask(() =>
        {
            OnGameJoinedPatch.WaitingForChat = false;
        }, 4.4f, "ChatCommand2");
    }

    public static void ModeratorChatCommand(string msg, string msg2, bool multi)
    {
        if (OnGameJoinedPatch.WaitingForChat) return;

        OnGameJoinedPatch.WaitingForChat = true;

        new LateTask(() =>
        {
            PlayerControl.LocalPlayer.RpcSendChat($"{msg}");
        }, 2.2f, "ModeratorChatCommand1");

        new LateTask(() =>
        {
            if (multi && msg2 != "") 
            {
                PlayerControl.LocalPlayer.RpcSendChat($"{msg2}");
            }

            if (!multi) 
            {
                OnGameJoinedPatch.WaitingForChat = false;
            }
        }, 4.4f, "ModeratorChatCommand2");

        if (!multi || msg2 == "") return;

        new LateTask(() =>
        {
            OnGameJoinedPatch.WaitingForChat = false;
        }, 6.6f, "ModeratorChatCommand3");
    }

    // 0 = no winner. 1 = solo winner. 2 = add winner.
    public static void CustomWinnerEndGame(PlayerControl winner, int winnerType)
    {
        HandlingGameEnd = true;
        MessageWriter writer = AmongUsClient.Instance.StartEndGame();

        if (winnerType == 0)
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                CustomGameOverReason = (byte)GameOverReason.ImpostorsByVote;
                pc.RpcSetRole(AmongUs.GameOptions.RoleTypes.CrewmateGhost, false);
            }
        }

        if (winnerType == 1)
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                CustomGameOverReason = (byte)GameOverReason.ImpostorsByVote;

                if (pc != winner) 
                {
                    pc.RpcSetRole(AmongUs.GameOptions.RoleTypes.CrewmateGhost, false);
                }
                else
                {
                    pc.RpcSetRole(AmongUs.GameOptions.RoleTypes.ImpostorGhost, false);                    
                }
            }
        }

        new LateTask(() =>
        {
            ContinueEndGame((byte)CustomGameOverReason);
        }, 1f, "CustomWinnerEndGame");     
    }

    public static void ContinueEndGame(byte gameOverReason)
    {
        MessageWriter writer = AmongUsClient.Instance.StartEndGame();
        writer.Write(gameOverReason);
        AmongUsClient.Instance.FinishEndGame(writer);
        HandlingGameEnd = false;
        Logger.Info($"{gameOverReason}", "ContinueEndGame");
    }

    public static void DumpLog()
    {
        string t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
#if ANDROID
        var f = $"{BanManager.DataPath}/AUR-Logs/{t}";
#else
        string f = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/AUR-logs/";
#endif
        string filename = $"{f}AUR-{Main.ModVersion}-{t}.log";
        if (!Directory.Exists(f)) Directory.CreateDirectory(f);
        FileInfo file = new(@$"{Environment.CurrentDirectory}/BepInEx/LogOutput.log");
        file.CopyTo(@filename);

        if (PlayerControl.LocalPlayer != null)
        {
            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"/Dump command activated\n\nFile: AUR-{Main.ModVersion}-{t}");
#if !ANDROID
            ProcessStartInfo psi = new("Explorer.exe") { Arguments = "/e,/select," + @filename.Replace("/", "\\") };
            Process.Start(psi);
#endif
        }
    }

    private readonly static Dictionary<string, Sprite> CachedSprites = [];
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            Logger.Error($"Failed to read Texture： {path}", "LoadSprite");
        }
        return null;
    }

    private static unsafe Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
            stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
            Logger.Error($"Failed to read Texture： {path}", "LoadTextureFromResources");
        }
        return null;
    }

    public static void SetChatVisible()
    {
        PlayerControl.LocalPlayer.CmdReportDeadBody(null);
        MeetingHud.Instance.RpcClose(); 
    }

    public static MethodBase GetStateMachineMoveNext<T>(string methodName)
    {
        var typeName = typeof(T).FullName;
        var stateMachine = typeof(T).GetNestedTypes().FirstOrDefault(x => x.Name.Contains(methodName));

        if (stateMachine == null)
        {
            Logger.Error($"Failed to find {methodName} state machine for {typeName}", "GetStateMachineMoveNext");
            return null;
        }

        var moveNext = AccessTools.Method(stateMachine, "MoveNext");
        if (moveNext == null)
        {
            Logger.Error($"Failed to find MoveNext method for {typeName}.{methodName}", "GetStateMachineMoveNext");
            return null;
        }

        // Logger.Info($"Found {methodName}.MoveNext", "GetStateMachineMoveNext");
        return moveNext;
    }

    public static readonly Dictionary<byte, string> StoredRoleText = new();
    public static string GetRoleText(PlayerControl p)
    {
        if (p == null) return "";

        if (p.Data.RoleType == RoleTypes.Impostor || p.Data.RoleType == RoleTypes.Shapeshifter || p.Data.RoleType == RoleTypes.Phantom || p.Data.RoleType == RoleTypes.Viper || p.Data.RoleType == RoleTypes.ImpostorGhost)
        {
            return $"<color=#ff1919>{p.Data.RoleType}</color>";
        }
        else return $"<color=#8cffff>{p.Data.RoleType}</color>";
    }

    public static int skeldVotes;
    public static int miraVotes;
    public static int polusVotes;
    public static int airshipVotes;
    public static int fungleVotes;
    public static int dleksVotes;
    public static bool MapVoteActive;
    public static readonly List<byte> HasVoted = [];
    public static void MapVote(bool moderator)
    {
        if (MapVoteActive || ModeVoteActive) return;

        MapVoteActive = true;

        string voteMessage = "A map vote is active!\n\n";
        if (!Options.CustomizeMapVote.GetBool()) voteMessage += "A: Skeld\nB: Mira\nC: Polus\nD: Airship\nE: Fungle\nF: Dleks\n";
        if (Options.IncludeSkeld.GetBool()) voteMessage += "A: Skeld\n";
        if (Options.IncludeMira.GetBool()) voteMessage += "B: Mira\n";
        if (Options.IncludePolus.GetBool()) voteMessage += "C: Polus\n";
        if (Options.IncludeAirship.GetBool()) voteMessage += "D: Airship\n";
        if (Options.IncludeFungle.GetBool()) voteMessage += "E: Fungle\n";
        if (Options.IncludeDleks.GetBool()) voteMessage += "F: Dleks\n";
        voteMessage += "\nType /vote 'letter' to vote.";

        if (moderator) Utils.ModeratorChatCommand(voteMessage, "", false);
        else Utils.ChatCommand(DestroyableSingleton<HudManager>.Instance.Chat, voteMessage, "", false);

        new LateTask(() =>
        {
            int highest = -1;
            string winner = "";
            byte mapId = 0;
            bool tie = false;

            void Check(int votes, string name, byte id)
            {
                if (votes > highest)
                {
                    highest = votes;
                    winner = name;
                    mapId = id;
                    tie = false;
                }
                else if (votes == highest)
                {
                    tie = true;
                }
            }

            if (!Options.CustomizeMapVote.GetBool())
            {
                Check(skeldVotes, "Skeld", 0);
                Check(miraVotes, "Mira", 1);
                Check(polusVotes, "Polus", 2);
                Check(airshipVotes, "Airship", 4);
                Check(fungleVotes, "Fungle", 5);
                Check(dleksVotes, "Dleks", 3);
            }
            else
            {
                if (Options.IncludeSkeld.GetBool()) Check(skeldVotes, "Skeld", 0);
                if (Options.IncludeMira.GetBool()) Check(miraVotes, "Mira", 1);
                if (Options.IncludePolus.GetBool()) Check(polusVotes, "Polus", 2);
                if (Options.IncludeAirship.GetBool()) Check(airshipVotes, "Airship", 4);
                if (Options.IncludeFungle.GetBool()) Check(fungleVotes, "Fungle", 5);
                if (Options.IncludeDleks.GetBool()) Check(dleksVotes, "Dleks", 3);
            }

            string voteResults = "";
            if (!Options.CustomizeMapVote.GetBool())
            {
                voteResults += $"Skeld: {skeldVotes}\n";
                voteResults += $"Mira: {miraVotes}\n";
                voteResults += $"Polus: {polusVotes}\n";
                voteResults += $"Airship: {airshipVotes}\n";
                voteResults += $"Fungle: {fungleVotes}\n";
                voteResults += $"Dleks: {dleksVotes}\n";
            }
            else
            {
                if (Options.IncludeSkeld.GetBool()) voteResults += $"Skeld: {skeldVotes}\n";
                if (Options.IncludeMira.GetBool()) voteResults += $"Mira: {miraVotes}\n";
                if (Options.IncludePolus.GetBool()) voteResults += $"Polus: {polusVotes}\n";
                if (Options.IncludeAirship.GetBool()) voteResults += $"Airship: {airshipVotes}\n";
                if (Options.IncludeFungle.GetBool()) voteResults += $"Fungle: {fungleVotes}\n";
                if (Options.IncludeDleks.GetBool()) voteResults += $"Dleks: {dleksVotes}\n";
            }
            voteResults = voteResults.TrimEnd();

            if (highest <= 0)
            {
                ModeratorChatCommand("Lol, no one voted. Hooray for democracy?", "", false);
            }
            else if (tie)
            {
                ModeratorChatCommand($"Map vote tied. No new map selected.\n\n{voteResults}", "", false);
            }
            else
            {
                ModeratorChatCommand(SendChatPatch.ConvertNum($"Winner: {winner} ({highest})\n\n{voteResults}"), "", false);

                if (winner == "Dleks")
                {
                    CreateOptionsPickerPatch.ApplyDleks(Utils.isHideNSeek);
                }
                else
                {
                    CreateOptionsPickerPatch.ClearDleks();

                    if (!Utils.isHideNSeek) Main.NormalOptions.MapId = mapId;
                    else Main.HideNSeekOptions.MapId = mapId;
                }
            }

            skeldVotes = 0;
            miraVotes = 0;
            polusVotes = 0;
            airshipVotes = 0;
            fungleVotes = 0;
            dleksVotes = 0;
            HasVoted.Clear();

            MapVoteActive = false;
        }, Options.MapVoteDuration.GetInt(), "MapPoll");
    }

    public static int standardVotes;
    public static int noKillCooldownVotes;
    public static int speedrunVotes;
    public static int shiftAndSeekVotes;
    public static int poofAndSeekVotes;
    public static bool ModeVoteActive;
    public static readonly List<byte> HasModeVoted = [];
    public static void ModeVote(bool moderator)
    {
        if (ModeVoteActive || MapVoteActive) return;

        ModeVoteActive = true;

        string voteMessage = "A Gamemode vote is active!\n\n";
        if (!Options.CustomizeModeVote.GetBool()) voteMessage += "A: None\nB: No Kill Cooldown\nC: Speedrun\nD: SnS\nE: PnS\n";
        if (Options.IncludeStandard.GetBool()) voteMessage += "A: None\n";
        if (Options.IncludeNoKillCooldown.GetBool()) voteMessage += "B: No Kill Cooldown\n";
        if (Options.IncludeSpeedrun.GetBool()) voteMessage += "C: Speedrun\n";
        if (Options.IncludeShiftAndSeek.GetBool()) voteMessage += "D: SnS\n";
        if (Options.IncludePoofAndSeek.GetBool()) voteMessage += "E: PnS\n";
        voteMessage += "\nType /vote 'letter' to vote.";

        if (moderator) Utils.ModeratorChatCommand(voteMessage, "", false);
        else Utils.ChatCommand(DestroyableSingleton<HudManager>.Instance.Chat, voteMessage, "", false);

        new LateTask(() =>
        {
            int highest = -1;
            string winner = "";
            byte modeId = 0;
            bool tie = false;

            void Check(int votes, string name, byte id)
            {
                if (votes > highest)
                {
                    highest = votes;
                    winner = name;
                    modeId = id;
                    tie = false;
                }
                else if (votes == highest)
                {
                    tie = true;
                }
            }

            if (!Options.CustomizeModeVote.GetBool())
            {
                Check(standardVotes, "None", 0);
                Check(noKillCooldownVotes, "0 Kill Cooldown", 0);
                Check(speedrunVotes, "Speedrun", 2);
                Check(shiftAndSeekVotes, "Shift and Seek", 1);
                Check(poofAndSeekVotes, "Poof and Seek", 3);
            }
            else
            {
                if (Options.IncludeStandard.GetBool()) Check(standardVotes, "None", 0);
                if (Options.IncludeNoKillCooldown.GetBool()) Check(noKillCooldownVotes, "0 Kill Cooldown", 0);
                if (Options.IncludeSpeedrun.GetBool()) Check(speedrunVotes, "Speedrun", 2);
                if (Options.IncludeShiftAndSeek.GetBool()) Check(shiftAndSeekVotes, "Shift and Seek", 1);
                if (Options.IncludePoofAndSeek.GetBool()) Check(poofAndSeekVotes, "Poof and Seek", 3);
            }

            string voteResults = "";
            if (!Options.CustomizeModeVote.GetBool())
            {
                voteResults += $"Standard: {standardVotes}\n";
                voteResults += $"No Kill Cooldown: {noKillCooldownVotes}\n";
                voteResults += $"Speedrun: {speedrunVotes}\n";
                voteResults += $"Shift and Seek: {shiftAndSeekVotes}\n";
                voteResults += $"Poof and Seek: {poofAndSeekVotes}\n";
            }
            else
            {
                if (Options.IncludeStandard.GetBool()) voteResults += $"Standard: {standardVotes}\n";
                if (Options.IncludeNoKillCooldown.GetBool()) voteResults += $"No Kill Cooldown: {noKillCooldownVotes}\n";
                if (Options.IncludeSpeedrun.GetBool()) voteResults += $"Speedrun: {speedrunVotes}\n";
                if (Options.IncludeShiftAndSeek.GetBool()) voteResults += $"Shift and Seek: {shiftAndSeekVotes}\n";
                if (Options.IncludePoofAndSeek.GetBool()) voteResults += $"Poof and Seek: {poofAndSeekVotes}\n";
            }
            voteResults = voteResults.TrimEnd();

            if (highest <= 0)
            {
                ModeratorChatCommand("We have a winner! Just kidding. No one voted.", "", false);
            }
            else if (tie)
            {
                ModeratorChatCommand($"Gamemode vote tied. No new mode selected.\n\n{voteResults}", "", false);
            }
            else
            {
                ModeratorChatCommand(SendChatPatch.ConvertNum($"Winner: {winner} ({highest})\n\n{voteResults}"), "", false);
                Options.Gamemode.SetValue(modeId);

                if (winner == "0 Kill Cooldown")
                {
                    Main.NormalOptions.KillCooldown = 0.001f;
                }

            }
            standardVotes = 0;
            noKillCooldownVotes = 0;
            speedrunVotes = 0;
            shiftAndSeekVotes = 0;
            poofAndSeekVotes = 0;
            HasModeVoted.Clear();

            ModeVoteActive = false;
        }, Options.ModeVoteDuration.GetInt(), "ModePoll");
    }

    // As of v2026.8.18, Innersloth bans the host for hacking if the start game button is pressed when any player is not Serialized (or something similar).
    // CheckValidData serves as an emergency blockage to prevent this and stop host from being kicked.
    public static bool CheckValidData()
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            if (p == null || !p.hasBeenSerialized)
            {
                return false;
            }
        }
        return true;
    }

    // Thanks to g0aty
    public static string RemoveDiacritics(string text)
    {
	    Dictionary<string, string> diacritics = new Dictionary<string, string>(){ {"ª","a"},{"à","a"},{"á","a"},{"â","a"},{"ã","a"},{"ä","a"},{"å","a"},{"ā","a"},{"ă","a"},{"ǎ","a"},{"ɑ","a"},{"α","a"},{"а","a"},{"д","a"},{"ạ","a"},{"ả","a"},{"ấ","a"},{"ầ","a"},{"ẩ","a"},{"ẫ","a"},{"ậ","a"},{"ắ","a"},{"ằ","a"},{"ẳ","a"},{"ẵ","a"},{"ặ","a"},{"⒜","a"},{"ⓐ","a"},{"ａ","a"},{"À","A"},{"Á","A"},{"Â","A"},{"Ã","A"},{"Ä","A"},{"Å","A"},{"Ā","A"},{"Ă","A"},{"Ǎ","A"},{"Α","A"},{"Δ","A"},{"Λ","A"},{"λ","A"},{"А","A"},{"Д","A"},{"Ạ","A"},{"Ả","A"},{"Ấ","A"},{"Ầ","A"},{"Ẩ","A"},{"Ẫ","A"},{"Ậ","A"},{"Ắ","A"},{"Ằ","A"},{"Ẳ","A"},{"Ẵ","A"},{"Ặ","A"},{"Å","A"},{"∆","A"},{"∧","A"},{"⊿","A"},{"⌆","A"},{"⏃","A"},{"⏄","A"},{"⏅","A"},{"Ⓐ","A"},{"▲","A"},{"△","A"},{"★","A"},{"☆","A"},{"⼈","A"},{"⼊","A"},{"⽕","A"},{"ㅅ","A"},{"Ａ","A"},{"♲","A"},{"♳","A"},{"♴","A"},{"♵","A"},{"♶","A"},{"♷","A"},{"♸","A"},{"♹","A"},{"♺","A"},{"♻","A"},{"⚠","A"},{"︽","A"},{"︿","A"},{"♠","A"},{"♣","A"},{"♤","A"},{"♧","A"},
	    {"æ","ae"},{"Æ","AE"},
	    {"Ъ","b"},{"Ь","b"},{"ъ","b"},{"ь","b"},{"⒝","b"},{"ⓑ","b"},{"♭","b"},{"ß","B"},{"Б","B"},{"В","B"},{"в","B"},{"Ⓑ","B"},
	    {"Ы","bl"},{"ы","bi"},
	    {"¢","c"},{"©","c"},{"ç","c"},{"с","c"},{"ς","c"},{"⊂","c"},{"⊄","c"},{"⊆","c"},{"⊊","c"},{"⒞","c"},{"ⓒ","c"},{"ㄷ","c"},{"ｃ","c"},{"ﾧ","c"},{"￠","c"},{"㈂","c"},{"㉢","c"},{"ㄸ","cc"},{"ﾨ","cc"},{"Ç","C"},{"С","C"},{"℃","C"},{"Ⓒ","C"},{"⼕","C"},{"⼖","C"},{"⿷","C"},{"【","C"},{"〖","C"},{"ㄈ","C"},{"Ｃ","C"},
	    {"đ","d"},{"ð","d"},{"δ","d"},{"₫","d"},{"∂","d"},{"⒟","d"},{"ⓓ","d"},{"ㆳ","d"},{"ｄ","d"},{"♩","d"},{"♪","d"},{"Ð","D"},{"Đ","D"},{"Ⓓ","D"},{"Ｄ","D"},
	    {"è","e"},{"é","e"},{"ê","e"},{"ë","e"},{"ē","e"},{"ě","e"},{"ε","e"},{"е","e"},{"ё","e"},{"ẹ","e"},{"ẻ","e"},{"ẽ","e"},{"ế","e"},{"ề","e"},{"ể","e"},{"ễ","e"},{"ệ","e"},{"℮","e"},{"⒠","e"},{"ⓔ","e"},{"⺋","e"},{"⺒","e"},{"ｅ","e"},{"£","E"},{"È","E"},{"É","E"},{"Ê","E"},{"Ë","E"},{"Ē","E"},{"Ě","E"},{"Ε","E"},{"Ξ","E"},{"Σ","E"},{"ξ","E"},{"Ё","E"},{"Е","E"},{"Ẹ","E"},{"Ẻ","E"},{"Ẽ","E"},{"Ế","E"},{"Ề","E"},{"Ể","E"},{"Ễ","E"},{"Ệ","E"},{"€","E"},{"∈","E"},{"∉","E"},{"∊","E"},{"∑","E"},{"Ⓔ","E"},{"モ","E"},{"ㅌ","E"},{"ㆯ","E"},{"㈋","E"},{"㉫","E"},{"Ｅ","E"},{"ミ","E"},
	    {"ƒ","f"},{"⒡","f"},{"ⓕ","f"},{"ｆ","f"},{"℉","F"},{"Ⓕ","F"},{"Ｆ","F"},
	    {"ﬀ","ff"},{"ﬁ","fi"},{"ﬂ","fl"},{"ﬃ","ffi"},{"ﬄ","ffl"},
	    {"ɡ","g"},{"ℊ","g"},{"⒢","g"},{"ⓖ","g"},{"ｇ","g"},{"Ⓖ","G"},{"⼛","G"},{"Ｇ","G"},
	    {"ℏ","h"},{"⒣","h"},{"ⓗ","h"},{"ん","h"},{"Η","H"},{"Н","H"},{"н","H"},{"Ⓗ","H"},{"⧺","H"},{"⧻","HH"},
	    {"¦","i"},{"¡","i"},{"ì","i"},{"í","i"},{"î","i"},{"ï","i"},{"ĩ","i"},{"ī","i"},{"ι","i"},{"ⅰ","i"},{"↑","i"},{"↕","i"},{"⇧","i"},{"Ⓘ","i"},{"┆","i"},{"┇","i"},{"┊","i"},{"┋","i"},{"╎","i"},{"╏","i"},{"⬆","i"},{"⬇","i"},{"〡","i"},{"㆐","i"},{"㇑","i"},{"︴","i"},{"ｉ","i"},{"￤","i"},{"Ì","I"},{"Í","I"},{"Î","I"},{"Ï","I"},{"Ĩ","I"},{"Ī","I"},{"Ι","I"},{"Ⅰ","I"},{"↓","I"},{"⇩","I"},{"∣","I"},{"⒤","I"},{"ⓘ","I"},{"⼁","I"},{"ㅣ","I"},{"︱","I"},{"︳","I"},{"ￜ","I"},{"Ｉ","I"},
	    {"Ⅳ","iv"},{"ⅳ","iv"},{"Ⅵ","vi"},{"ⅵ","vi"},{"Ⅶ","vii"},{"ⅶ","vii"},{"Ⅷ","viii"},{"ⅷ","viii"},{"ⅱ","ii"},{"〢","ii"},{"‼","ii"},{"Ⅱ","II"},{"‖","II"},{"ⅲ","iii"},{"〣","iii"},{"Ⅲ","III"},{"Ю","io"},{"ю","io"},
	    {"⒥","j"},{"ⓙ","j"},{"ｊ","j"},{"Ⓙ","J"},{"㆜","J"},{"Ｊ","J"},
	    {"⒦","k"},{"ⓚ","k"},{"ｋ","k"},{"Κ","K"},{"κ","K"},{"К","K"},{"к","K"},{"Ⓚ","K"},{"Ｋ","K"},{"㉿","K"},
	    {"⒧","l"},{"ⓛ","l"},{"ｌ","l"},{"│","l"},{"┃","l"},{"▎","l"},{"▏","l"},{"⎱","l"},{"Ⓛ","L"},{"Ｌ","L"},{"⎿","L"},{"ㆹ","L"},{"㇄","L"},{"㇏","L"},{"㇗","L"},{"㇙","L"},{"㇜","L"},{"㇟","L"},{"└","L"},{"┕","L"},{"┖","L"},{"┗","L"},{"⻌","L"},{"⻍","L"},{"⻎","L"},{"⿺","L"},{"し","L"},{"じ","L"},{"║","ll"},
	    {"ḿ","m"},{"ⓜ","m"},{"ｍ","m"},{"М","M"},{"м","M"},{"Ḿ","M"},{"Ⓜ","M"},{"𝖬","M"},{"Ｍ","M"},
	    {"ñ","n"},{"ń","n"},{"ň","n"},{"ǹ","n"},{"η","n"},{"Л","n"},{"П","n"},{"л","n"},{"п","n"},{"ⓝ","n"},{"ｎ","n"},{"⺆","n"},{"⺇","n"},{"⼌","n"},{"⼍","n"},{"⼑","n"},{"⾨","n"},{"⾾","n"},{"⿵","n"},{"れ","n"},{"ㄇ","n"},{"︵","n"},{"︷","n"},{"︹","n"},{"︻","n"},{"﹇","n"},{"Ñ","N"},{"Ń","N"},{"Ň","N"},{"Ǹ","N"},{"Ν","N"},{"И","N"},{"Й","N"},{"и","N"},{"й","N"},{"Ⓝ","N"},{"𝖭","N"},{"Ｎ","N"},{"ℵ","N"},
	    {"№","No"},
	    {"¤","o"},{"ò","o"},{"ó","o"},{"ô","o"},{"õ","o"},{"ö","o"},{"ø","o"},{"ō","o"},{"ŏ","o"},{"ơ","o"},{"ǒ","o"},{"ο","o"},{"σ","o"},{"φ","o"},{"о","o"},{"ọ","o"},{"ỏ","o"},{"ố","o"},{"ồ","o"},{"ổ","o"},{"ỗ","o"},{"ộ","o"},{"ớ","o"},{"ờ","o"},{"ở","o"},{"ỡ","o"},{"ợ","o"},{"∅","o"},{"⊕","o"},{"⊖","o"},{"⊗","o"},{"⊘","o"},{"⊙","o"},{"⏀","o"},{"⏁","o"},{"⏂","o"},{"⒪","o"},{"■","o"},{"□","o"},{"▢","o"},{"▣","o"},{"▤","o"},{"▥","o"},{"▦","o"},{"▧","o"},{"▨","o"},{"▩","o"},{"▪","o"},{"▫","o"},{"◆","o"},{"◇","o"},{"◈","o"},{"◉","o"},{"◊","o"},{"○","o"},{"◌","o"},{"◍","o"},{"◎","o"},{"●","o"},{"◐","o"},{"◑","o"},{"◒","o"},{"◓","o"},{"◦","o"},{"◯","o"},{"☉","o"},{"☯","o"},{"♡","o"},{"♢","o"},{"♥","o"},{"♦","o"},{"✿","o"},{"❀","o"},{"⦿","o"},{"㆕","o"},{"ㅁ","o"},{"ㅇ","o"},{"㇣","o"},{"ㇿ","o"},{"㈄","o"},{"㈇","o"},{"㉤","o"},{"㉧","o"},{"㋺","o"},{"ｏ","o"},{"ﾷ","o"},{"￮","o"},{"〼","o"},{"Ò","O"},{"Ó","O"},{"Ô","O"},{"Õ","O"},{"Ö","O"},{"Ø","O"},{"Ō","O"},{"Ŏ","O"},{"Ơ","O"},{"Ǒ","O"},{"Θ","O"},{"Ο","O"},{"Φ","O"},{"Ω","O"},{"θ","O"},{"О","O"},{"Ф","O"},{"ф","O"},{"Ọ","O"},{"Ỏ","O"},{"Ố","O"},{"Ồ","O"},{"Ổ","O"},{"Ỗ","O"},{"Ộ","O"},{"Ớ","O"},{"Ờ","O"},{"Ở","O"},{"Ỡ","O"},{"Ợ","O"},{"Ω","O"},{"Ⓞ","O"},{"▇","O"},{"█","O"},{"▉","O"},{"▊","O"},{"▋","O"},{"▱","O"},{"♼","O"},{"♽","O"},{"⚽","O"},{"⚾","O"},{"⼝","O"},{"⼞","O"},{"⿴","O"},{"〇","O"},{"〄","O"},{"Ｏ","O"},{"⓪","0"},{"⓿","0"},{"０","0"},{"ㄖ","0"},
	    {"œ","oe"},{"Œ","OE"},
	    {"ⓟ","p"},{"ｐ","p"},{"þ","p"},{"Þ","p"},{"ρ","p"},{"р","p"},{"Ⓟ","P"},{"𝖯","P"},{"Ｐ","P"},{"Ρ","P"},{"Р","P"},{"⼙","P"},{"⼫","P"},{"ㄕ","P"},{"ㄗ","P"},{"ㆡ","P"},
	    {"ⓠ","q"},{"ｑ","q"},{"Ⓠ","Q"},{"𝖰","Q"},{"Ｑ","Q"},
	    {"ⓡ","r"},{"ｒ","r"},{"Γ","r"},{"⎾","r"},{"┌","r"},{"┍","r"},{"┎","r"},{"┏","r"},{"╒","r"},{"╓","r"},{"╔","r"},{"╭","r"},{"⺁","r"},{"⼚","r"},{"⼴","r"},{"⽧","r"},{"⿸","r"},{"「","r"},{"『","r"},{"ㄏ","r"},{"ㄬ","r"},{"ㆷ","r"},{"Ⓡ","R"},{"𝖱","R"},{"Ｒ","R"},{"­","R"},{"®","R"},{"Я","R"},{"я","R"},
	    {"ⓢ","s"},{"ｓ","s"},{"$","S"},{"Ⓢ","S"},{"𝖲","S"},{"Ｓ","S"},{"⎰","S"},{"∫","S"},{"∮","S"},{"∾","S"},{"ㄎ","S"},{"ㆶ","S"},{"＄","S"},
	    {"∬","ss"},{"∭","sss"},
	    {"ⓣ","t"},{"ｔ","t"},{"⺊","t"},{"⼔","t"},{"⼗","t"},{"⼘","t"},{"〸","t"},{"ㆺ","t"},{"ㇶ","t"},{"㈦","t"},{"㈩","t"},{"Ⓣ","T"},{"𝖳","T"},{"Ｔ","T"},{"Τ","T"},{"τ","T"},{"Т","T"},{"т","T"},{"⏇","T"},{"⏉","T"},{"┬","T"},{"┭","T"},{"┮","T"},{"┯","T"},{"┰","T"},{"┱","T"},{"┲","T"},{"┳","T"},{"╤","T"},{"╥","T"},{"╦","T"},{"⺅","T"},{"ィ","T"},{"イ","T"},{"ㄒ","T"},{"ￓ","T"},
	    {"〹","tt"},
	    {"µ","u"},{"ũ","u"},{"ū","u"},{"ŭ","u"},{"ư","u"},{"ǔ","u"},{"ǖ","u"},{"ǘ","u"},{"ǚ","u"},{"ǜ","u"},{"ц","u"},{"ụ","u"},{"ủ","u"},{"ứ","u"},{"ừ","u"},{"ú","u"},{"ử","u"},{"ữ","u"},{"ự","u"},{"⒰","u"},{"ⓤ","u"},{"㇃","u"},{"ｕ","u"},{"︶","u"},{"︸","u"},{"︺","u"},{"︼","u"},{"﹈","u"},{"υ","u"},{"Ũ","U"},{"Ū","U"},{"Ŭ","U"},{"Ư","U"},{"Ǔ","U"},{"Ǖ","U"},{"Ǘ","U"},{"Ǚ","U"},{"Ǜ","U"},{"Ц","U"},{"Ụ","U"},{"Ủ","U"},{"Ú","U"},{"Ứ","U"},{"Ừ","U"},{"Ử","U"},{"Ữ","U"},{"Ự","U"},{"∪","U"},{"℧","U"},{"Ⓤ","U"},{"⿶","U"},{"ひ","U"},{"び","U"},{"ぴ","U"},{"Ｕ","U"},
	    {"˅","v"},{"ⅴ","v"},{"∨","v"},{"⒱","v"},{"ⓥ","v"},{"▼","v"},{"▽","v"},{"✓","v"},{"㇢","v"},{"︾","v"},{"﹀","v"},{"ｖ","v"},{"Ⅴ","V"},{"∀","V"},{"Ⓥ","V"},{"Ｖ","V"},
	    {"ⓦ","w"},{"ｗ","w"},{"ω","w"},{"ш","w"},{"щ","w"},{"Ⓦ","W"},{"𝖶","W"},{"Ｗ","W"},{"Ш","W"},{"Щ","W"},{"₩","W"},{"￦","W"},
	    {"ⓧ","x"},{"ｘ","x"},{"χ","x"},{"х","x"},{"ⅹ","x"},{"〆","x"},{"〤","x"},{"×","x"},{"Ⓧ","X"},{"𝖷","X"},{"Ｘ","X"},{"Χ","X"},{"Х","X"},{"Ⅹ","X"},{"╳","X"},{"ㄨ","X"},{"ㆫ","X"},
	    {"⽹","XX"},{"〷","XX"},{"ⓨ","y"},{"ｙ","y"},{"ý","y"},{"ÿ","y"},{"У","y"},{"у","y"},{"ỳ","y"},{"ỵ","y"},{"ỷ","y"},{"ỹ","y"},{"Ⓨ","Y"},{"𝖸","Y"},{"Ｙ","Y"},{"￥","Y"},{"¥","Y"},{"Ý","Y"},{"Υ","Y"},{"Ỳ","Y"},{"Ỵ","Y"},{"Ỷ","Y"},{"Ỹ","Y"},{"ㄚ","Y"},{"ㆩ","Y"},
	    {"ⓩ","z"},{"ｚ","z"},{"Ⓩ","Z"},{"𝖹","Z"},{"Ｚ","Z"},{"Ζ","Z"},{"ζ","Z"},{"⼄","Z"},{"㆚","Z"},{"㇠","Z"},
	    {"①","1"},{"⑴","1"},{"⒈","1"},{"⓵","1"},{"❶","1"},{"➀","1"},{"➊","1"},{"１","1"},
	    {"②","2"},{"⑵","2"},{"⒉","2"},{"⓶","2"},{"❷","2"},{"➁","2"},{"➋","2"},{"２","2"},
	    {"³","3"},{"З","3"},{"Э","3"},{"з","3"},{"э","3"},{"∃","3"},{"∋","3"},{"③","3"},{"⑶","3"},{"⒊","3"},{"⓷","3"},{"❸","3"},{"➂","3"},{"➌","3"},{"⺕","3"},{"⼹","3"},{"る","3"},{"ろ","3"},{"ョ","3"},{"ヨ","3"},{"ヺ","3"},{"ㄋ","3"},{"㇋","3"},{"㇌","3"},{"㇡","3"},{"３","3"},
	    {"④","4"},{"⑷","4"},{"⒋","4"},{"⓸","4"},{"❹","4"},{"➃","4"},{"➍","4"},{"４","4"},
	    {"⑤","5"},{"⑸","5"},{"⒌","5"},{"⓹","5"},{"❺","5"},{"➄","5"},{"➎","5"},{"５","5"},
	    {"⑥","6"},{"⑹","6"},{"⒍","6"},{"⓺","6"},{"❻","6"},{"➅","6"},{"➏","6"},{"６","6"},
	    {"⑦","7"},{"⑺","7"},{"⒎","7"},{"⓻","7"},{"❼","7"},{"➆","7"},{"➐","7"},{"７","7"},
	    {"⑧","8"},{"⑻","8"},{"⒏","8"},{"⓼","8"},{"❽","8"},{"➇","8"},{"➑","8"},{"８","8"},
	    {"⑨","9"},{"⑼","9"},{"⒐","9"},{"⓽","9"},{"❾","9"},{"➈","9"},{"➒","9"},{"９","9"},
	    {"＠","@"},{"﹫","@"},{"＾","^"},{"⌅","^"},{"㆟","^"},
	    {"℀","ac"},{"℅","co"},{"℡","TEL"},{"™","TM"},{"℻","FAX"},{"ⅸ","ix"},{"Ⅸ","IX"},{"ⅺ","xi"},{"Ⅺ","XI"},{"ⅻ","xii"},{"Ⅻ","XII"},{"㉐","PTE"},{"㋌","Hg"},{"㋍","erg"},{"㋎","eV"},{"㋏","LTD"},{"㍱","hPa"},{"㍲","da"},{"㍳","AU"},{"㍴","bar"},{"㍵","oV"},{"㍶","pc"},{"㍷","dm"},{"㍸","dm2"},{"㍹","dm3"},{"㍺","IU"},{"㎀","pA"},{"㎁","nA"},{"㎂","uA"},{"㎃","mA"},{"㎄","kA"},{"㎅","KB"},{"㎆","MB"},{"㎇","GB"},{"㎈","cal"},{"㎉","kcal"},{"㎊","pF"},{"㎋","nF"},{"㎌","uF"},{"㎍","ug"},{"㎎","mg"},{"㎏","kg"},{"㎐","Hz"},{"㎑","kHz"},{"㎒","MHz"},{"㎓","GHz"},{"㎔","Thz"},{"㎕","ul"},{"㎖","ml"},{"㎗","dl"},{"㎘","kl"},{"㎙","fm"},{"㎚","nm"},{"㎛","um"},{"㎜","mm"},{"㎝","cm"},{"㎞","km"},{"㎟","mm2"},{"㎠","cm2"},{"㎡","m2"},{"㎢","km2"},{"㎣","mm3"},{"㎤","cm3"},{"㎥","m3"},{"㎦","km3"},{"㎧","ms"},{"㎨","ms2"},{"㎩","Pa"},{"㎪","kPa"},{"㎫","MPa"},{"㎬","GPa"},{"㎭","rad"},{"㎮","rads"},{"㎯","rads2"},{"㎰","ps"},{"㎱","ns"},{"㎲","us"},{"㎳","ms"},{"㎴","pV"},{"㎵","nV"},{"㎶","uV"},{"㎷","mV"},{"㎸","kW"},{"㎹","MV"},{"㎺","pW"},{"㎻","nW"},{"㎼","uW"},{"㎽","mW"},{"㎾","kW"},{"㎿","MW"},{"㏀","kO"},{"㏁","MO"},{"㏂","am"},{"㏃","Bq"},{"㏄","cc"},{"㏅","cd"},{"㏆","Ckg"},{"㏇","Co"},{"㏈","dB"},{"㏉","Gy"},{"㏊","ha"},{"㏋","HP"},{"㏌","in"},{"㏍","KK"},{"㏎","KM"},{"㏏","kt"},{"㏐","lm"},{"㏑","ln"},{"㏒","log"},{"㏓","lx"},{"㏔","mb"},{"㏕","mil"},{"㏖","mol"},{"㏗","pH"},{"㏘","pm"},{"㏙","PPM"},{"㏚","PR"},{"㏛","sr"},{"㏜","Sv"},{"㏝","Wb"},{"㏞","Vm"},{"㏟","Am"},{"㏿","gal"},
	    {"⑩","10"},{"⑽","10"},{"⒑","10"},{"⓾","10"},{"❿","10"},{"➉","10"},{"➓","10"},{"㉈","10"},{"⑪","11"},{"⑾","11"},{"⒒","11"},{"⓫","11"},{"⑫","12"},{"⑿","12"},{"⒓","12"},{"⓬","12"},{"⑬","13"},{"⒀","13"},{"⒔","13"},{"⓭","13"},{"⑭","14"},{"⒁","14"},{"⒕","14"},{"⓮","14"},{"⑮","15"},{"⒂","15"},{"⒖","15"},{"⓯","15"},{"⑯","16"},{"⒃","16"},{"⒗","16"},{"⓰","16"},{"⑰","17"},{"⒄","17"},{"⒘","17"},{"⓱","17"},{"⑱","18"},{"⒅","18"},{"⒙","18"},{"⓲","18"},{"⑲","19"},{"⒆","19"},{"⒚","19"},{"⓳","19"},{"⑳","20"},{"⒇","20"},{"⒛","20"},{"⓴","20"},{"㉉","20"},{"㉑","21"},{"㉒","22"},{"㉓","23"},{"㉔","24"},{"㉕","25"},{"㉖","26"},{"㉗","27"},{"㉘","28"},{"㉙","29"},{"㉚","30"},{"㉊","30"},{"㉛","31"},{"㉜","32"},{"㉝","33"},{"㉞","34"},{"㉟","35"},{"㊱","36"},{"㊲","37"},{"㊳","38"},{"㊴","39"},{"㊵","40"},{"㉋","40"},{"㊶","41"},{"㊷","42"},{"㊸","43"},{"㊹","44"},{"㊺","45"},{"㊻","46"},{"㊼","47"},{"㊽","48"},{"㊾","49"},{"㊿","50"},{"㉌","50"},{"㉍","60"},{"㉎","70"},{"㉏","80"},
	    {"-"," "},{";"," "},{"."," "},{"'"," "},{"\\"," "},{"/"," "},{","," "},{"、"," "},{"；"," "}, {"!"," "} };

	    var newText = text;

	    foreach (var i in diacritics)
	    {
		    newText = newText.Replace(i.Key, i.Value);
	    }

	    return newText;
    }
}