using AmongUs.GameOptions;
using Hazel;
using System;
using System.Runtime.CompilerServices;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.FixedUpdate))]
public static class FixedUpdate
{
    public static Dictionary<byte, float> BadMovementTimer = new Dictionary<byte, float>();
    public static readonly Dictionary<string, Vector2> SpawnRange = new()
    {
        ["Skeld"] = new(-0.8f, 1.2f),
        ["Mira"] = new(-4.4f, 2.4f),
        ["MiraMeeting"] = new(23.9f, 2.6f),
        ["Polus"] = new(16.6f, -1.1f),
        ["PolusMeeting"] = new(19.5f, -16.9f),
        ["AirshipMeeting"] = new(11f, 15.3f),
        ["Fungle"] = new(-9.8f, 1.8f),
        ["FungleMeetings"] = new(-3.1f, -1.3f)
    };

    public static float Range = 4f;

    public static Dictionary<byte, Vector2> Position = new Dictionary<byte, Vector2>();
    public static void Postfix()
    {
        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            Main.GameTimer += Time.fixedDeltaTime;
            Main.AfkTimer += Time.fixedDeltaTime;
        }

        GameObject n = GameObject.Find("NewRequestInactive");
        if (n != null)
        {
            n.SetActive(false);
        }

        GameObject nr = GameObject.Find("NewRequest");
        if (nr != null)
        {
            nr.SetActive(false);
        }

        if (!AmongUsClient.Instance.AmHost) return;
        DisableDevice.FixedUpdate();

        if (Main.AfkTimer >= Options.AfkTimer.GetInt())
        {
            Main.AfkTimer = 0;

            if (!Options.EnableAfkDetection.GetBool()) return;

            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p == PlayerControl.LocalPlayer || p.Data.IsDead) continue;
                
                Vector2 newPos = p.GetTruePosition();

                if (Options.OnlyDetectSpawn.GetBool() && !IsSpawn(newPos))
                {
                    Position[p.PlayerId] = newPos;
                    continue;
                }

