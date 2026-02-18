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
    public static bool GamePastRoleSelection => Main.GameTimer > 10f;
    public static bool HandlingGameEnd;
    public static byte CustomGameOverReason;
    public static bool CanCallMeetings;

    public static string ColorString(Color32 color, string str) => $"<#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{str}</color>";
    public static string ColorToHex(Color32 color) => $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";
    public static byte GetActiveMapId() => GameOptionsManager.Instance.CurrentGameOptions.MapId;

    public static bool IsPlayerModerator(string friendCode)
    {
        if (friendCode == "") return false;

        var friendCodesFilePath = @$"{BanManager.DataPath}/AUR-DATA/ModeratorList.txt";
        var friendCodes = File.ReadAllLines(friendCodesFilePath);
        return friendCodes.Any(code => code.Contains(friendCode));
    }

    public static string GetTabName(TabGroup tab)
    {
        switch (tab)
        {
            case TabGroup.SystemSettings:
                return "System Settings";
            case TabGroup.CustomRoleSettings:
                return "Custom Roles";
            case TabGroup.ModSettings:
                return "Gameplay Settings";
            case TabGroup.GamemodeSettings:
                return "Gamemode Settings";
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
        RpcSetTasksPatch.GlobalTaskIds = null;
        HandlingGameEnd = false;
        OnGameJoinedPatch.AutoStartCheck = false;
        Main.GameTimer = 0f;
        MurderPlayerPatch.misfireCount.Clear();
        LateTask.Tasks.Clear();
        FixedUpdateInGamePatch.ProcessedModerators.Clear();   
        NormalGameEndChecker.ImpCheckComplete = false;
        CreateOptionsPickerPatch.SetDleks2 = false;
        CanCallMeetings = true;
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

    public static void SendPrivateMessage(PlayerControl target, string message)
    {
        if (!AmongUsClient.Instance.AmHost || PlayerControl.LocalPlayer == null || target == null || target.Data.ClientId == 255) return;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 13, SendOption.Reliable, target.Data.ClientId);

        writer.Write(message);
        writer.Write(PlayerControl.LocalPlayer.PlayerId);

        AmongUsClient.Instance.FinishRpcImmediately(writer);

    }

    public static void ChatCommand(ChatController __instance, string msg, string msg2, bool multi)
    {
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
        Logger.Info(" -------- GAME ENDED --------", "ContinueEndGame");
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
}