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
        List<(string roleName, int percentage)> crewmateRoles = new List<(string, int)>
        {
            ("Jester", Options.JesterPerc.GetInt()),
            ("Mayor", Options.MayorPerc.GetInt())
        };

        List<(string roleName, int percentage)> impostorRoles = new List<(string, int)>
        {

        };

        List<PlayerControl> availablePlayers = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            availablePlayers.Add(player);
        }

        availablePlayers = availablePlayers.OrderBy(x => random.Next()).ToList();
        PlayerRoles.Clear();
        HashSet<string> assignedRoles = new HashSet<string>();

        foreach (var player in availablePlayers)
        {
            bool isCrewmate = !player.Data.Role.IsImpostor;

            List<(string roleName, int percentage)> rolesToAssign = isCrewmate ? crewmateRoles : impostorRoles;

            foreach (var (roleName, percentage) in rolesToAssign)
            {
                if (assignedRoles.Contains(roleName))
                    continue;

                int randomValue = random.Next(0, 101);

                Logger.Info($"{roleName}, Value: {randomValue}, Percentage: {percentage}", "StartGameCustomRole1");

                if (randomValue <= percentage && !PlayerRoles.ContainsKey(player.PlayerId))
                {
                    PlayerRoles[player.PlayerId] = roleName;
                    assignedRoles.Add(roleName);
                    Logger.Info($"({player.PlayerId}) {player.Data.PlayerName} -> {roleName}", "StartGameCustomRole2");

                    break;
                }
            }
        }
    }

    public static string GetActiveRoles()
    {
        var crewmateRoles = new List<string>();
        var neutralRoles = new List<string>();
        var impostorRoles = new List<string>();
        var lines = new List<string>();

        if (Options.MayorPerc.GetInt() > 1) crewmateRoles.Add($"Mayor ({Options.MayorPerc.GetInt()}%)");

        if (Options.JesterPerc.GetInt() > 1) neutralRoles.Add($"Jester ({Options.JesterPerc.GetInt()})%");

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

    public static void SendRoleMessages(Dictionary<string, string> roleMessages)
    {
        HashSet<string> sentRoles = new HashSet<string>();

        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        float delay = 0f;

        foreach (var player in players)
        {
            if (!PlayerRoles.TryGetValue(player.PlayerId, out string role)) continue;
            if (!roleMessages.ContainsKey(role)) continue;
            if (sentRoles.Contains(role)) continue;

            new LateTask(() => 
            {
                Utils.SendPrivateMessage(player, roleMessages[role]);
            }, delay, "SendRoleMessage");

            sentRoles.Add(role);
            delay += 2.2f;
        }
    }
}