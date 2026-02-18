using AmongUs.GameOptions;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

public static class CustomRoleManagement
{
    public static Dictionary<byte, string> PlayerRoles = new Dictionary<byte, string>();
    private static readonly System.Random random = new System.Random();

    public static void AssignRoles()
    {
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
                if (attemptedRoles.Contains(roleName)) continue;

                attemptedRoles.Add(roleName);

                int randomValue = random.Next(0, 101);
                Logger.Info($"{roleName}, Value: {randomValue}, Percentage: {percentage}", "StartGameCustomRole1");

                if (randomValue >= percentage) continue;
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
        HashSet<string> sentRoles = new HashSet<string>();

        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        float delay = 0f;

        PendingRoleMessages = 0;
        HandlingRoleMessages = false;
        foreach (var player in players)
        {
            if (!PlayerRoles.TryGetValue(player.PlayerId, out string role)) continue;
            if (!roleMessages.ContainsKey(role)) continue;
            if (sentRoles.Contains(role)) continue;

            PendingRoleMessages++;
            new LateTask(() => 
            {
                Utils.SendPrivateMessage(player, roleMessages[role]);
                PendingRoleMessages--;

                if (PendingRoleMessages <= 0)
                {
                    PendingRoleMessages = 0;
                    HandlingRoleMessages = false;
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