using AmongUs.GameOptions;
using Hazel;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;
// It was a fun ride, but this won't last.
// Edit: WE ARE SO BACK

public static class AbilityManagement
{
    public static bool IsMayor(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Crewmate && Options.CrewmateAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Scientist && Options.ScientistAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Engineer && Options.EngineerAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Noisemaker && Options.NoisemakerAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Tracker && Options.TrackerAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Detective && Options.DetectiveAbility.GetValue() == 0)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsWorkhorse(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Crewmate && Options.CrewmateAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Scientist && Options.ScientistAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Engineer && Options.EngineerAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Noisemaker && Options.NoisemakerAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Tracker && Options.TrackerAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Detective && Options.DetectiveAbility.GetValue() == 1)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsJester(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Crewmate && Options.CrewmateAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Scientist && Options.ScientistAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Engineer && Options.EngineerAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Noisemaker && Options.NoisemakerAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Tracker && Options.TrackerAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Detective && Options.DetectiveAbility.GetValue() == 2)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsSpeedrunner(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Crewmate && Options.CrewmateAbility.GetValue() == 3 ||
            player.Data.RoleType == RoleTypes.Scientist && Options.ScientistAbility.GetValue() == 3 ||
            player.Data.RoleType == RoleTypes.Engineer && Options.EngineerAbility.GetValue() == 3 ||
            player.Data.RoleType == RoleTypes.Noisemaker && Options.NoisemakerAbility.GetValue() == 3 ||
            player.Data.RoleType == RoleTypes.Tracker && Options.TrackerAbility.GetValue() == 3 ||
            player.Data.RoleType == RoleTypes.Detective && Options.DetectiveAbility.GetValue() == 3)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsTyrant(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Impostor && Options.ImpostorAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Shapeshifter && Options.ShapeshifterAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Phantom && Options.PhantomAbility.GetValue() == 0 ||
            player.Data.RoleType == RoleTypes.Viper && Options.ViperAbility.GetValue() == 0)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsStealer(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Impostor && Options.ImpostorAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Shapeshifter && Options.ShapeshifterAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Phantom && Options.PhantomAbility.GetValue() == 1 ||
            player.Data.RoleType == RoleTypes.Viper && Options.ViperAbility.GetValue() == 1)
        {
            return true;
        }
        else return false; 
    }

    public static bool IsJuggernaut(PlayerControl player)
    {
        if (player.Data.RoleType == RoleTypes.Impostor && Options.ImpostorAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Shapeshifter && Options.ShapeshifterAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Phantom && Options.PhantomAbility.GetValue() == 2 ||
            player.Data.RoleType == RoleTypes.Viper && Options.ViperAbility.GetValue() == 2)
        {
            return true;
        }
        else return false; 
    }

    public static string RoleList()
    {
        if (!AmongUsClient.Instance.AmHost) return "";

        var lines = new List<string>();

        if (Options.CrewmateAbility.GetValue() != 4) lines.Add($"Crewmate: {Options.CrewmateAbility.GetString()}");
        if (Options.ScientistAbility.GetValue() != 4) lines.Add($"Scientist: {Options.ScientistAbility.GetString()}");
        if (Options.EngineerAbility.GetValue() != 4) lines.Add($"Engineer: {Options.EngineerAbility.GetString()}");
        if (Options.NoisemakerAbility.GetValue() != 4) lines.Add($"Noisemaker: {Options.NoisemakerAbility.GetString()}");
        if (Options.TrackerAbility.GetValue() != 4) lines.Add($"Tracker: {Options.TrackerAbility.GetString()}");
        if (Options.DetectiveAbility.GetValue() != 4) lines.Add($"Detective: {Options.DetectiveAbility.GetString()}");

        if (Options.ImpostorAbility.GetValue() != 3) lines.Add($"Impostor: {Options.ImpostorAbility.GetString()}");
        if (Options.ShapeshifterAbility.GetValue() != 3) lines.Add($"Shapeshifter: {Options.ShapeshifterAbility.GetString()}");
        if (Options.PhantomAbility.GetValue() != 3) lines.Add($"Phantom: {Options.PhantomAbility.GetString()}");
        if (Options.ViperAbility.GetValue() != 3) lines.Add($"Viper: {Options.ViperAbility.GetString()}");

        if (lines.Count == 0) return "";

        return "ABILITIES:\n\n" + string.Join("\n", lines);
    }

    public static void SendRoleList(ChatController c, string text, bool moderator)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (string.IsNullOrEmpty(RoleList()) || Utils.isHideNSeek) return;

        if (text.Length <= 120)
        {
            if (moderator) Utils.ModeratorChatCommand(text, "", false);
            else Utils.ChatCommand(c, text, "", false);
            return;
        }

        int splitIndex = text.LastIndexOf('\n', 120);

        if (splitIndex <= 0) splitIndex = 120;

        string first = text[..splitIndex];
        string second = text[(splitIndex + 1)..];

        if (moderator) Utils.ModeratorChatCommand(first, second, true);
        else Utils.ChatCommand(c, first, second, true);
    }

    public static bool HandlingRoleMessages = false;
    public static void SendRoleMessages()
    {
        if (!AmongUsClient.Instance.AmHost || Options.Gamemode.GetValue() > 1 || Utils.isHideNSeek) return;

        if (string.IsNullOrEmpty(RoleList()) || PlayerControl.LocalPlayer.Data.IsDead)
        {
            HandlingRoleMessages = false;
            return;
        }

        HandlingRoleMessages = true;

        SendRoleList(DestroyableSingleton<HudManager>.Instance.Chat, AbilityManagement.RoleList(), true);

        new LateTask(() => 
        {
            if (Utils.InGame)
            {
                Utils.ChatCommand(DestroyableSingleton<HudManager>.Instance.Chat, "Custom Abilities are on!\n\nIf you have one of the roles above, you also have the Ability next to it.", "", false);
            }
            else
            {
                Logger.Info("Role sending was forcefully canceled. This should not happen.", "SendRoleMessages");
            }

            HandlingRoleMessages = false;

        }, 6.6f, "SendRoleMessage");
    }
}
/*
public static class CustomRoleManagement
{
    public static Dictionary<byte, string> PlayerRoles = new Dictionary<byte, string>();
    private static readonly System.Random random = new System.Random();

    public static string GetRole(byte playerId)
    {
        PlayerRoles.TryGetValue(playerId, out string role);
        return role ?? "None";
    }

    public static void AssignRoles()
    {
        if (Options.Gamemode.GetValue() > 1) return;

        // crewmateRoles = assigned with crewmate role base
        // impostorRoles = assigned with impostor role base
        List<(string roleName, int percentage)> crewmateRoles = new()
        {
            ("Jester", Options.JesterPerc.GetInt()),
            ("Mayor", Options.MayorPerc.GetInt())
        };

        List<(string roleName, int percentage)> impostorRoles = new()
        {

        };

        List<PlayerControl> availablePlayers = new();
        foreach (var player in PlayerControl.AllPlayerControls) availablePlayers.Add(player);
        availablePlayers = availablePlayers.OrderBy(x => random.Next()).ToList();
        PlayerRoles.Clear();

        HashSet<string> assignedRoles = new HashSet<string>();
        HashSet<string> attemptedRoles = new HashSet<string>();

        foreach (var player in availablePlayers)
        {
            bool isCrewmate = !player.Data.Role.IsImpostor;
            var rolesToAssign = isCrewmate ? crewmateRoles : impostorRoles;

            foreach (var (roleName, percentage) in rolesToAssign)
            {
                if (attemptedRoles.Contains(roleName) || percentage == 0) continue;

                attemptedRoles.Add(roleName);

                int randomValue = random.Next(0, 101);
                Logger.Info($"{roleName}, Value: {randomValue}, Percentage: {percentage}", "StartGameCustomRole1");

                if (randomValue > percentage) continue;
                if (PlayerRoles.ContainsKey(player.PlayerId)) continue;

                PlayerRoles[player.PlayerId] = roleName;
                assignedRoles.Add(roleName);

                Logger.Info($"({player.PlayerId}) {player.Data.PlayerName} -> {roleName}", "StartGameCustomRole2");
                break;
            }
        }
    }

    public static string TD(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

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

    public static string GetActiveRoles()
    {
        var crewmateRoles = new List<string>();
        var neutralRoles = new List<string>();
        var impostorRoles = new List<string>();
        var lines = new List<string>();

        if (Options.MayorPerc.GetInt() > 1) crewmateRoles.Add(Translator.Get("Mayor") + TD(Options.MayorPerc.GetInt().ToString()) + "%");

        if (Options.JesterPerc.GetInt() > 1) neutralRoles.Add(Translator.Get("Jester") + TD(Options.JesterPerc.GetInt().ToString()) + "%");

        void AddCategory(string header, List<string> roles)
        {
            if (roles.Count == 0) return;

            if (lines.Count > 0) lines.Add("");

            lines.Add(header);
            lines.AddRange(roles);
        }

        AddCategory("Crewmate:", crewmateRoles);
        AddCategory("Neutral:", neutralRoles);
        AddCategory("Impostor:", impostorRoles);

        return lines.Count > 0 ? string.Join("\n", lines) : string.Empty;
    }

    public static bool HandlingRoleMessages = false;
    private static int PendingRoleMessages = 0;
    public static void SendRoleMessages(Dictionary<string, string> roleMessages)
    {
        if (PlayerRoles.Count == 0 || PlayerControl.LocalPlayer.Data.IsDead)
        {
            HandlingRoleMessages = false;
            PendingRoleMessages = 0;
            return;
        }

        HashSet<string> sentRoles = new HashSet<string>();

        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        float delay = 2.2f;

        PendingRoleMessages = 0;
        HandlingRoleMessages = true;

        PlayerControl.LocalPlayer.RpcSendChat($"{Translator.Get("customRoleAnnouncement")}");

        foreach (var player in players)
        {
            PlayerRoles.TryGetValue(player.PlayerId, out string role);

            if (string.IsNullOrEmpty(role)) continue;
            if (!roleMessages.ContainsKey(role)) continue;
            if (sentRoles.Contains(role)) continue;

            PendingRoleMessages++;
            new LateTask(() => 
            {
                if (Utils.InGame)
                {
                    Utils.SendPrivateMessage(player, roleMessages[role]);
                }
                else
                {
                    Logger.Info("Role sending was forcefully canceled. This should not happen.", "SendRoleMessages");
                }

                PendingRoleMessages--;

                if (PendingRoleMessages <= 0)
                {
                    PendingRoleMessages = 0;
                    HandlingRoleMessages = false;
                    sentRoles.Clear();
                }
            }, delay, "SendRoleMessage");

            sentRoles.Add(role);
            delay += 2.2f;
        }
    }

    public static string PlayerToCustomRole()
    {
        if (CustomRoleManagement.PlayerRoles.Count == 0) return string.Empty;

        Dictionary<string, List<string>> roleToPlayers = new Dictionary<string, List<string>>();

        foreach (var kvp in CustomRoleManagement.PlayerRoles)
        {
            byte playerId = kvp.Key;
            string role = kvp.Value;

            PlayerControl pc = null;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId == playerId)
                {
                    pc = p;
                    break;
                }
            }

            if (pc == null) continue;

            if (!roleToPlayers.ContainsKey(role)) roleToPlayers[role] = new List<string>();
            roleToPlayers[role].Add(pc.Data.PlayerName);
        }

        if (roleToPlayers.Count == 0) return string.Empty;

        var lines = new List<string>();

        foreach (var entry in roleToPlayers)
        {
            string players = string.Join(", ", entry.Value);
            lines.Add($"{entry.Key}: {players}");
        }
        return string.Join("\n", lines);
    }
}
*/