                if (Position.TryGetValue(p.PlayerId, out Vector2 oldPos))
                {
                    if (Vector2.Distance(oldPos, newPos) < 0.05f)
                    {
                        switch (Options.AfkPenalty.GetValue())
                        {
                            case 0:
                                AmongUsClient.Instance.KickPlayer(p.Data.ClientId, false);
                                Logger.Info(Translator.Get("afkKick", p.Data.PlayerName, Options.AfkTimer.GetInt()), "AfkManagement");
                                Logger.SendInGame(Translator.Get("afkKick", p.Data.PlayerName, Options.AfkTimer.GetInt()));
                                break;

                            case 1:
                                AmongUsClient.Instance.KickPlayer(p.Data.ClientId, true);
                                Logger.Info(Translator.Get("afkBan", p.Data.PlayerName, Options.AfkTimer.GetInt()), "AfkManagement");
                                Logger.SendInGame(Translator.Get("afkBan", p.Data.PlayerName, Options.AfkTimer.GetInt()));
                                break;

                            case 2:
                                Logger.Info(Translator.Get("afkNotify", p.Data.PlayerName, Options.AfkTimer.GetInt()), "AfkManagement");
                                Logger.SendInGame(Translator.Get("afkNotify", p.Data.PlayerName, Options.AfkTimer.GetInt()));
                                break;
                        }
                    }
                }
                Position[p.PlayerId] = p.GetTruePosition();
            }
        }

        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data.RoleType == RoleTypes.Phantom && !p.PhantomFadeActive && !p.Data.IsDead && p.MyPhysics.GetVelocity() != new Vector2(0, 0) && OnGameStartPatch.PastStartScreen)
                {
                    if (!BadMovementTimer.ContainsKey(p.Data.PlayerId))
                    {
                        BadMovementTimer[p.Data.PlayerId] = 0f;
                    }

                    BadMovementTimer[p.Data.PlayerId] += Time.fixedDeltaTime;

                    if (BadMovementTimer[p.Data.PlayerId] > Options.BadMoveTimeToSuicide.GetInt())
                    {
                        p.RpcSetRole(RoleTypes.ImpostorGhost);
                        Logger.Info($" {p.Data.PlayerName} wrongfully moved for {Options.BadMoveTimeToSuicide.GetInt()}s and suicided", "SNSKillManager");
                        Logger.SendInGame($" {p.Data.PlayerName} wrongfully moved for {Options.BadMoveTimeToSuicide.GetInt()}s and suicided");
                    }
                }
            }
        }

        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek && !Utils.IsLobby)
        {
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.Data == null || p.Data.IsDead || p.Data.RoleType == RoleTypes.Shapeshifter) continue;

                if (p.protectedByGuardianId == -1)
                {
                    p.RpcProtectPlayer(p, p.cosmetics.ColorId);
                }
            }
        }
    }

    private static bool IsSpawn(Vector2 pos)
    {
        switch (Utils.GetActiveMapId())
        {
            case 0:
                return Vector2.Distance(pos, SpawnRange["Skeld"]) <= Range;

            case 1:
                return Vector2.Distance(pos, SpawnRange["Mira"]) <= Range || Vector2.Distance(pos, SpawnRange["MiraMeeting"]) <= Range;

            case 2:
                return Vector2.Distance(pos, SpawnRange["Polus"]) <= Range || Vector2.Distance(pos, SpawnRange["PolusMeeting"]) <= Range;

            case 4:
                return Vector2.Distance(pos, SpawnRange["AirshipMeeting"]) <= Range;

            case 5:
                return Vector2.Distance(pos, SpawnRange["Fungle"]) <= Range || Vector2.Distance(pos, SpawnRange["FungleMeetings"]) <= Range;

            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
class FixedUpdateInGamePatch
{
    private static Dictionary<byte, string> LastColors = new();
    private static float t;
    private static GameObject settingsLabel;

    public static bool CanUseKillButton(PlayerControl pc)
    {
        pc.Data.Role.CanUseKillButton = false;
        return false;
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (__instance == null || __instance.PlayerId == 255 || !AmongUsClient.Instance.AmHost) return;

        t += Time.deltaTime;
        if (t < 0.2f) return;
        t = 0f;

        if (Utils.IsLobby)
        {
            int access = Utils.CheckAccessLevel(__instance.Data.FriendCode);
            string color = access switch
            {
                1 => "yellow",
                2 => "purple",
                3 => "red",
                _ => "white"
            };

            if (!LastColors.TryGetValue(__instance.PlayerId, out var lastColor) || lastColor != color || !__instance.cosmetics.nameText.text.Contains($"<color={color}>"))
            {
                __instance.cosmetics.nameText.text = $"<color={color}>{__instance.Data.PlayerName}</color>";
                LastColors[__instance.PlayerId] = color;
            }
        }

        if (settingsLabel == null) settingsLabel = GameObject.Find("GameSettingsLabel");

        int gamemode = Options.Gamemode.GetValue();

        switch (gamemode)
        {

            case 0:
                break;

            case 1: // SnS
                if (Utils.InGame) return;
                if (Main.NormalOptions.KillCooldown != 2.5f)
                {
                    Main.NormalOptions.KillCooldown = 2.5f;
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Shapeshifter, 3, 100);
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Viper, 0, 0);
                }
                break;

            case 2: // Speedrun
                break;

            case 3: // PnS
                if (Utils.InGame) return;
                if (Main.NormalOptions.KillCooldown != 1f)
                {
                    Main.NormalOptions.KillCooldown = 1f;
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Phantom, 3, 100);
                    Main.NormalOptions.roleOptions.SetRoleRate(RoleTypes.Viper, 0, 0);
                }
                break;
        }

        if (Utils.InGame && !Utils.IsMeeting && !ExileController.Instance)
        {
            // 1 = Shift and Seek
            if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek && Options.CrewAutoWinsGameAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {                        
                if (Main.GameTimer > Options.CrewAutoWinsGameAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);
                    Logger.Info($" Crewmates won because the game took longer than {Options.CrewAutoWinsGameAfter.GetInt()}s", "SNSManager");
                    NormalGameEndChecker.CheckWinnerText("SnSTimer");
                }
            }
            // 2 = Speedrun
            if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek && Options.GameAutoEndsAfter.GetInt() != 0 && !Options.NoGameEnd.GetBool())
            {                        
                if (Main.GameTimer > Options.GameAutoEndsAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.CustomWinnerEndGame(PlayerControl.LocalPlayer, 0);
                    Logger.Info($" No one won because the game took longer than {Options.GameAutoEndsAfter.GetInt()}s", "SpeedrunManager");
                    NormalGameEndChecker.CheckWinnerText("NoOneWinsSpeedrun");
                }
            }
            // 3 = Poof and Seek
            if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek)
            {
                if (Main.GameTimer > Options.PNSCrewAutoWinsGameAfter.GetInt())
                {
                    Main.GameTimer = 0f;

                    Utils.ContinueEndGame((byte)GameOverReason.CrewmatesByVote);
                    Logger.Info($" Crewmates won because the game took longer than {Options.PNSCrewAutoWinsGameAfter.GetInt()}s", "PNSManager");
                    NormalGameEndChecker.CheckWinnerText("PnSTimer");
                }
            }
        }
    }
}