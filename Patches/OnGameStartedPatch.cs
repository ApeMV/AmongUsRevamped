using AmongUs.GameOptions;
using System;
using InnerNet;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
internal class CoStartGamePatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Logger.Info(" -------- GAME STARTED --------", "StartGame");
        Logger.Info($" Gamemode: {Options.Gamemode.GetValue()}", "StartGame");
        Logger.Info($" Players: {PlayerControl.AllPlayerControls.Count}", "StartGame");

        if ((Options.Gamemode.GetValue() == 0 || Options.Gamemode.GetValue() == 1) && !Utils.isHideNSeek)
        {
            CustomRoleManagement.AssignRoles();
        }
        NormalGameEndChecker.imps.Clear();
        NormalGameEndChecker.LastWinReason = "";

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class PlayerControlSetRolePatch
{
    private static int i;
    public static bool FirstAssign;
    private static HashSet<byte> Seekers = new();
    private static readonly System.Random rand = new System.Random();

    public static bool Prefix(PlayerControl __instance, ref RoleTypes roleType, ref bool canOverrideRole)
    {
        if (!FirstAssign || !AmongUsClient.Instance.AmHost) return true;

        canOverrideRole = false;

        if (Main.GM.Value && __instance == PlayerControl.LocalPlayer)
        {
            roleType = RoleTypes.CrewmateGhost;
            return true;
        }

        if (CustomRoleManagement.PlayerRoles.TryGetValue(__instance.PlayerId, out var role) && role == "Mayor" && Options.Gamemode.GetValue() < 2)
        {
            if (Options.MayorVentToMeeting.GetBool())
            {
                roleType = RoleTypes.Engineer;
                Logger.Info($"{__instance.Data.PlayerName} is Engineer due to being Mayor", "SetRolePatch");
            }
            else
            {
                roleType = RoleTypes.Crewmate;
            }
        }
        if (CustomRoleManagement.PlayerRoles.TryGetValue(__instance.PlayerId, out var r) && r == "Jester" && Options.Gamemode.GetValue() < 2)
        {
            if (Options.JesterCanVent.GetBool())
            {
                roleType = RoleTypes.Engineer;
                Logger.Info($"{__instance.Data.PlayerName} is Engineer due to being Jester", "SetRolePatch");
            }
            else
            {
                roleType = RoleTypes.Crewmate;
            }
        }

        if (Utils.isHideNSeek && i == 0)
        {
            int seekersCount = Options.NumSeekers.GetInt();

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (!Main.GM.Value || p != PlayerControl.LocalPlayer) candidates.Add(p);
            }

            seekersCount = Math.Min(seekersCount, candidates.Count);

            for (int j = candidates.Count - 1; j > 0; j--)
            {
                int k = rand.Next(j + 1);
                (candidates[j], candidates[k]) = (candidates[k], candidates[j]);
            }

            for (int j = 0; j < seekersCount; j++)
            {
                Seekers.Add(candidates[j].PlayerId);
            }
        }

        if (Utils.isHideNSeek)
        {
            if (Seekers.Contains(__instance.PlayerId)) roleType = RoleTypes.Impostor;
            else roleType = RoleTypes.Engineer;
        }

        if (Options.Gamemode.GetValue() == 3 && !Utils.isHideNSeek)
        {
            if (__instance == PlayerControl.LocalPlayer && Main.GM.Value)
            {
                PlayerControl.LocalPlayer.myTasks.Clear();
                return true;
            }
            roleType = RoleTypes.Crewmate;
        }

        i++;
        if (i >= PlayerControl.AllPlayerControls.Count)
        {
            Seekers.Clear();
            FirstAssign = false;
            i = 0;
            Logger.Info("PCSRP successful", "RoleManaging");
        }

        return true;
    }
}