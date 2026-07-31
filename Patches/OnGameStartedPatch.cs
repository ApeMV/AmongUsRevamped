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
        Logger.Info($" Gamemode: {Options.Gamemode.GetString()}", "StartGame");
        Logger.Info($" Players: {PlayerControl.AllPlayerControls.Count}", "StartGame");
        Logger.Info($" Map: {(MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId}", "StartGame");

        if (Main.GM.Value)
        {
            Logger.Info($" Game Master Enabled", "StartGame");      
        }

        NormalGameEndChecker.imps.Clear();
        NormalGameEndChecker.LastWinReason = "";

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
class PlayerControlSetRolePatch
{
    public static bool FirstAssign;
    private static readonly HashSet<byte> ProcessedPlayers = new();
    public static HashSet<byte> Seekers = new();
    public static HashSet<byte> manualSeekers = new();
    private static readonly System.Random rand = new System.Random();

    public static bool Prefix(PlayerControl __instance, ref RoleTypes roleType, ref bool canOverrideRole)
    {
        if (!FirstAssign || !AmongUsClient.Instance.AmHost) return true;

        if (!ProcessedPlayers.Add(__instance.PlayerId)) return true;

        canOverrideRole = false;

        if (Main.GM.Value && __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            roleType = RoleTypes.Crewmate;
            OnGameStartPatch.ScheduleExile = true;
        }

        if (Utils.isHideNSeek && Seekers.Count() == 0 && Options.NumSeekers.GetInt() > 0)
        {
            int seekersCount = Options.NumSeekers.GetInt();

            if (manualSeekers.Count > seekersCount) seekersCount = manualSeekers.Count;

            var candidates = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (!Main.GM.Value || p != PlayerControl.LocalPlayer) candidates.Add(p);
            }

            if (manualSeekers.Count == 0)
            {
                seekersCount = Math.Min(seekersCount, candidates.Count);
            }
            else
            {
                seekersCount = Math.Min(seekersCount, candidates.Count + manualSeekers.Count);                
            }

            foreach (byte id in manualSeekers)
            {
                if (Seekers.Count >= seekersCount) break;
                Seekers.Add(id);
            }

            candidates.RemoveAll(p => manualSeekers.Contains(p.PlayerId));

            for (int j = candidates.Count - 1; j > 0; j--)
            {
                int k = rand.Next(j + 1);
                (candidates[j], candidates[k]) = (candidates[k], candidates[j]);
            }

            foreach (var p in candidates)
            {
                if (Seekers.Count >= seekersCount) break;

                Seekers.Add(p.PlayerId);
            }
        }

        if (Utils.isHideNSeek && Options.NumSeekers.GetInt() > 0)
        {
            if (Seekers.Contains(__instance.PlayerId)) roleType = RoleTypes.Impostor;
            else roleType = RoleTypes.Engineer;
        }

        if (Options.Gamemode.GetValue() == 2 && !Utils.isHideNSeek)
        {
            if (Options.EngineerMode.GetBool()) roleType = RoleTypes.Engineer;
            else roleType = RoleTypes.Crewmate;
        }

        if (Options.Gamemode.GetValue() == 1 && !Utils.isHideNSeek)
        {
            if (roleType == RoleTypes.Impostor || roleType == RoleTypes.Phantom || roleType == RoleTypes.Viper) roleType = RoleTypes.Shapeshifter;
        }

        if (ProcessedPlayers.Count >= PlayerControl.AllPlayerControls.Count)
        {
            Seekers.Clear();
            ProcessedPlayers.Clear();
            FirstAssign = false;

            Logger.Info("PCSRP successful", "RoleManaging");
        }
    
        return true;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.OnGameStart))]
internal static class OnGameStartPatch
{
    public static bool ScheduleExile;
    public static void Postfix()
    {
        if (AmongUsClient.Instance.AmHost && Options.Gamemode.GetValue() == 1 && Options.SNSChatInGame.GetBool())
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            if (MeetingHud.Instance != null) MeetingHud.Instance.RpcClose();
        }

        if (ScheduleExile)
        {
            Logger.Info($" Game Master Successful", "StartGame");
            PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.CrewmateGhost);
            ScheduleExile = false;
        }

        foreach (var p in PlayerControl.AllPlayerControls)
        {
            Utils.StoredRoleText[p.PlayerId] = Utils.GetRoleText(p);
        }
    }
}